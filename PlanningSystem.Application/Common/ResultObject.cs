namespace PlanningSystem.Application.Common;

public class ResultObject<T>
{
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public bool Success { get; set; }
    public T? Data { get; set; }
}
