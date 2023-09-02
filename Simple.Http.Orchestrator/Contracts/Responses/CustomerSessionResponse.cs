namespace Simple.Http.Orchestrator.Contracts.Responses;

public record CustomerSessionResponse(CustomerInformation Customer);
public record CustomerInformation(Contact Contact);
public record Contact(string Id);
