using System.Text.Json.Serialization;

namespace TimelyBackEnd.DTOs.Schedule;

public record ScheduleDayEntryDto(
    [property: JsonPropertyName("time")] string Time,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("teacher")] string Teacher,
    [property: JsonPropertyName("room")] string Room,
    [property: JsonPropertyName("period")] string Period
);

