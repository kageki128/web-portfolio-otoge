using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Core
{
    public interface ISaveDataRepository
    {
        UniTask SaveAsync(SaveDataCore saveData, CancellationToken ct);
        UniTask<SaveDataCore> LoadAsync(CancellationToken ct);
    }
}
