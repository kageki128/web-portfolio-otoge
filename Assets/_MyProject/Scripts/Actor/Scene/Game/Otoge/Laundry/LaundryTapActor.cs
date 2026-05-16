using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class LaundryTapActor : NoteActorBase
    {
        const float JudgeRadius = 4.774f;
        const float InnerRadius = 1f;
        const float LaneStartAngleDeg = 112.5f;
        const float LaneStepAngleDeg = 45f;
        const float ScaleUpDistance = 1f;

        [SerializeField] SpriteRenderer image;

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

            var angleDeg = CalculateLaneAngleDeg(NoteCore.Property.Lane, NoteCore.Property.Width);
            var direction = CalculateDirection(angleDeg);
            var rawDistance = CalculateDistance(NoteCore.Property.ScrollBegin, currentScroll, scrollSpeed);
            if (rawDistance < InnerRadius)
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.zero;
                return;
            }

            gameObject.SetActive(true);
            var distance = CalculateDisplayedDistance(rawDistance);
            transform.localPosition = new Vector3(direction.x * distance, direction.y * distance, 0f);

            var scaleT = CalculateScale(rawDistance);
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
            }
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
            var clampedDistance = Mathf.Lerp(InnerRadius, JudgeRadius, moveT);
            return clampedDistance + Mathf.Max(0f, rawDistance - JudgeRadius);
        }
    }
}
