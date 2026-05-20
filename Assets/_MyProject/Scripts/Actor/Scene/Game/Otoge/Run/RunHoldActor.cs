using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class RunHoldActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer image;
        [SerializeField] Sprite laneSprite0;
        [SerializeField] Sprite laneSprite1;

        Color defaultColor;
        Sprite defaultSprite;
        bool hasDefaultAppearance;
        float laneWidth = 1f;

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

            var beginX = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            var endX = (NoteCore.Property.ScrollEnd - currentScroll) * scrollSpeed;
            var x = (beginX + endX) * 0.5f;
            var y = RunLaneLayout.GetLaneY(NoteCore.Property.Lane);
            var length = Mathf.Abs(endX - beginX);

            transform.localPosition = new Vector3(x, y, 0f);
            image.size = new Vector2(length, laneWidth);
        }

        protected override void SetWidth(int width)
        {
            laneWidth = width;
            image.size = new Vector2(image.size.x, width);
        }

        protected override void SetLayer(int layer)
        {
            image.sortingOrder = -layer;
        }

        protected override void SetAppearance(NoteState state)
        {
            if (state is NoteState.AfterJudge)
            {
                gameObject.SetActive(false);
                return;
            }

            if (!hasDefaultAppearance)
            {
                defaultColor = image.color;
                defaultSprite = image.sprite;
                hasDefaultAppearance = true;
            }

            gameObject.SetActive(true);
            image.sprite = NoteCore.Property.Lane switch
            {
                0 => laneSprite0 != null ? laneSprite0 : defaultSprite,
                1 => laneSprite1 != null ? laneSprite1 : defaultSprite,
                _ => defaultSprite
            };
            image.color = HoldAppearance.ApplyStateBrightness(defaultColor, state);
        }
    }
}
