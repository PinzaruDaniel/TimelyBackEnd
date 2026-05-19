using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimelyBackEnd.DTOs.Group;
using TimelyBackEnd.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TimelyBackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpPost("create")]
    [AllowAnonymous] //TODO: to uncomment this!!!
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        // For private groups, require authentication and set OwnerId from JWT
        if (dto.IsPrivate)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
            {
                return Unauthorized();
            }
            dto.OwnerId = ownerId;
        }
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
    [AllowAnonymous]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto dto)
    {
        if (dto.UserId == Guid.Empty || string.IsNullOrWhiteSpace(dto.InviteCode))
        {
            return BadRequest(new { error = "userId and inviteCode are required." });
        }

        try
        {
            var result = await _groupService.JoinGroupByInviteCodeAsync(dto.UserId, dto.InviteCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}