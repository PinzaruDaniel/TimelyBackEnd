using Microsoft.AspNetCore.Http;

namespace TimelyBackEnd.DTOs.Schedule;

public class ScheduleImageUploadDto
{
    public Guid GroupId { get; set; }
    public IFormFile? Image { get; set; }
}

