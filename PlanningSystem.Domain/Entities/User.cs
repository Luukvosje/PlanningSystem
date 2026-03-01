using System.Text.Json.Serialization;

namespace PlanningSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string? Pwd { get; set; }
}
