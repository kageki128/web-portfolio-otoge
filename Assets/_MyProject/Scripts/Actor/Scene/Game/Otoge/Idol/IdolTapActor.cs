using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class IdolTapActor : NoteActorBase
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
            var direction = IdolLaneLayout.GetDirection(lane, width);
            var judgeDistance = IdolLaneLayout.GetJudgeDistance(lane, width);
            var rawDistance = CalculateRawDistance(NoteCore.Property.ScrollBegin, currentScroll, scrollSpeed, judgeDistance);
            if (rawDistance < IdolLaneLayout.InnerRadius)
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.zero;
                return;
            }

            var displayedDistance = CalculateDisplayedDistance(rawDistance, judgeDistance);
            var center = IdolLaneLayout.GetCenterPosition();
            gameObject.SetActive(true);
            transform.localPosition = new Vector3(
                center.x + (direction.x * displayedDistance),
                center.y + (direction.y * displayedDistance),
                0f
            );

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
            if (!hasDefaultColor)
            {
                defaultColor = image.color;
                hasDefaultColor = true;
            }

            if (state is NoteState.AfterJudge)
            {
                gameObject.SetActive(false);
                return;
            }

            image.color = NoteCore.Property.Lane == IdolLaneLayout.CenterLane ? centerColor : defaultColor;
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
