using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Core
{
    public interface IBeatmapRepository
    {
        UniTask LoadAsync(CancellationToken ct);
        UniTask<Beatmap> GetAsync(CancellationToken ct);
    }
}
