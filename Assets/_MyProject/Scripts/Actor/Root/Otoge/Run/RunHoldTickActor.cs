using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class RunHoldTickActor : NoteActorBase
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
            var position = new Vector3(0f, RunLaneLayout.GetLaneY(NoteCore.Property.Lane), 0f);
            JudgeEffectFactory.PlayEffect(judgeType, RunLaneLayout.JudgeEffectRiseOffset, RiseAxis.Y, position);
        }
    }
}
