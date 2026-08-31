namespace Planning.Domain.Enums;

/// <summary>
/// Where a request stands. Rejected is terminal and Approved is terminal: a decision is not
/// revisited, the employee submits a new request instead.
/// </summary>
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
}
