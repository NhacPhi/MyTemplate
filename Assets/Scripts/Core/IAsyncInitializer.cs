using Cysharp.Threading.Tasks;
using System.Threading;

public interface IAsyncInitializer
{
    UniTask InitializeAsync(CancellationToken token);
}
