using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;
using unityroom.Api;

namespace MyProject.Infrastructure
{
    public class UnityroomRankingRegisterer : IRankingRegisterer
    {
        public UniTask RegisterAsync(ResultCore result, CancellationToken ct)
        {
            // Unityroomのランキング登録処理をここに実装する
            // UnityroomApiClient.Instance.SendScore();

            Debug.Log($"[UnityroomRankingRegisterer] Registered score to Unityroom ranking: {result}");

            return UniTask.CompletedTask;
        }
    }
}
