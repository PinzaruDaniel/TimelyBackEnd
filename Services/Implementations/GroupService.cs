using Microsoft.EntityFrameworkCore;
using TimelyBackEnd.Data;
using TimelyBackEnd.DTOs.Group;
using TimelyBackEnd.Models;
using TimelyBackEnd.Services.Interfaces;

namespace TimelyBackEnd.Services.Implementations;

public class GroupService : IGroupService
{
    private readonly TimelyDbContext _context;

    public GroupService(TimelyDbContext context)
    {
        _context = context;
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new Exception("Group name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.SchoolName))
        {
            throw new Exception("School name is required.");
        }

        if (!dto.OwnerId.HasValue)
        {
            throw new Exception("Owner is required.");
        }

        var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.OwnerId.Value)
            ?? throw new Exception("Owner not found.");

        string? inviteCode = null;
        if (!dto.IsPrivate)
        {
            inviteCode = await GenerateUniqueInviteCodeAsync();
        }

        var group = new Group
        {
            Name = dto.Name,
            SchoolName = dto.SchoolName,
            InviteCode = inviteCode,
            IsPrivate = dto.IsPrivate,
            OwnerId = dto.OwnerId
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        owner.GroupId = group.Id;
        if (!group.Users.Any(u => u.Id == owner.Id))
        {
            group.Users.Add(owner);
        }

        await _context.SaveChangesAsync();

        var userIds = group.Users.Select(u => u.Id).ToList();
        return new GroupDto(group.Id, group.Name, group.SchoolName, group.InviteCode ?? string.Empty, group.IsPrivate, group.OwnerId, userIds);
    }

    public async Task<List<GroupDto>> GetAllGroupsAsync()
    {
        return await _context.Groups
            .Select(g => new GroupDto(
       g.Id, g.Name, g.SchoolName, g.InviteCode ?? string.Empty, g.IsPrivate, g.OwnerId,
       g.Users.Select(u => u.Id).ToList()))
            .ToListAsync();
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid id)
    {
     var g = await _context.Groups.Include(gr => gr.Users).FirstOrDefaultAsync(gr => gr.Id == id);
     return g == null ? null : new GroupDto(g.Id, g.Name, g.SchoolName, g.InviteCode ?? string.Empty, g.IsPrivate, g.OwnerId, g.Users.Select(u => u.Id).ToList());
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // avoid similar-looking
        var random = Random.Shared;
        var buffer = new char[8];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = chars[random.Next(chars.Length)];
        }
        return new string(buffer);
    }

    private async Task<string> GenerateUniqueInviteCodeAsync()
    {
        string code;
        do
        {
            code = GenerateInviteCode();
        } while (await _context.Groups.AnyAsync(g => g.InviteCode == code));
        return code;
    }

    public async Task<GroupDto> JoinGroupByInviteCodeAsync(Guid userId, string inviteCode)
    {
     var group = await _context.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.InviteCode == inviteCode);
        if (group == null)
        {
            throw new Exception("Invalid invite code.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (user.GroupId != group.Id)
        {
            user.GroupId = group.Id;
        }

        if (!group.Users.Any(u => u.Id == user.Id))
        {
            group.Users.Add(user);
        }

        await _context.SaveChangesAsync();

        var userIds = group.Users.Select(u => u.Id).ToList();
     return new GroupDto(group.Id, group.Name, group.SchoolName, group.InviteCode ?? string.Empty, group.IsPrivate, group.OwnerId, userIds);
    }
}