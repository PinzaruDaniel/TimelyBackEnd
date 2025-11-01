namespace TimelyBackEnd.DTOs.Group
{
public record GroupDto(Guid Id, string Name, string SchoolName, string InviteCode, bool IsPrivate, Guid? OwnerId, List<Guid> UserIds);}