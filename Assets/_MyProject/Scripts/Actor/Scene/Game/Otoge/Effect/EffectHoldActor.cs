using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class EffectHoldActor : NoteActorBase
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

            float x = EffectLaneLayout.GetVisualCenterX(NoteCore.Property.Lane, NoteCore.Property.Width);
            float y = CalculateCenterY(NoteCore.Property.ScrollBegin, NoteCore.Property.ScrollEnd, currentScroll, scrollSpeed);
            float height = CalculateHeight(NoteCore.Property.ScrollBegin, NoteCore.Property.ScrollEnd, scrollSpeed);

            transform.localPosition = new Vector3(x, y, 0f);
            image.size = new Vector2(image.size.x, height);
        }

        protected override void SetWidth(int width)
        {
            int visualWidth = EffectLaneLayout.GetVisualWidth(NoteCore.Property.Lane, width);
            image.size = new Vector2(visualWidth, image.size.y);
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
            var baseColor = EffectLaneLayout.IsCenterLane(NoteCore.Property.Lane) ? centerColor : defaultColor;
            image.color = HoldAppearance.ApplyStateBrightness(baseColor, state);
        }
    }
}
