
namespace Planning.Application.Customers;



public sealed record CreateCustomerRequest(

    string Name,

    string Email,

    string? Address);



public sealed record UpdateCustomerRequest(

    string Name,

    string Email,

    string? Address);



public sealed record CustomerResponse(

    Guid Id,

    Guid OrganizationId,

    string Name,

    string Email,

    string? Address,

    DateTime CreatedAtUtc,

    DateTime UpdatedAtUtc);

