using System.ComponentModel.DataAnnotations;

namespace TimelyBackEnd.DTOs.Group
{
    public class CreateGroupDto
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string SchoolName { get; set; } = string.Empty;

        public bool IsPrivate { get; set; } = false;
        public Guid? OwnerId { get; set; } // only for private groups
    }
}