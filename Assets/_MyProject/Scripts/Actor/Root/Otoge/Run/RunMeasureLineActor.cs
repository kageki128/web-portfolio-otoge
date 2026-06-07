using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class RunMeasureLineActor : NoteActorBase
    {
        const int MeasureLineWidth = 4;
        const int SortingOrderBeatScale = 100;

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
            float x = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            float y = 1f;
            transform.localPosition = new Vector3(x, y, 0f);
        }

        protected override void SetWidth(int width)
        {
            image.size = new Vector2(image.size.x, MeasureLineWidth);
        }

        protected override void SetLayer(int _)
        {
            image.sortingOrder = Mathf.RoundToInt(-NoteCore.Property.TimingBegin.Beat * SortingOrderBeatScale);
        }

        protected override void SetAppearance(NoteState state)
        {
        }
    }
}
