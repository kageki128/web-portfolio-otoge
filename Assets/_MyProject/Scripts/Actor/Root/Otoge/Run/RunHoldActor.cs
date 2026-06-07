using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class RunHoldActor : NoteActorBase
    {
        const int SortingOrderBeatScale = 100;

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
            return ShowWithFadeAsync(ct, image);
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            return HideWithFadeAsync(ct, image);
        }

        public override void SetPosition(float currentBeat, float currentScroll, float scrollSpeed)
        {
            var state = NoteCore.State.CurrentValue;
            if (state is NoteState.AfterJudge)
            {
                return;
            }

            var startScroll = GetHoldStartScroll(NoteCore.Property.ScrollBegin, currentScroll, state);
            var beginX = (startScroll - currentScroll) * scrollSpeed;
            var endX = (NoteCore.Property.ScrollEnd - currentScroll) * scrollSpeed;
            var x = (beginX + endX) * 0.5f;
            var y = RunLaneLayout.GetLaneY(NoteCore.Property.Lane);
            var length = Mathf.Abs(endX - beginX) + 1f;

            transform.localPosition = new Vector3(x, y, 0f);
            image.size = new Vector2(length, laneWidth);
        }

        protected override void SetWidth(int width)
        {
            laneWidth = width;
            image.size = new Vector2(image.size.x, width);
        }

        protected override void SetLayer(int _)
        {
            image.sortingOrder = Mathf.RoundToInt(-NoteCore.Property.TimingBegin.Beat * SortingOrderBeatScale);
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
            image.color = OtogeAppearance.GetHoldColor(defaultColor, state);
        }

        protected override void PlayJudgeEffect(JudgeType judgeType)
        {
            var position = new Vector3(0f, transform.localPosition.y, 0f);
            JudgeEffectFactory.PlayEffect(judgeType, RunLaneLayout.JudgeEffectRiseOffset, RiseAxis.Y, position);
        }
    }
}

