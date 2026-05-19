using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScanHoldActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer image;
        [SerializeField] Color centerColor;

        Color defaultColor;
        bool hasDefaultColor;

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

        public override void SetPosition(float currentBeat, float currentScroll, float scrollSpeed)
        {
            if (NoteCore.State.CurrentValue is NoteState.AfterJudge)
            {
                return;
            }

            var startBeat = NoteCore.Property.TimingBegin.Beat;
            var appearBeat = startBeat - ScanLaneLayout.NoteAppearBeats;
            if (currentBeat < appearBeat)
            {
                gameObject.SetActive(false);
                return;
            }

            var endBeat = NoteCore.Property.TimingEnd.Beat;
            var beginY = ScanLaneLayout.GetJudgeLineY(startBeat);
            var endY = ScanLaneLayout.GetJudgeLineY(endBeat);
            var x = ScanLaneLayout.GetLaneCenterX(NoteCore.Property.Lane, NoteCore.Property.Width);
            var y = (beginY + endY) * 0.5f;
            var height = Mathf.Abs(endY - beginY);
            var appearProgress = (currentBeat - appearBeat) / ScanLaneLayout.NoteAppearBeats;

            transform.localPosition = new Vector3(x, y, 0f);
            transform.localScale = Vector3.one * ScanLaneLayout.EaseNoteIn(appearProgress);
            image.size = new Vector2(image.size.x, height);
            gameObject.SetActive(true);
        }

        protected override void SetWidth(int width)
        {
            image.size = new Vector2(width, image.size.y);
        }

        protected override void SetLayer(int layer)
        {
            image.sortingOrder = layer;
        }

        protected override void SetAppearance(NoteState state)
        {
            if (state is NoteState.AfterJudge)
            {
                gameObject.SetActive(false);
                return;
            }

            if (!hasDefaultColor)
            {
                defaultColor = image.color;
                hasDefaultColor = true;
            }

            gameObject.SetActive(true);
            var baseColor = ScanLaneLayout.IsCenterLane(NoteCore.Property.Lane) ? centerColor : defaultColor;
            image.color = HoldAppearance.ApplyStateAlpha(baseColor, state);
        }
    }
}
