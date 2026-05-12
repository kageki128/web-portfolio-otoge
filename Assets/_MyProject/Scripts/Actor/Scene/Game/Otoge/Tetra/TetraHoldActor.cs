using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class TetraHoldActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer image;
        [SerializeField] Color normalColor;
        [SerializeField] Color missColor;

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
            float x = CalculateCenterX(NoteCore.Property.Lane, NoteCore.Property.Width);
            float y = CalculateCenterY(NoteCore.Property.ScrollBegin, NoteCore.Property.ScrollEnd, currentScroll, scrollSpeed);
            float height = CalculateHeight(NoteCore.Property.ScrollBegin, NoteCore.Property.ScrollEnd, scrollSpeed);

            transform.localPosition = new Vector3(x, y, 0);
            image.size = new Vector2(image.size.x, height);
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

            gameObject.SetActive(true);
            image.color = state switch
            {
                NoteState.Missed => missColor,
                NoteState.Released => missColor,
                _ => normalColor
            };
        }
    }
}
