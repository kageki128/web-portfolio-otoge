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

            var lane = NoteCore.Property.Lane;
            var width = NoteCore.Property.Width;
            var judgeDistance = IdolLaneLayout.GetJudgeDistance(lane, width);
            var direction = IdolLaneLayout.GetDirection(lane, width);

            var beginRawDistance = CalculateRawDistance(NoteCore.Property.ScrollBegin, currentScroll, scrollSpeed, judgeDistance);
            if (beginRawDistance < IdolLaneLayout.InnerRadius)
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.zero;
                return;
            }

            var endRawDistance = CalculateRawDistance(NoteCore.Property.ScrollEnd, currentScroll, scrollSpeed, judgeDistance);
            var clampedEndRawDistance = Mathf.Max(IdolLaneLayout.InnerRadius, endRawDistance);

            var beginDistance = CalculateDisplayedDistance(beginRawDistance, judgeDistance);
            var endDistance = CalculateDisplayedDistance(clampedEndRawDistance, judgeDistance);

            var centerDistance = (beginDistance + endDistance) * 0.5f;
            var length = Mathf.Abs(beginDistance - endDistance);
            var scaleT = CalculateScale(beginRawDistance);
            var center = IdolLaneLayout.GetCenterPosition();
            var angleDeg = IdolLaneLayout.GetLaneAngleDeg(lane, width);

            gameObject.SetActive(true);
            transform.localPosition = new Vector3(
                center.x + (direction.x * centerDistance),
                center.y + (direction.y * centerDistance),
                0f
            );
            transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
            image.size = new Vector2(image.size.x, 1f + length);
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

            if (!hasDefaultColor)
            {
                defaultColor = image.color;
                hasDefaultColor = true;
            }

            gameObject.SetActive(true);
            var baseColor = NoteCore.Property.Lane == IdolLaneLayout.CenterLane ? centerColor : defaultColor;
            image.color = HoldAppearance.ApplyStateAlpha(baseColor, state);
        }

        static float CalculateRawDistance(float scroll, float currentScroll, float scrollSpeed, float judgeDistance)
        {
            return judgeDistance - ((scroll - currentScroll) * scrollSpeed);
        }

        static float CalculateScale(float rawDistance)
        {
            if (rawDistance < IdolLaneLayout.InnerRadius)
            {
                return 0f;
            }

            return Mathf.Clamp01((rawDistance - IdolLaneLayout.InnerRadius) / IdolLaneLayout.ScaleUpDistance);
        }

        static float CalculateDisplayedDistance(float rawDistance, float judgeDistance)
        {
            var movementStartRawDistance = IdolLaneLayout.InnerRadius + IdolLaneLayout.ScaleUpDistance;
            if (rawDistance <= movementStartRawDistance)
            {
                return IdolLaneLayout.InnerRadius;
            }

            var moveRangeRaw = judgeDistance - movementStartRawDistance;
            var moveT = Mathf.Clamp01((rawDistance - movementStartRawDistance) / moveRangeRaw);
            var clampedDistance = Mathf.Lerp(IdolLaneLayout.InnerRadius, judgeDistance, moveT);
            return clampedDistance + Mathf.Max(0f, rawDistance - judgeDistance);
        }
    }
}
