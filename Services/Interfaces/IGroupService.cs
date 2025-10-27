using TimelyBackEnd.DTOs.Group;

namespace TimelyBackEnd.Services.Interfaces;

public interface IGroupService
{
    Task<GroupDto> CreateGroupAsync(CreateGroupDto dto);
    Task<List<GroupDto>> GetAllGroupsAsync();
    Task<GroupDto?> GetGroupByIdAsync(Guid id);
}