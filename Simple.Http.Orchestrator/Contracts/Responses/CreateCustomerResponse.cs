using Simple.Http.Orchestrator.Contracts.Responses.Base;

namespace Simple.Http.Orchestrator.Contracts.Requests;

public record CreateCustomerResponse(
    string Id,
    string AddressId,
    string Name,
    string Surname,
    int BirthYear,
    string Nationality,
    string ContactInformationId,
    string TraceId) : BaseResponse(Id);
