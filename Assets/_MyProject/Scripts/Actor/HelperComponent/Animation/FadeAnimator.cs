using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Actor
{
    [DisallowMultipleComponent]
    public class FadeAnimator : MonoBehaviour
    {
        const float DurationSeconds = 0.3f;
        const Ease FadeEase = Ease.OutCubic;

        readonly List<FadeTarget> fadeTargets = new();

        MotionHandle fadeHandle;

        public void Initialize()
        {
            CacheFadeTargets();
            fadeHandle.TryCancel();
        }

        public UniTask ShowAsync(CancellationToken ct)
        {
            return PlayAsync(PhaseType.Show, ct);
        }

        public UniTask HideAsync(CancellationToken ct)
        {
            return PlayAsync(PhaseType.Hide, ct);
        }

        UniTask PlayAsync(PhaseType phaseType, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            fadeHandle.TryCancel();

            ApplyFadeProgress(0f, phaseType);

            fadeHandle = LMotion.Create(0f, 1f, DurationSeconds)
                .WithEase(FadeEase)
                .Bind(progress => ApplyFadeProgress(progress, phaseType))
                .AddTo(this);

            return fadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct);
        }

        void CacheFadeTargets()
        {
            fadeTargets.Clear();

            if (TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                fadeTargets.Add(new FadeTarget(value => canvasGroup.alpha = value, canvasGroup.alpha));
                return;
            }

            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                fadeTargets.Add(new FadeTarget(
                    value =>
                    {
                        var color = graphic.color;
                        color.a = value;
                        graphic.color = color;
                    },
                    graphic.color.a));
            }

            foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>(true))
            {
                fadeTargets.Add(new FadeTarget(
                    value =>
                    {
                        var color = spriteRenderer.color;
                        color.a = value;
                        spriteRenderer.color = color;
                    },
                    spriteRenderer.color.a));
            }
        }

        void ApplyFadeProgress(float progress, PhaseType phaseType)
        {
            foreach (var target in fadeTargets)
            {
                var alpha = phaseType == PhaseType.Show
                    ? Mathf.Lerp(0f, target.BaseAlpha, progress)
                    : Mathf.Lerp(target.BaseAlpha, 0f, progress);

                target.ApplyAlpha(alpha);
            }
        }

        readonly struct FadeTarget
        {
            public float BaseAlpha { get; }

            readonly Action<float> applyAlpha;

            public FadeTarget(Action<float> applyAlpha, float baseAlpha)
            {
                this.applyAlpha = applyAlpha;
                BaseAlpha = baseAlpha;
            }

            public void ApplyAlpha(float alpha)
            {
                applyAlpha(alpha);
            }
        }

        enum PhaseType
        {
            Show,
            Hide
        }
    }
}
