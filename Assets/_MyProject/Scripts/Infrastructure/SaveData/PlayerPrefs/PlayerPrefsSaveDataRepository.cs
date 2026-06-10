using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;
using Newtonsoft.Json;

namespace MyProject.Infrastructure
{
    public class PlayerPrefsSaveDataRepository : ISaveDataRepository
    {
        const string PlayerSettingsSaveDataKey = "player_settings";
        const string ScoreSaveDataKey = "score";

        public UniTask SavePlayerSettingsAsync(PlayerSettingsSaveDataCore saveData, CancellationToken ct)
        {
            return SaveAsync(PlayerSettingsSaveDataKey, saveData, ct);
        }

        public UniTask<PlayerSettingsSaveDataCore> LoadPlayerSettingsAsync(CancellationToken ct)
        {
            return LoadAsync<PlayerSettingsSaveDataCore>(PlayerSettingsSaveDataKey, ct);
        }

        public UniTask SaveScoreAsync(ScoreSaveDataCore saveData, CancellationToken ct)
        {
            return SaveAsync(ScoreSaveDataKey, saveData, ct);
        }

        public UniTask<ScoreSaveDataCore> LoadScoreAsync(CancellationToken ct)
        {
            return LoadAsync<ScoreSaveDataCore>(ScoreSaveDataKey, ct);
        }

        static UniTask SaveAsync<T>(string key, T saveData, CancellationToken ct) where T : class
        {
            ct.ThrowIfCancellationRequested();

            var json = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();

            Debug.Log($"[PlayerPrefsSaveDataRepository] Saved data. key={key}, length={json.Length}");

            return UniTask.CompletedTask;
        }

        static UniTask<T> LoadAsync<T>(string key, CancellationToken ct) where T : class
        {
            ct.ThrowIfCancellationRequested();

            if (!PlayerPrefs.HasKey(key))
            {
                return UniTask.FromResult<T>(null);
            }

            var json = PlayerPrefs.GetString(key);
            var saveData = JsonConvert.DeserializeObject<T>(json);
            if (saveData == null)
            {
                throw new InvalidOperationException($"Failed to deserialize save data. key={key}");
            }

            Debug.Log($"[PlayerPrefsSaveDataRepository] Loaded data. key={key}, length={json.Length}");

            return UniTask.FromResult(saveData);
        }
    }
}
