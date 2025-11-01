namespace TimelyBackEnd.DTOs.Group
{
    public class JoinGroupDto
    {
        public Guid UserId { get; set; }
        public string InviteCode { get; set; } = string.Empty;
    }
}

