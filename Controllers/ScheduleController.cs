using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimelyBackEnd.DTOs.Schedule;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> AddSchedule([FromBody] CreateScheduleDto dto)
    {
        await _scheduleService.AddScheduleAsync(dto);
        return Ok(new { message = "Schedule added successfully" });
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetScheduleByGroup(Guid groupId)
    {
        var schedule = await _scheduleService.GetScheduleByGroupAsync(groupId);
        return schedule == null ? NotFound() : Ok(schedule);
    }
}