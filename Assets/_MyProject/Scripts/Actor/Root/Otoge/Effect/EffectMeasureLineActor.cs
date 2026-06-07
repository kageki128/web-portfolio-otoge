using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class EffectMeasureLineActor : NoteActorBase
    {
        const int MeasureLineWidth = 4;

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
            float x = EffectLaneLayout.GetVisualCenterX(NoteCore.Property.Lane, MeasureLineWidth);
            float y = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            transform.localPosition = new Vector3(x, y, 0f);
        }

        protected override void SetWidth(int width)
        {
            image.size = new Vector2(MeasureLineWidth, image.size.y);
        }

        protected override void SetLayer(int layer)
        {
            image.sortingOrder = EffectLaneLayout.GetSortingOrder(layer, NoteCore.Property.Lane);
        }

        protected override void SetAppearance(NoteState state)
        {
        }
    }
}
