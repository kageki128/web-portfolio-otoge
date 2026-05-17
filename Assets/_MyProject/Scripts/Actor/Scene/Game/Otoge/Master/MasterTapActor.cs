using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class MasterTapActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer image;
        [SerializeField] Sprite laneSprite0;
        [SerializeField] Sprite laneSprite1;

        Sprite defaultSprite;
        bool hasDefaultSprite;

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

            var x = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            transform.localPosition = new Vector3(x, 0f, 0f);
        }

        protected override void SetWidth(int width)
        {
            image.size = new Vector2(width, image.size.y);
        }

        protected override void SetLayer(int layer)
        {
            image.sortingOrder = -layer;
        }

        protected override void SetAppearance(NoteState state)
        {
            if (!hasDefaultSprite)
            {
                defaultSprite = image.sprite;
                hasDefaultSprite = true;
            }

            gameObject.SetActive(state is not NoteState.AfterJudge);
            if (!gameObject.activeSelf)
            {
                return;
            }

            image.sprite = NoteCore.Property.Lane switch
            {
                0 => laneSprite0 != null ? laneSprite0 : defaultSprite,
                1 => laneSprite1 != null ? laneSprite1 : defaultSprite,
                _ => defaultSprite
            };
        }
    }
}
