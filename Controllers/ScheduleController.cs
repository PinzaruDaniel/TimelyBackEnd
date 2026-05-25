using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using TimelyBackEnd.DTOs.Schedule;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly IHttpClientFactory _httpClientFactory;

    public ScheduleController(IScheduleService scheduleService, IHttpClientFactory httpClientFactory)
    {
        _scheduleService = scheduleService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> AddSchedule([FromBody] CreateScheduleDto dto)
    {
        await _scheduleService.AddScheduleAsync(dto);
        return Ok(new { message = "Schedule added successfully" });
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetScheduleByGroup(Guid groupId, [FromQuery] string? weekParity = null)
    {
        var isEvenWeek = ResolveWeekParity(weekParity);

        var schedule = await _scheduleService.GetScheduleByGroupAsync(groupId);
        if (schedule == null)
        {
            return Ok(BuildMockScheduleResponse("Unknown Group", groupId, isEvenWeek));
        }

        if (schedule.ScheduleEntries.Count == 0)
        {
            return Ok(BuildMockScheduleResponse(schedule.GroupName, schedule.GroupId, isEvenWeek));
        }

        var response = BuildScheduleResponse(schedule.GroupName, schedule.GroupId, schedule.ScheduleEntries, isEvenWeek);
        return Ok(response);
    }

    [HttpPost("upload-image")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<IActionResult> UploadScheduleImage([FromForm] ScheduleImageUploadDto dto, CancellationToken cancellationToken)
    {
        if (dto.Image == null || dto.Image.Length == 0)
        {
            return BadRequest(new { error = "Image file is required." });
        }

        // We do not store the uploaded image; it is streamed directly to the extraction service.

        // TODO: Replace with the actual schedule extraction API URL
        const string extractionApiUrl = "https://TODO_ADD_EXTRACTION_API_URL_HERE";

        string json;
        if (extractionApiUrl.Contains("TODO", StringComparison.OrdinalIgnoreCase))
        {
            // No extraction API configured yet, fall back to mock data.
            json = MockScheduleJson;
        }
        else
        {
            var client = _httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();
            await using var imageStream = dto.Image.OpenReadStream();
            var streamContent = new StreamContent(imageStream);
            content.Add(streamContent, "file", dto.Image.FileName);
            content.Add(new StringContent(dto.GroupId.ToString()), "groupId");

            HttpResponseMessage extractionResponse;
            try
            {
                extractionResponse = await client.PostAsync(extractionApiUrl, content, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { error = $"Failed to contact extraction service: {ex.Message}" });
            }

            if (!extractionResponse.IsSuccessStatusCode)
            {
                var failure = await extractionResponse.Content.ReadAsStringAsync(cancellationToken);
                return StatusCode((int)extractionResponse.StatusCode, new { error = "Schedule extraction failed.", details = failure });
            }

            json = await extractionResponse.Content.ReadAsStringAsync(cancellationToken);
        }

        var extractedSchedule = ParseScheduleFromJson(json);
        if (extractedSchedule.ScheduleEntries.Count == 0)
        {
            return BadRequest(new { error = "Extraction API returned invalid or empty schedule data." });
        }

        extractedSchedule.GroupId = dto.GroupId;
        await _scheduleService.AddScheduleAsync(extractedSchedule);

        return Ok(new { message = "Schedule extracted and saved successfully." });
    }

    private static readonly IReadOnlyDictionary<string, int> DayOrder = new ReadOnlyDictionary<string, int>(
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Luni"] = 1,
            ["Marți"] = 2,
            ["Marti"] = 2,
            ["Miercuri"] = 3,
            ["Joi"] = 4,
            ["Vineri"] = 5,
            ["Sâmbătă"] = 6,
            ["Sambata"] = 6,
            ["Duminică"] = 7,
            ["Duminica"] = 7
        });

    private static CreateScheduleDto ParseScheduleFromJson(string json)
    {
        var schedule = new CreateScheduleDto();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return schedule;

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.NameEquals("group") || property.NameEquals("groupId") || property.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in property.Value.EnumerateArray())
            {
                var entry = new CreateScheduleEntryDto
                {
                    DayOfWeek = property.Name,
                    Time = GetPropertyOrDefault(item, "time"),
                    Subject = GetPropertyOrDefault(item, "subject"),
                    Teacher = GetPropertyOrDefault(item, "teacher"),
                    Room = GetPropertyOrDefault(item, "room"),
                    Period = GetPropertyOrDefault(item, "period", "every_week")
                };

                if (!string.IsNullOrWhiteSpace(entry.Subject))
                {
                    schedule.ScheduleEntries.Add(entry);
                }
            }
        }

        return schedule;
    }

    private static string GetPropertyOrDefault(JsonElement element, string propertyName, string defaultValue = "")
    {
        if (element.ValueKind != JsonValueKind.Object)
            return defaultValue;

        if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? defaultValue;
        }

        return defaultValue;
    }

    private static Dictionary<string, object?> BuildScheduleResponse(string groupName, object groupIdentifier, IEnumerable<ScheduleEntryDto> entries, bool isEvenWeek)
    {
        var response = new Dictionary<string, object?>
        {
            ["group"] = groupName,
            ["groupId"] = groupIdentifier
        };

        var filteredEntries = entries
            .Where(e => ShouldIncludeThisWeek(e.Period, isEvenWeek));

        var groupedEntries = filteredEntries
            .GroupBy(e => e.DayOfWeek)
            .OrderBy(g => DayOrder.TryGetValue(g.Key, out var order) ? order : int.MaxValue);

        foreach (var dayGroup in groupedEntries)
        {
            var dayEntries = dayGroup
                .OrderBy(e => ParseStartTime(e.Time))
                .Select(e => new ScheduleDayEntryDto(e.Time, e.Subject, e.Teacher, e.Room, e.Period))
                .ToList();

            response[dayGroup.Key] = dayEntries;
        }

        return response;
    }

    private static Dictionary<string, object?> BuildMockScheduleResponse(string groupName, object groupId, bool isEvenWeek)
    {
        var json = GetMockScheduleJsonForGroup(groupName, groupId);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var resolvedGroupName = root.TryGetProperty("group", out var groupProp)
            ? groupProp.GetString() ?? groupName
            : groupName;

        object groupIdValue = groupId;
        if (root.TryGetProperty("groupId", out var groupIdProp))
        {
            if (groupIdProp.ValueKind == JsonValueKind.Number && groupIdProp.TryGetInt64(out var idLong))
            {
                groupIdValue = idLong;
            }
            else if (groupIdProp.ValueKind == JsonValueKind.String)
            {
                groupIdValue = groupIdProp.GetString() ?? groupId;
            }
        }

        var entries = ParseScheduleFromJson(json).ScheduleEntries
            .Where(e => ShouldIncludeThisWeek(e.Period, isEvenWeek))
            .Select(e => new ScheduleEntryDto(e.DayOfWeek, e.Time, e.Subject, e.Teacher, e.Room, e.Period));

        return BuildScheduleResponse(resolvedGroupName, groupIdValue, entries, isEvenWeek);
    }

    private static string GetMockScheduleJsonForGroup(string groupName, object groupId)
    {
        if (string.Equals(groupName, MockScheduleGroupName, StringComparison.OrdinalIgnoreCase))
        {
            return MockScheduleJson;
        }

        return BuildEmptyScheduleJson(groupName, groupId);
    }

    private static string BuildEmptyScheduleJson(string groupName, object groupId)
    {
        var payload = new Dictionary<string, object?>
        {
            ["group"] = groupName,
            ["groupId"] = groupId
        };

        return JsonSerializer.Serialize(payload);
    }

    private const string MockScheduleGroupName = "PAPP-231";

    private const string MockScheduleJson = @"{
    ""group"": ""PAPP-231"",
    ""groupId"": ""cb41031e-f64e-484b-a38b-3ac7c6995c38"",
    ""Luni"": [
        {
            ""time"": ""11:30-13:00"",
            ""subject"": ""Sisteme de gestiune a bazelor de date"",
            ""teacher"": ""A. Pintea"",
            ""room"": ""213"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""13:30-15:00"",
            ""subject"": ""Fizica"",
            ""teacher"": ""A. Mihălachi"",
            ""room"": ""213"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""15:15-16:45"",
            ""subject"": ""EPS"",
            ""teacher"": ""M. Ciobanu"",
            ""room"": ""I-04"",
            ""period"": ""odd_week""
        },
        {
            ""time"": ""15:15-16:45"",
            ""subject"": ""Matematica"",
            ""teacher"": ""V. Guci"",
            ""room"": ""I-04"",
            ""period"": ""even_week""
        }
    ],
    ""Marti"": [
        {
            ""time"": ""9:45-11:15"",
            ""subject"": ""Limba franceză"",
            ""teacher"": ""L. Bivol"",
            ""room"": ""108"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""11:30-13:00"",
            ""subject"": ""Educația fizică"",
            ""teacher"": """",
            ""room"": """",
            ""period"": ""every_week""
        },
        {
            ""time"": ""13:30-15:00"",
            ""subject"": ""Limba română"",
            ""teacher"": ""T. Zaorodniuc"",
            ""room"": ""308"",
            ""period"": ""every_week""
        }
    ],
    ""Miercuri"": [
        {
            ""time"": ""8:00-9:30"",
            ""subject"": ""Bazele legislației"",
            ""teacher"": ""R. Bîrlădeanu"",
            ""room"": ""213"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""9:45-11:15"",
            ""subject"": ""Matematica"",
            ""teacher"": ""V. Guci"",
            ""room"": ""213"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""11:30-13:00"",
            ""subject"": ""Informatica"",
            ""teacher"": ""A. Pintea"",
            ""room"": ""213"",
            ""period"": ""every_week""
        }
    ],
    ""Joi"": [
        {
            ""time"": ""8:00-9:30"",
            ""subject"": ""Matematica"",
            ""teacher"": ""V. Guci"",
            ""room"": ""308"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""9:45-11:15"",
            ""subject"": ""Limba engleză"",
            ""teacher"": ""D. Leahu, L. Samson"",
            ""room"": ""219, 305"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""11:30-13:00"",
            ""subject"": ""Geografia"",
            ""teacher"": ""S. Antoniuc"",
            ""room"": ""104"",
            ""period"": ""odd_week""
        },
        {
            ""time"": ""11:30-13:00"",
            ""subject"": ""Chimia"",
            ""teacher"": ""E. Stratulat"",
            ""room"": ""104"",
            ""period"": ""even_week""
        }
    ],
    ""Vineri"": [
        {
            ""time"": ""8:00-9:30"",
            ""subject"": ""Sisteme de gestiune a bazelor de date"",
            ""teacher"": ""A. Pintea"",
            ""room"": ""307"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""9:45-11:15"",
            ""subject"": ""Utilizarea sistemelor de operare în rețea"",
            ""teacher"": ""L. Peca"",
            ""room"": ""313"",
            ""period"": ""every_week""
        },
        {
            ""time"": ""11:30-13:00"",
            ""subject"": ""Istoria"",
            ""teacher"": ""R. Bîrlădeanu"",
            ""room"": ""109"",
            ""period"": ""odd_week""
        },
        {
            ""time"": ""13:30-15:00"",
            ""subject"": ""Limba română"",
            ""teacher"": ""T. Zaorodniuc"",
            ""room"": ""109"",
            ""period"": ""even_week""
        }
    ]
}";

    private static bool ShouldIncludeThisWeek(string period, bool isEvenWeek)
    {
        if (string.IsNullOrWhiteSpace(period))
        {
            return true;
        }

        return period.ToLowerInvariant() switch
        {
            "every_week" => true,
            "even_week" => !isEvenWeek,
            "odd_week" => isEvenWeek,
            _ => true
        };
    }

    private static bool IsEvenWeek(DateTime date)
    {
        // ISO 8601 week number
        var calendar = CultureInfo.InvariantCulture.Calendar;
        var week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return week % 2 == 0;
    }

    private static bool ResolveWeekParity(string? weekParity)
    {
        if (!string.IsNullOrWhiteSpace(weekParity))
        {
            var normalized = weekParity.Trim().ToLowerInvariant();
            if (normalized is "even" or "e")
                return true;
            if (normalized is "odd" or "o")
                return false;
        }

        // Fallback to computed week parity (current assumption: this week is even)
        return IsEvenWeek(DateTime.UtcNow);
    }

    private static TimeSpan ParseStartTime(string timeRange)
    {
        if (string.IsNullOrWhiteSpace(timeRange))
        {
            return TimeSpan.MaxValue;
        }

        var dashIndex = timeRange.IndexOf('-');
        var startPart = dashIndex > 0 ? timeRange[..dashIndex] : timeRange;

        if (TimeSpan.TryParse(startPart, out var start))
        {
            return start;
        }

        return TimeSpan.MaxValue;
    }
}