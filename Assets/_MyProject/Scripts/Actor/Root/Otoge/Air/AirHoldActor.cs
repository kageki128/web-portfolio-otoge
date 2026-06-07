using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class AirHoldActor : NoteActorBase
    {
        [SerializeField] SpriteRenderer image;

        Color defaultColor;
        bool hasDefaultColor;

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
            var state = NoteCore.State.CurrentValue;
            var startScroll = GetHoldStartScroll(NoteCore.Property.ScrollBegin, currentScroll, state);
            float x = CalculateCenterX(NoteCore.Property.Lane, NoteCore.Property.Width);
            float y = CalculateCenterY(startScroll, NoteCore.Property.ScrollEnd, currentScroll, scrollSpeed);
            float height = CalculateHeight(startScroll, NoteCore.Property.ScrollEnd, scrollSpeed) + 0.5f;

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

            if (!hasDefaultColor)
            {
                defaultColor = image.color;
                hasDefaultColor = true;
            }

            gameObject.SetActive(true);
            image.color = OtogeAppearance.GetHoldColor(defaultColor, state);
        }

        protected override void PlayJudgeEffect(JudgeType judgeType)
        {
            var position = new Vector3(transform.localPosition.x, 0f, 0f);
            JudgeEffectFactory.PlayEffect(judgeType, AirLaneLayout.JudgeEffectRiseOffset, RiseAxis.Z, position);
        }
    }
}

