using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class OctaMeasureLineActor : NoteActorBase
    {
        const int MeasureLineWidth = 8;

        [SerializeField] SpriteRenderer image;

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
            float x = CalculateCenterX(NoteCore.Property.Lane, MeasureLineWidth);
            float y = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            transform.localPosition = new Vector3(x, y, 0f);
        }

        protected override void SetWidth(int width)
        {
            image.size = new Vector2(MeasureLineWidth, image.size.y);
        }

        protected override void SetLayer(int layer)
        {
            image.sortingOrder = layer;
        }

        protected override void SetAppearance(NoteState state)
        {
        }
    }
}
