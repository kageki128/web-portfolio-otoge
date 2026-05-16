using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class LaundryHoldActor : NoteActorBase
    {
        const float JudgeRadius = 4.774f;
        const float InnerRadius = 1f;
        const float LaneStartAngleDeg = 112.5f;
        const float LaneStepAngleDeg = 45f;
        const float ScaleUpDistance = 1f;

        [SerializeField] SpriteRenderer image;
        [SerializeField] Color beforeColor;
        [SerializeField] Color holdingColor;
        [SerializeField] Color missedColor;

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

        public override void SetPosition(float currentScroll, float scrollSpeed)
        {
            if (NoteCore.State.CurrentValue is NoteState.AfterJudge)
            {
                return;
            }

            var beginRawDistance = CalculateDistance(NoteCore.Property.ScrollBegin, currentScroll, scrollSpeed);
            if (beginRawDistance < InnerRadius)
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.zero;
                return;
            }

            var endRawDistance = CalculateDistance(NoteCore.Property.ScrollEnd, currentScroll, scrollSpeed);
            var clampedEndRawDistance = Mathf.Max(InnerRadius, endRawDistance);

            var beginDistance = CalculateDisplayedDistance(beginRawDistance);
            var endDistance = CalculateDisplayedDistance(clampedEndRawDistance);

            var angleDeg = CalculateLaneAngleDeg(NoteCore.Property.Lane, NoteCore.Property.Width);
            var direction = CalculateDirection(angleDeg);

            var centerDistance = (beginDistance + endDistance) * 0.5f;
            var length = Mathf.Abs(beginDistance - endDistance);
            var scaleT = CalculateScale(beginRawDistance);

            gameObject.SetActive(true);
            transform.localPosition = new Vector3(direction.x * centerDistance, direction.y * centerDistance, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
            var displayLength = 1f + length;
            image.size = new Vector2(image.size.x, displayLength);
            transform.localScale = Vector3.one * scaleT;
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

            image.color = state switch
            {
                NoteState.BeforeJudge => beforeColor,
                NoteState.Holding => holdingColor,
                NoteState.Missed => missedColor,
                NoteState.Released => missedColor,
                _ => beforeColor
            };
        }

        static float CalculateLaneAngleDeg(int lane, int width)
        {
            var laneCenter = CalculateCenterX(lane, width);
            return LaneStartAngleDeg + (LaneStepAngleDeg * laneCenter);
        }

        static Vector2 CalculateDirection(float angleDeg)
        {
            var angleRad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        static float CalculateDistance(float scroll, float currentScroll, float scrollSpeed)
        {
            return JudgeRadius - ((scroll - currentScroll) * scrollSpeed);
        }

        static float CalculateScale(float rawDistance)
        {
            if (rawDistance < InnerRadius)
            {
                return 0f;
            }

            return Mathf.Clamp01((rawDistance - InnerRadius) / ScaleUpDistance);
        }

        static float CalculateDisplayedDistance(float rawDistance)
        {
            var movementStartRawDistance = InnerRadius + ScaleUpDistance;
            if (rawDistance <= movementStartRawDistance)
            {
                return InnerRadius;
            }

            var moveRangeRaw = JudgeRadius - movementStartRawDistance;
            var moveT = Mathf.Clamp01((rawDistance - movementStartRawDistance) / moveRangeRaw);
            return Mathf.Lerp(InnerRadius, JudgeRadius, moveT);
        }
    }
}
