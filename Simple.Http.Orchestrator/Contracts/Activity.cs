namespace Simple.Http.Orchestrator.Contracts;

public class Activity
{
    public Guid Id { get; init; }
    public List<Requests> Requests { get; init; } = new List<Requests>();

    public void Validate()
    {
        if (!Requests.Any())
        {
            throw new ArgumentException($"Property {nameof(Requests)} in JSON object {nameof(Activity)} not defined.");
        }

        foreach (Requests request in Requests) 
        { 
            request.Validate();
        }
    }
}

