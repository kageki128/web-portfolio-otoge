using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScanJudgeLineActor : ActorBase
    {
        public override void Initialize()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void SetPosition(float currentBeat)
        {
            var localPosition = transform.localPosition;
            localPosition.y = ScanLaneLayout.GetJudgeLineY(currentBeat);
            transform.localPosition = localPosition;
        }
    }
}
