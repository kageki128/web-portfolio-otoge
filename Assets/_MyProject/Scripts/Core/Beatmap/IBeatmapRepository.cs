using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Core
{
    public interface IBeatmapRepository
    {
        UniTask LoadAllAsync(CancellationToken ct);
        UniTask<IReadOnlyList<Beatmap>> GetAllAsync(CancellationToken ct);
    }
}
