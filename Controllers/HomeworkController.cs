using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimelyBackEnd.DTOs.Homework;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeworkController : ControllerBase
{
    private readonly IHomeworkService _homeworkService;

    public HomeworkController(IHomeworkService homeworkService)
    {
        _homeworkService = homeworkService;
    }

    [HttpPost("add")]
    [Authorize]
    public async Task<IActionResult> AddHomework([FromBody] CreateHomeworkDto dto)
    {
        var homework = await _homeworkService.AddHomeworkAsync(dto);
        return Ok(homework);
    }

    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetHomeworksForGroup(Guid groupId)
    {
        var homeworks = await _homeworkService.GetHomeworksForGroupAsync(groupId);
        return Ok(homeworks);
    }

    [HttpPut("{id}/done")]
    [Authorize]
    public async Task<IActionResult> MarkHomeworkDone(Guid id)
    {
        await _homeworkService.MarkHomeworkDoneAsync(id);
        return Ok(new { message = "Homework marked as done." });
    }
}