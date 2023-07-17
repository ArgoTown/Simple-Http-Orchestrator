namespace Simple.Http.Orchestrator.Services;

public interface IServiceCall
{
    Task Execute(Uri uri, CancellationToken cancellationToken);
}
