namespace Planning.Domain.Organizations;

public static class OrganizationPlanningDefaults
{
    public static IReadOnlyList<TimeOnly> ImportantWorkTimes { get; } =
    [
        new(6, 0),
        new(9, 0),
        new(13, 0),
        new(17, 0),
        new(21, 0),
    ];
}
