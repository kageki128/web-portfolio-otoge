using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class LaundryTapActor : NoteActorBase
    {
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

        public override void SetPosition(float currentBeat, float currentScroll, float scrollSpeed)
        {
            if (NoteCore.State.CurrentValue is NoteState.AfterJudge)
            {
                return;
            }

            var lane = NoteCore.Property.Lane;
            var width = NoteCore.Property.Width;
            var direction = LaundryLaneLayout.GetDirection(lane, width);
            var judgeDistance = LaundryLaneLayout.GetJudgeDistance();
            var rawDistance = LaundryLaneLayout.GetRawDistance(NoteCore.Property.ScrollBegin, currentScroll, scrollSpeed, judgeDistance);
            if (rawDistance < LaundryLaneLayout.InnerRadius)
            {
                gameObject.SetActive(false);
                transform.localScale = Vector3.zero;
                return;
            }

            gameObject.SetActive(true);
            var distance = CalculateDisplayedDistance(rawDistance, judgeDistance);
            transform.localPosition = new Vector3(direction.x * distance, direction.y * distance, 0f);

            var scaleT = LaundryLaneLayout.GetScale(rawDistance);
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

        static float CalculateDisplayedDistance(float rawDistance, float judgeDistance)
        {
            var movementStartRawDistance = LaundryLaneLayout.InnerRadius + LaundryLaneLayout.ScaleUpDistance;
            if (rawDistance <= movementStartRawDistance)
            {
                return LaundryLaneLayout.InnerRadius;
            }

            var moveRangeRaw = judgeDistance - movementStartRawDistance;
            var moveT = Mathf.Clamp01((rawDistance - movementStartRawDistance) / moveRangeRaw);
            var clampedDistance = Mathf.Lerp(LaundryLaneLayout.InnerRadius, judgeDistance, moveT);
            return clampedDistance + Mathf.Max(0f, rawDistance - judgeDistance);
        }
    }
}
