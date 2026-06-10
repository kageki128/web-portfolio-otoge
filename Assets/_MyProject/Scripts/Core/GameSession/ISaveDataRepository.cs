using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Core
{
    public interface ISaveDataRepository
    {
        UniTask SavePlayerSettingsAsync(PlayerSettingsSaveDataCore saveData, CancellationToken ct);
        UniTask<PlayerSettingsSaveDataCore> LoadPlayerSettingsAsync(CancellationToken ct);
        UniTask SaveScoreAsync(ScoreSaveDataCore saveData, CancellationToken ct);
        UniTask<ScoreSaveDataCore> LoadScoreAsync(CancellationToken ct);
    }
}
