using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Http.Orchestrator.Services.Payment;

public class PaymentService : IPaymentService, IServiceCall
{
    public Task Execute(Uri uri, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(PaymentService)} URL ----> {uri.AbsoluteUri}");
        return Task.CompletedTask;
    }
}
