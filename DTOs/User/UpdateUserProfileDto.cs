using Microsoft.AspNetCore.Http;

namespace TimelyBackEnd.DTOs.User
{
    public class UpdateUserProfileDto
    {
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Age { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? Country { get; set; }
        public IFormFile? Photo { get; set; }
    }
}

