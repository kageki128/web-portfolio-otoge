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
        const string SaveDataKey = "player_settings";

        public UniTask SavePlayerSettingsAsync(PlayerSettingsSaveDataCore saveData, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var json = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(SaveDataKey, json);
            PlayerPrefs.Save();

            Debug.Log($"[PlayerPrefsSaveDataRepository] Saved data. key={SaveDataKey}, length={json.Length}");

            return UniTask.CompletedTask;
        }

        public UniTask<PlayerSettingsSaveDataCore> LoadPlayerSettingsAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (!PlayerPrefs.HasKey(SaveDataKey))
            {
                return UniTask.FromResult<PlayerSettingsSaveDataCore>(null);
            }

            var json = PlayerPrefs.GetString(SaveDataKey);
            var saveData = JsonConvert.DeserializeObject<PlayerSettingsSaveDataCore>(json);
            if (saveData == null)
            {
                throw new InvalidOperationException($"Failed to deserialize save data. key={SaveDataKey}");
            }

            Debug.Log($"[PlayerPrefsSaveDataRepository] Loaded data. key={SaveDataKey}, length={json.Length}");

            return UniTask.FromResult(saveData);
        }
    }
}
