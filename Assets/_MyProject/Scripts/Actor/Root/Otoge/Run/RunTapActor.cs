using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class RunTapActor : NoteActorBase
    {
        const int SortingOrderBeatScale = 100;

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
            return ShowWithFadeAsync(ct, image);
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            return HideWithFadeAsync(ct, image);
        }

        public override void SetPosition(float currentBeat, float currentScroll, float scrollSpeed)
        {
            if (NoteCore.State.CurrentValue is NoteState.AfterJudge)
            {
                return;
            }

            var x = (NoteCore.Property.ScrollBegin - currentScroll) * scrollSpeed;
            var y = RunLaneLayout.GetLaneY(NoteCore.Property.Lane);
            transform.localPosition = new Vector3(x, y, 0f);
        }

        protected override void SetWidth(int width)
        {
            image.size = new Vector2(width, image.size.y);
        }

        protected override void SetLayer(int _)
        {
            image.sortingOrder = Mathf.RoundToInt(-NoteCore.Property.TimingBegin.Beat * SortingOrderBeatScale);
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

        protected override void PlayJudgeEffect(JudgeType judgeType)
        {
            var position = new Vector3(0f, transform.localPosition.y, 0f);
            JudgeEffectFactory.PlayEffect(judgeType, RunLaneLayout.JudgeEffectRiseOffset, RiseAxis.Y, position);
        }
    }
}

