namespace Simple.Http.Orchestrator.Contracts.Requests;

public record CreateCustomerRequest(
    string Name, 
    string Surname, 
    int BirthYear, 
    string Nationality,
    ContactInformation ContactInformation);
