using Simple.Http.Orchestrator;
using Simple.Http.Orchestrator.Services;
using Simple.Http.Orchestrator.Services.CreateBankAccount;
using Simple.Http.Orchestrator.Services.CreateCustomer;
using Simple.Http.Orchestrator.Services.Payment;
using System.Text.Json;

var sequenceJson = @"
{
   ""Id"" : ""D4D1ED39-68D1-433F-8A65-A7C52FA294BB"",
   ""Requests"":
     [
         {
             ""Id"" : ""UniqueId1"",
             ""ExecutionOrder"" : 1,
             ""ServiceCall"" : ""CreateCustomer"",
             ""Uri"" : ""https://www.bank.com/create-customer"",
             ""Dependencies"" : []
          },
          {
             ""Id"" : ""UniqueId3"",
             ""ExecutionOrder"" : 3,
             ""ServiceCall"" : ""Payment"",
             ""Uri"" : ""https://www.bank.com/payment"",
             ""Dependencies"" : [ ""UniqueId1"", ""UniqueId2"" ]
          },
          {
             ""Id"" : ""UniqueId2"",
             ""ExecutionOrder"" : 2,
             ""ServiceCall"" : ""CreateBankAccount"",
             ""Uri"" : ""https://www.bank.com/create-account"",
             ""Dependencies"" : [ ""UniqueId1"" ]
          },
          {
             ""Id"" : ""UniqueId4"",
             ""ExecutionOrder"" : 4,
             ""ServiceCall"" : ""Payment"",
             ""Uri"" : ""https://www.bank.com/payment"",
             ""Dependencies"" : [ ""UniqueId1"", ""UniqueId3"" ]
          }
     ]
}";

var activity = JsonSerializer.Deserialize<Activity>(sequenceJson)!;

var orchestrator = new ServiceOrchestrator(
    new Dictionary<string, IServiceCall> 
    {
        { nameof(CreateCustomerService).Replace("Service", string.Empty), new CreateCustomerService() },
        { nameof(CreateBankAccountService).Replace("Service", string.Empty), new CreateBankAccountService() },
        { nameof(PaymentService).Replace("Service", string.Empty), new PaymentService() }
    }
);

await orchestrator.ExecuteByOrder(activity, CancellationToken.None);

Console.ReadLine();
