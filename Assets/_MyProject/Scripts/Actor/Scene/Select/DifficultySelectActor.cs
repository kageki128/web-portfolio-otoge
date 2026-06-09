using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class DifficultySelectActor : ActorBase
    {
        const float ValueAnimationDuration = 0.1f;
        const float ValueSlideDistance = 50f;

        [SerializeField] KeyIconActor downKey;
        [SerializeField] KeyIconActor upKey;
        [SerializeField] TMP_Text valueText;
        [SerializeField] Color normalValueTextColor = Color.white;
        [SerializeField] Color hardValueTextColor = Color.red;

        RectTransform valueTextRect;
        Vector2 baseValueTextPosition;
        MotionHandle valueMoveHandle;
        MotionHandle valueFadeHandle;
        int valueAnimationVersion;
        BeatmapType currentBeatmapType;
        bool hasBeatmapType;

        public override void Initialize()
        {
            valueTextRect = valueText.rectTransform;
            baseValueTextPosition = valueTextRect.anchoredPosition;
            hasBeatmapType = false;

            downKey.Initialize();
            upKey.Initialize();
            ResetValueText();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(
                downKey.ShowAsync(ct),
                upKey.ShowAsync(ct)
            );
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            CancelValueAnimation();
            ResetValueText();
            await UniTask.WhenAll(
                downKey.HideAsync(ct),
                upKey.HideAsync(ct)
            );
            gameObject.SetActive(false);
        }

        public void SetBeatmapType(BeatmapType beatmapType)
        {
            if (!hasBeatmapType || currentBeatmapType == beatmapType)
            {
                SetBeatmapTypeImmediately(beatmapType);
                return;
            }

            var changeDirection = (int)beatmapType > (int)currentBeatmapType ? Vector2.right : Vector2.left;
            currentBeatmapType = beatmapType;
            valueAnimationVersion++;
            CancelValueAnimation();
            PlayValueAnimationAsync(beatmapType, changeDirection, valueAnimationVersion, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void LightUpUpKey()
        {
            upKey.LightUp();
        }

        public void LightDownUpKey()
        {
            upKey.LightDown();
        }

        public void LightUpDownKey()
        {
            downKey.LightUp();
        }

        public void LightDownDownKey()
        {
            downKey.LightDown();
        }

        async UniTaskVoid PlayValueAnimationAsync(BeatmapType beatmapType, Vector2 changeDirection, int version, CancellationToken ct)
        {
            try
            {
                var outPosition = baseValueTextPosition - changeDirection * ValueSlideDistance;
                valueMoveHandle = LMotion.Create(valueTextRect.anchoredPosition, outPosition, ValueAnimationDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(position => valueTextRect.anchoredPosition = position)
                    .AddTo(this);

                valueFadeHandle = LMotion.Create(valueText.color.a, 0f, ValueAnimationDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(SetValueTextAlpha)
                    .AddTo(this);

                await UniTask.WhenAll(
                    valueMoveHandle.ToUniTask(CancelBehavior.Cancel, false, ct),
                    valueFadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct)
                );

                if (valueAnimationVersion != version)
                {
                    return;
                }

                valueText.text = ToDisplayText(beatmapType);
                valueTextRect.anchoredPosition = baseValueTextPosition + changeDirection * ValueSlideDistance;
                SetValueTextColor(beatmapType, 0f);

                valueMoveHandle = LMotion.Create(valueTextRect.anchoredPosition, baseValueTextPosition, ValueAnimationDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(position => valueTextRect.anchoredPosition = position)
                    .AddTo(this);

                valueFadeHandle = LMotion.Create(0f, ToTextColor(beatmapType).a, ValueAnimationDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(SetValueTextAlpha)
                    .AddTo(this);

                await UniTask.WhenAll(
                    valueMoveHandle.ToUniTask(CancelBehavior.Cancel, false, ct),
                    valueFadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct)
                );
            }
            catch (System.OperationCanceledException)
            {
            }

            if (valueAnimationVersion == version)
            {
                ResetValueText();
            }
        }

        void CancelValueAnimation()
        {
            valueMoveHandle.TryCancel();
            valueFadeHandle.TryCancel();
        }

        void ResetValueText()
        {
            valueTextRect.anchoredPosition = baseValueTextPosition;
            valueText.color = hasBeatmapType ? ToTextColor(currentBeatmapType) : normalValueTextColor;
        }

        void SetValueTextColor(BeatmapType beatmapType, float alpha)
        {
            var color = ToTextColor(beatmapType);
            color.a = alpha;
            valueText.color = color;
        }

        void SetValueTextAlpha(float alpha)
        {
            var color = valueText.color;
            color.a = alpha;
            valueText.color = color;
        }

        void SetBeatmapTypeImmediately(BeatmapType beatmapType)
        {
            currentBeatmapType = beatmapType;
            hasBeatmapType = true;
            valueAnimationVersion++;
            CancelValueAnimation();
            valueText.text = ToDisplayText(beatmapType);
            ResetValueText();
        }

        Color ToTextColor(BeatmapType beatmapType)
        {
            return beatmapType == BeatmapType.Hard ? hardValueTextColor : normalValueTextColor;
        }

        static string ToDisplayText(BeatmapType beatmapType)
        {
            return beatmapType switch
            {
                BeatmapType.Demo => "DEMO",
                BeatmapType.Normal => "NORMAL",
                BeatmapType.Hard => "HARD",
                _ => throw new System.ArgumentOutOfRangeException(nameof(beatmapType), beatmapType, null)
            };
        }
    }
}
