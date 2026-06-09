using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    public class ResultInputKeysActor : ActorBase
    {
        [SerializeField] KeyIconActor quitKey;
        [SerializeField] KeyIconActor retryKey;

        public override void Initialize()
        {
            quitKey.Initialize();
            retryKey.Initialize();
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.WhenAll(quitKey.ShowAsync(ct), retryKey.ShowAsync(ct));
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await UniTask.WhenAll(quitKey.HideAsync(ct), retryKey.HideAsync(ct));
            gameObject.SetActive(false);
        }

        public void LightUpQuitKey()
        {
            quitKey.LightUp();
        }

        public void LightDownQuitKey()
        {
            quitKey.LightDown();
        }

        public void LightUpRetryKey()
        {
            retryKey.LightUp();
        }

        public void LightDownRetryKey()
        {
            retryKey.LightDown();
        }
    }
}
