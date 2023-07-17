namespace Simple.Http.Orchestrator;

public record Activity(Guid Id, IEnumerable<Request> Requests);
