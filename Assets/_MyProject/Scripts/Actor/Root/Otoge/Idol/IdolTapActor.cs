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
            return ShowWithFadeAsync(ct, image);
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            return HideWithFadeAsync(ct, image);
        }

        public override void SetPosition(float currentBeat, float currentScroll, float scrollSpeed)
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
                return;
            }

            var center = IdolLaneLayout.GetCenterPosition();
            gameObject.SetActive(true);
            transform.localPosition = new Vector3(
                center.x + (direction.x * rawDistance),
                center.y + (direction.y * rawDistance),
                0f
            );
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

