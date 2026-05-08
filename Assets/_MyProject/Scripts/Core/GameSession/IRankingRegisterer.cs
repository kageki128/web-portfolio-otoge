using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Core
{
    public interface IRankingRegisterer
    {
        UniTask RegisterAsync(ResultCore result, CancellationToken ct);
    }
}
