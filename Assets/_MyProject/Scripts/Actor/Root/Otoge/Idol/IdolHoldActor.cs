using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class IdolHoldActor : NoteActorBase
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
            return ShowWithFadeAsync(ct, image);
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            return HideWithFadeAsync(ct, image);
        }

        public override void SetPosition(float currentBeat, float currentScroll, float scrollSpeed)
        {
            var state = NoteCore.State.CurrentValue;
            if (state is NoteState.AfterJudge)
            {
                return;
            }

            var lane = NoteCore.Property.Lane;
            var width = NoteCore.Property.Width;
            var judgeDistance = IdolLaneLayout.GetJudgeDistance(lane, width);
            var direction = IdolLaneLayout.GetDirection(lane, width);

            var beginRawDistance = IsHoldStartFixed(state)
                ? judgeDistance
                : CalculateRawDistance(NoteCore.Property.ScrollBegin, currentScroll, scrollSpeed, judgeDistance);
            if (beginRawDistance < IdolLaneLayout.InnerRadius)
            {
                gameObject.SetActive(false);
                return;
            }

            var endRawDistance = CalculateRawDistance(NoteCore.Property.ScrollEnd, currentScroll, scrollSpeed, judgeDistance);
            var clampedEndRawDistance = Mathf.Max(IdolLaneLayout.InnerRadius, endRawDistance);

            var beginDistance = beginRawDistance;
            var endDistance = clampedEndRawDistance;

            var centerDistance = (beginDistance + endDistance) * 0.5f;
            var length = Mathf.Abs(beginDistance - endDistance);
            var center = IdolLaneLayout.GetCenterPosition();
            var angleDeg = IdolLaneLayout.GetLaneAngleDeg(lane, width);

            gameObject.SetActive(true);
            transform.localPosition = new Vector3(
                center.x + (direction.x * centerDistance),
                center.y + (direction.y * centerDistance),
                0f
            );
            transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
            var imageLengthScale = Mathf.Abs(image.transform.localScale.y);
            image.size = new Vector2(image.size.x, (1f + length) / imageLengthScale);
            transform.localScale = Vector3.one;
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
            var baseColor = NoteCore.Property.Lane == IdolLaneLayout.CenterLane ? centerColor : defaultColor;
            image.color = OtogeAppearance.GetHoldColor(baseColor, state);
        }

        protected override void PlayJudgeEffect(JudgeType judgeType)
        {
            var judgePosition = IdolLaneLayout.GetJudgePosition(NoteCore.Property.Lane, NoteCore.Property.Width);
            var position = new Vector3(judgePosition.x, judgePosition.y, 0f);
            JudgeEffectFactory.PlayEffect(judgeType, IdolLaneLayout.JudgeEffectRiseOffset, RiseAxis.Y, position);
        }

        static float CalculateRawDistance(float scroll, float currentScroll, float scrollSpeed, float judgeDistance)
        {
            return judgeDistance - ((scroll - currentScroll) * scrollSpeed);
        }

    }
}

