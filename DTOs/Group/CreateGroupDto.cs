namespace TimelyBackEnd.DTOs.Group
{
    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public bool IsPrivate { get; set; } = false;
        public Guid? OwnerId { get; set; } // only for private groups
    }
}