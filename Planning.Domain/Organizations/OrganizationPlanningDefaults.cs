namespace Planning.Domain.Organizations;

public static class OrganizationPlanningDefaults
{
    public static IReadOnlyList<ImportantWorkTime> ImportantWorkTimes { get; } =
    [
        new(label: null, startTime: new TimeOnly(6, 0)),
        new(label: null, startTime: new TimeOnly(9, 0)),
        new(label: null, startTime: new TimeOnly(13, 0)),
        new(label: null, startTime: new TimeOnly(17, 0)),
        new(label: null, startTime: new TimeOnly(21, 0)),
    ];
}
