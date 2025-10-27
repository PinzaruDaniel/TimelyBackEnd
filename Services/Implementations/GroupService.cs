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
        var group = new Group { Name = dto.Name, SchoolName = dto.SchoolName };
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return new GroupDto(group.Id, group.Name, group.SchoolName);
    }

    public async Task<List<GroupDto>> GetAllGroupsAsync()
    {
        return await _context.Groups
            .Select(g => new GroupDto(g.Id, g.Name, g.SchoolName))
            .ToListAsync();
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid id)
    {
        var g = await _context.Groups.FindAsync(id);
        return g == null ? null : new GroupDto(g.Id, g.Name, g.SchoolName);
    }
}