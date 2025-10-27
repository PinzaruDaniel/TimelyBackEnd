using TimelyBackEnd.DTOs.Homework;

namespace TimelyBackEnd.Services.Interfaces;

public interface IHomeworkService
{
    Task<HomeworkDto> AddHomeworkAsync(CreateHomeworkDto dto, Guid userId);
    Task<List<HomeworkDto>> GetHomeworksForGroupAsync(Guid groupId);
    Task MarkHomeworkDoneAsync(Guid homeworkId);
} 