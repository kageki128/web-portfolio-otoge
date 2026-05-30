using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class JudgeLaneEffectActor : ActorBase
    {
        const float ExpandDuration = 0.3f;
        const float FadeHalfDuration = 0.15f;
        const float MaxScale = 1.3f;

        [SerializeField] SpriteRenderer effectImage;
        [SerializeField] Color perfectColor;
        [SerializeField] Color goodColor;
        [SerializeField] Color missColor;

        MotionHandle scaleHandle;
        MotionHandle fadeHandle;

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
            scaleHandle.TryCancel();
            fadeHandle.TryCancel();
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void Play(JudgeType judgeType)
        {
            if (!TrySetJudgeColor(judgeType))
            {
                gameObject.SetActive(false);
                return;
            }

            scaleHandle.TryCancel();
            fadeHandle.TryCancel();

            gameObject.SetActive(true);
            effectImage.transform.localScale = Vector3.zero;
            SetAlpha(0f);
            PlayAnimationAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        async UniTaskVoid PlayAnimationAsync(CancellationToken ct)
        {
            try
            {
                scaleHandle = LMotion.Create(0f, 1f, ExpandDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(scale => effectImage.transform.localScale = Vector3.one * (MaxScale * scale))
                    .AddTo(this);

                fadeHandle = LMotion.Create(0f, 1f, FadeHalfDuration)
                    .WithEase(Ease.OutCubic)
                    .Bind(SetAlpha)
                    .AddTo(this);

                await fadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct);

                fadeHandle = LMotion.Create(1f, 0f, FadeHalfDuration)
                    .WithEase(Ease.Linear)
                    .Bind(SetAlpha)
                    .AddTo(this);

                await UniTask.WhenAll(
                    scaleHandle.ToUniTask(CancelBehavior.Cancel, false, ct),
                    fadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct)
                );

                Destroy(gameObject);
            }
            catch (System.OperationCanceledException)
            {
            }
        }

        void SetAlpha(float alpha)
        {
            var color = effectImage.color;
            color.a = alpha;
            effectImage.color = color;
        }

        bool TrySetJudgeColor(JudgeType judgeType)
        {
            switch (judgeType)
            {
                case JudgeType.PerfectCriticalFast:
                case JudgeType.PerfectCriticalLate:
                case JudgeType.PerfectFast:
                case JudgeType.PerfectLate:
                    SetBaseColor(perfectColor);
                    return true;
                case JudgeType.GoodFast:
                case JudgeType.GoodLate:
                    SetBaseColor(goodColor);
                    return true;
                case JudgeType.MissFast:
                case JudgeType.MissLate:
                    SetBaseColor(missColor);
                    return true;
                default:
                    return false;
            }
        }

        void SetBaseColor(Color baseColor)
        {
            baseColor.a = effectImage.color.a;
            effectImage.color = baseColor;
        }
    }
}
