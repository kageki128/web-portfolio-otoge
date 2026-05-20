using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScanHoldActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer beginImage;
        [SerializeField] SpriteRenderer trailImage;
        [SerializeField] Color centerColor;

        Color defaultBeginColor;
        Color defaultTrailColor;
        Vector3 defaultBeginScale;
        Vector3 defaultTrailScale;
        bool hasDefaults;

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

            EnsureDefaults();

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
            var deltaY = endY - beginY;
            var x = ScanLaneLayout.GetLaneCenterX(NoteCore.Property.Lane, NoteCore.Property.Width);
            var height = Mathf.Abs(endY - beginY);
            var appearProgress = (currentBeat - appearBeat) / ScanLaneLayout.NoteAppearBeats;
            var appearScale = ScanLaneLayout.EaseNoteIn(appearProgress);
            var trailYDirection = IsJudgeLineMovingUp(startBeat) ? -1f : 1f;

            transform.localPosition = new Vector3(x, beginY, 0f);
            beginImage.transform.localPosition = Vector3.zero;
            beginImage.transform.localScale = new Vector3(
                defaultBeginScale.x * appearScale,
                defaultBeginScale.y * appearScale,
                defaultBeginScale.z
            );

            trailImage.transform.localPosition = new Vector3(0f, deltaY * 0.5f, 0f);
            trailImage.transform.localScale = new Vector3(
                defaultTrailScale.x * appearScale,
                defaultTrailScale.y * trailYDirection,
                defaultTrailScale.z
            );
            trailImage.size = new Vector2(trailImage.size.x, height);
            gameObject.SetActive(true);
        }

        protected override void SetWidth(int width)
        {
            beginImage.size = new Vector2(width, beginImage.size.y);
            trailImage.size = new Vector2(width, trailImage.size.y);
        }

        protected override void SetLayer(int layer)
        {
            beginImage.sortingOrder = layer;
        }

        protected override void SetAppearance(NoteState state)
        {
            if (state is NoteState.AfterJudge)
            {
                gameObject.SetActive(false);
                return;
            }

            EnsureDefaults();

            gameObject.SetActive(true);
            var beginBaseColor = ScanLaneLayout.IsCenterLane(NoteCore.Property.Lane) ? centerColor : defaultBeginColor;
            var trailBaseColor = ScanLaneLayout.IsCenterLane(NoteCore.Property.Lane) ? centerColor : defaultTrailColor;
            beginImage.color = HoldAppearance.ApplyStateAlpha(beginBaseColor, state);
            trailImage.color = HoldAppearance.ApplyStateAlpha(trailBaseColor, state);
        }

        void EnsureDefaults()
        {
            if (hasDefaults)
            {
                return;
            }

            defaultBeginColor = beginImage.color;
            defaultTrailColor = trailImage.color;
            defaultBeginScale = beginImage.transform.localScale;
            defaultTrailScale = trailImage.transform.localScale;
            hasDefaults = true;
        }

        static bool IsJudgeLineMovingUp(float beat)
        {
            var halfTripBeats = ScanLaneLayout.RoundTripBeats * 0.5f;
            var phase = Mathf.Repeat(beat, ScanLaneLayout.RoundTripBeats);
            return phase < halfTripBeats;
        }
    }
}
