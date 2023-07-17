namespace Simple.Http.Orchestrator.Services.CreateCustomer;

public class CreateCustomerService : ICreateCustomerService, IServiceCall
{
    public Task Execute(Uri uri, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(CreateCustomerService)} URL ----> {uri.AbsoluteUri}");
        return Task.CompletedTask;
    }
}
