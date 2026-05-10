using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    public class QuadTapActor : NoteActorBase
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

        public override void SetPosition(float currentScroll, float scrollSpeed)
        {
            float x = gameObject.transform.localPosition.x;
            float y = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            transform.localPosition = new Vector3(x, y, 0);
        }

        protected override void SetWidth(int width)
        {
            image.size = new Vector2((width * 2f) - 0.1f, image.size.y);
        }

        protected override void SetLayer(int layer)
        {
            image.sortingOrder = layer;
        }
    }
}
