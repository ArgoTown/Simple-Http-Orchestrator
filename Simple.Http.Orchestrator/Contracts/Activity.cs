namespace Simple.Http.Orchestrator.Contracts;

public class Activity
{
    public Guid Id { get; init; }
    public List<Request> Requests { get; init; } = new();

    public void Validate()
    {
        if (!Requests.Any())
        {
            throw new ArgumentException($"Property {nameof(Requests)} in JSON object {nameof(Activity)} not defined.");
        }

        foreach (var request in Requests) 
        { 
            request.Validate();
        }
    }
}

