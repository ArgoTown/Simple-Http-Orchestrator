namespace Simple.Http.Orchestrator.Contracts;

public class Activity
{
    public Guid Id { get; set; }
    public IEnumerable<Request> Requests { get; set; } = Enumerable.Empty<Request>();
}

