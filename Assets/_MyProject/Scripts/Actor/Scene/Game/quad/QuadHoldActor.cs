using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    public class QuadHoldActor : NoteActorBase
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
            float beginY = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            float endY = (NoteCore.Property.ScrollEnd - currentScroll) * scrollSpeed;
            float y = (beginY + endY) / 2f;
            float height = Mathf.Abs(endY - beginY);

            transform.localPosition = new Vector3(x, y, 0);
            image.size = new Vector2(image.size.x, height);
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
