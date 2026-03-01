namespace PlanningSystem.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string message) : base(message)
    {
    }

    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.")
    {
    }
}
