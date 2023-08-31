namespace Simple.Http.Orchestrator.Contracts;

public class Activity
{
    public Guid Id { get; init; }
    public List<Request> Requests { get; init; } = new List<Request>();

    public void Validate()
    {
        if (!Requests.Any())
        {
            throw new ArgumentException($"Property {nameof(Requests)} in JSON object {nameof(Activity)} not defined.");
        }

        foreach (Request request in Requests) 
        { 
            request.Validate();
        }
    }
}

