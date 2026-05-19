using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimelyBackEnd.DTOs.Group;
using TimelyBackEnd.Services.Interfaces;
using System.Security.Claims;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var userId = GetUserIdOrUnauthorized();
        if (userId == null)
        {
            return Unauthorized();
        }

        dto.OwnerId = userId.Value;
        var group = await _groupService.CreateGroupAsync(dto);
        return Ok(group);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGroups()
    {
        var groups = await _groupService.GetAllGroupsAsync();
        return Ok(groups);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroupById(Guid id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        return group == null ? NotFound() : Ok(group);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto dto)
    {
        var userId = GetUserIdOrUnauthorized();
        if (userId == null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(dto.InviteCode))
        {
            return BadRequest(new { error = "inviteCode is required." });
        }

        try
        {
            var result = await _groupService.JoinGroupByInviteCodeAsync(userId.Value, dto.InviteCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid? GetUserIdOrUnauthorized()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}