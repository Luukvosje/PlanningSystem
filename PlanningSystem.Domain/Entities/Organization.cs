using PlanningSystem.Domain.Exceptions;

namespace PlanningSystem.Domain.Entities;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<OrganizationUserMap> OrganizationUserMaps { get; set; } = new List<OrganizationUserMap>();

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Organization name is required");

        Name = name.Trim();
    }
}
