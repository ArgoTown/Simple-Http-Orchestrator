using Simple.Http.Orchestrator.Services.Payment;

namespace Simple.Http.Orchestrator.Services.CreateBankAccount;

public class CreateBankAccountService : ICreateBankAccountService, IServiceCall
{
    public Task Execute(Uri uri, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(CreateBankAccountService)} URL ----> {uri.AbsoluteUri}");
        return Task.CompletedTask;
    }
}
