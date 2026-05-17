using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class MasterTapActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer image;
        [SerializeField] Color laneColor0 = Color.white;
        [SerializeField] Color laneColor1 = Color.white;

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

        public override void SetPosition(float currentScroll, float scrollSpeed)
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
            if (!hasDefaultColor)
            {
                defaultColor = image.color;
                hasDefaultColor = true;
            }

            gameObject.SetActive(state is not NoteState.AfterJudge);
            if (!gameObject.activeSelf)
            {
                return;
            }

            image.color = NoteCore.Property.Lane switch
            {
                0 => laneColor0,
                1 => laneColor1,
                _ => defaultColor
            };
        }
    }
}
