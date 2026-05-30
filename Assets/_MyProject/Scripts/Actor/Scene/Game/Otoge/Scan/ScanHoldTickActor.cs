using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScanHoldTickActor : NoteActorBase
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
            var x = ScanLaneLayout.GetLaneCenterX(NoteCore.Property.Lane, NoteCore.Property.Width);
            var y = ScanLaneLayout.GetJudgeLineY(NoteCore.Property.TimingBegin.Beat);
            var position = new Vector3(x, y, 0f);
            JudgeEffectFactory.PlayEffect(
                judgeType,
                ScanLaneLayout.JudgeEffectRiseOffset,
                RiseAxis.Y,
                position,
                ScanLaneLayout.JudgeEffectRiseAmount
            );
        }
    }
}
