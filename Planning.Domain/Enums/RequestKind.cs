namespace Planning.Domain.Enums;

/// <summary>
/// What kind of thing is being requested. Deliberately its own enum rather than a projection of
/// <see cref="AvailabilityRuleType"/>: a shift swap will be a different entity entirely, and the
/// request feed has to be able to name it without that entity leaking into the availability model.
/// </summary>
public enum RequestKind
{
    Leave = 0,
    Availability = 1,
}
