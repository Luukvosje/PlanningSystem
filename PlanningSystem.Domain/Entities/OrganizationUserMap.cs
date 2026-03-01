namespace PlanningSystem.Domain.Entities;

public class OrganizationUserMap
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public OrganizationRole Role { get; set; }
}

public enum OrganizationRole
{
    Admin,
    Manager,
    Member,
    Viewer
}
