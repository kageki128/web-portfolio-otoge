using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Core
{
    public interface IBeatmapRepository
    {
        UniTask<BeatmapCore> GetAsync(CancellationToken ct);
    }
}
