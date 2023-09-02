namespace Simple.Http.Orchestrator.Contracts;

public class Activity
{
    public Guid Id { get; init; }
    public List<Request> Requests { get; init; } = new();

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (!Requests.Any())
        {
            errors.Add($"Property {nameof(Requests)} in JSON object {nameof(Activity)} not defined.");
        }

        Requests.ForEach(request => errors.AddRange(request.Validate()));

        return errors;
    }
}

