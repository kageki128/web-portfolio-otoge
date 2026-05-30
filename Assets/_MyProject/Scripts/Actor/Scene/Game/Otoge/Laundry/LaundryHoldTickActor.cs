using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class LaundryHoldTickActor : NoteActorBase
    {
        public override void Initialize()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }

        public override void SetPosition(float currentBeat, float currentScroll, float scrollSpeed)
        {
        }

        protected override void SetWidth(int width)
        {
        }

        protected override void SetLayer(int layer)
        {
        }

        protected override void SetAppearance(NoteState state)
        {
            gameObject.SetActive(false);
        }

        protected override void PlayJudgeEffect(JudgeType judgeType)
        {
            var judgePosition = LaundryLaneLayout.GetJudgePosition(NoteCore.Property.Lane, NoteCore.Property.Width);
            var position = new Vector3(judgePosition.x, judgePosition.y, 0f);
            JudgeEffectFactory.PlayEffect(judgeType, LaundryLaneLayout.JudgeEffectRiseOffset, RiseAxis.Y, position);
        }
    }
}
