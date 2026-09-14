namespace Planning.Application.Customers;

/// <summary>Fields shared by the create and update requests, so one validator covers both.</summary>
public interface ICustomerRequestFields
{
    string Name { get; }
    string? Email { get; }
    string? Address { get; }
    string? Color { get; }
}

public sealed record CreateCustomerRequest(
    string Name,
    string? Email,
    string? Address,
    string? Color) : ICustomerRequestFields;

public sealed record UpdateCustomerRequest(
    string Name,
    string? Email,
    string? Address,
    string? Color) : ICustomerRequestFields;

public sealed record CustomerResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Email,
    string? Address,
    string Color,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
