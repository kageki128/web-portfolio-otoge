using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class LaundryHoldActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer image;

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
            var judgeDistance = LaundryLaneLayout.GetJudgeDistance();
            var beginRawDistance = LaundryLaneLayout.GetRawDistance(NoteCore.Property.ScrollBegin, currentScroll, scrollSpeed, judgeDistance);
            if (beginRawDistance < LaundryLaneLayout.InnerRadius)
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.zero;
                return;
            }

            var endRawDistance = LaundryLaneLayout.GetRawDistance(NoteCore.Property.ScrollEnd, currentScroll, scrollSpeed, judgeDistance);
            var clampedEndRawDistance = Mathf.Max(LaundryLaneLayout.InnerRadius, endRawDistance);

            var beginDistance = CalculateDisplayedDistance(beginRawDistance, judgeDistance);
            var endDistance = CalculateDisplayedDistance(clampedEndRawDistance, judgeDistance);

            var angleDeg = LaundryLaneLayout.GetLaneAngleDeg(lane, width);
            var direction = LaundryLaneLayout.GetDirection(lane, width);

            var centerDistance = (beginDistance + endDistance) * 0.5f;
            var length = Mathf.Abs(beginDistance - endDistance);
            var scaleT = LaundryLaneLayout.GetScale(beginRawDistance);

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

            if (!hasDefaultColor)
            {
                defaultColor = image.color;
                hasDefaultColor = true;
            }

            image.color = HoldAppearance.ApplyStateAlpha(defaultColor, state);
        }

        static float CalculateDisplayedDistance(float rawDistance, float judgeDistance)
        {
            var movementStartRawDistance = LaundryLaneLayout.InnerRadius + LaundryLaneLayout.ScaleUpDistance;
            if (rawDistance <= movementStartRawDistance)
            {
                return LaundryLaneLayout.InnerRadius;
            }

            var moveRangeRaw = judgeDistance - movementStartRawDistance;
            var moveT = Mathf.Clamp01((rawDistance - movementStartRawDistance) / moveRangeRaw);
            return Mathf.Lerp(LaundryLaneLayout.InnerRadius, judgeDistance, moveT);
        }
    }
}
