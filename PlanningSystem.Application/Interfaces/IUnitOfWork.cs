namespace PlanningSystem.Application.Interfaces;

public interface IUnitOfWork
{
    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default);
}
