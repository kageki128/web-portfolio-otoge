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
    public class StandardTransitionAnimator : MonoBehaviour
    {

        [Serializable]
        class MoveSettings
        {
            [field: SerializeField, Min(0f)]
            public float Distance { get; private set; } = 3f;

            [field: SerializeField]
            public float AngleDegrees { get; private set; } = 0f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = Ease.OutCubic;
        }

        [Serializable]
        class RotationSettings
        {
            [field: SerializeField]
            public float AngleDegrees { get; private set; } = 0f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = Ease.OutCubic;
        }

        [Serializable]
        class ScaleSettings
        {
            [field: SerializeField, Min(0f)]
            public float Multiplier { get; private set; } = 1f;

            [field: SerializeField]
            public Ease Ease { get; private set; } = Ease.OutCubic;
        }

        [Serializable]
        class FadeSettings
        {
            [field: SerializeField]
            public bool IsFade { get; private set; } = true;

            [field: SerializeField]
            public Ease Ease { get; private set; } = Ease.OutCubic;
        }

        [Serializable]
        class PhaseSettings
        {
            [field: SerializeField, Min(0f)]
            public float DurationSeconds { get; private set; } = 0.3f;

            [field: SerializeField]
            public bool UseCanvasGroupForFade { get; private set; } = true;

            [field: SerializeField]
            public MoveSettings Move { get; private set; } = new();

            [field: SerializeField]
            public RotationSettings Rotation { get; private set; } = new();

            [field: SerializeField]
            public ScaleSettings Scale { get; private set; } = new();

            [field: SerializeField]
            public FadeSettings Fade { get; private set; } = new();
        }

        [Header("Initial Show")]
        [SerializeField] PhaseSettings initialShowSettings = new();

        [Header("Show")]
        [SerializeField] PhaseSettings showSettings = new();

        [Header("Hide")]
        [SerializeField] PhaseSettings hideSettings = new();

        FadeTarget selfCanvasGroupFadeTarget;
        readonly List<FadeTarget> childFadeTargets = new();

        RectTransform rectTransform;
        bool usesAnchoredPosition;
        Vector3 basePosition;
        Quaternion baseRotation;
        Vector3 baseScale;

        MotionHandle moveHandle;
        MotionHandle rotationHandle;
        MotionHandle fadeHandle;
        MotionHandle scaleHandle;

        public void Initialize()
        {
            var selfCanvasGroup = GetComponent<CanvasGroup>();
            selfCanvasGroupFadeTarget = CreateCanvasGroupFadeTarget(selfCanvasGroup);

            rectTransform = transform as RectTransform;
            usesAnchoredPosition = rectTransform != null;
            basePosition = usesAnchoredPosition ? rectTransform.anchoredPosition3D : transform.localPosition;
            baseRotation = transform.localRotation;
            baseScale = transform.localScale;

            CacheChildFadeTargets();
            CancelRunningMotions();
        }

        public UniTask InitialShowAsync(CancellationToken ct) =>
             PlayPhaseAsync(initialShowSettings, PhaseType.Show, ct);

        public UniTask ShowAsync(CancellationToken ct) =>
             PlayPhaseAsync(showSettings, PhaseType.Show, ct);

        public UniTask HideAsync(CancellationToken ct) =>
             PlayPhaseAsync(hideSettings, PhaseType.Hide, ct);



        UniTask PlayPhaseAsync(PhaseSettings settings, PhaseType phaseType, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            CancelRunningMotions();

            var duration = settings.DurationSeconds;
            var tasks = new List<UniTask>();

            moveHandle = CreateMoveMotion(settings.Move, phaseType, duration).AddTo(this);
            tasks.Add(moveHandle.ToUniTask(CancelBehavior.Cancel, false, ct));

            rotationHandle = CreateRotationMotion(settings.Rotation, phaseType, duration).AddTo(this);
            tasks.Add(rotationHandle.ToUniTask(CancelBehavior.Cancel, false, ct));

            scaleHandle = CreateScaleMotion(settings.Scale, phaseType, duration).AddTo(this);
            tasks.Add(scaleHandle.ToUniTask(CancelBehavior.Cancel, false, ct));

            if (settings.Fade.IsFade)
            {
                bool useCanvasGroup = settings.UseCanvasGroupForFade && selfCanvasGroupFadeTarget.IsAlive();

                fadeHandle = CreateFadeMotion(useCanvasGroup, settings.Fade, phaseType, duration).AddTo(this);
                tasks.Add(fadeHandle.ToUniTask(CancelBehavior.Cancel, false, ct));
            }

            return UniTask.WhenAll(tasks);
        }

        MotionHandle CreateMoveMotion(MoveSettings settings, PhaseType phaseType, float duration)
        {
            var radian = settings.AngleDegrees * Mathf.Deg2Rad;
            var direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);
            var offset = direction * settings.Distance;

            var from = phaseType == PhaseType.Hide ? basePosition : basePosition + offset;
            var to = phaseType == PhaseType.Hide ? basePosition + offset : basePosition;

            return LMotion.Create(from, to, duration)
                .WithEase(settings.Ease)
                .Bind(SetCurrentPosition);
        }

        void SetCurrentPosition(Vector3 value)
        {
            if (usesAnchoredPosition)
            {
                rectTransform.anchoredPosition3D = value;
                return;
            }

            transform.localPosition = value;
        }

        MotionHandle CreateFadeMotion(bool useCanvasGroup, FadeSettings settings, PhaseType phaseType, float duration)
        {
            var fadeTargets = useCanvasGroup ? new List<FadeTarget> { selfCanvasGroupFadeTarget } : childFadeTargets;
            return LMotion.Create(0f, 1f, duration)
                .WithEase(settings.Ease)
                .Bind(progress => ApplyFadeProgress(fadeTargets, progress, phaseType));
        }

        MotionHandle CreateRotationMotion(RotationSettings settings, PhaseType phaseType, float duration)
        {
            var offset = Quaternion.Euler(0f, 0f, settings.AngleDegrees);
            var startRotation = phaseType == PhaseType.Hide ? baseRotation : baseRotation * offset;
            var targetRotation = phaseType == PhaseType.Hide ? baseRotation * offset : baseRotation;

            return LMotion.Create(startRotation, targetRotation, duration)
                .WithEase(settings.Ease)
                .Bind(value => transform.localRotation = value);
        }

        MotionHandle CreateScaleMotion(ScaleSettings settings, PhaseType phaseType, float duration)
        {
            var startScale = phaseType == PhaseType.Hide ? baseScale : baseScale * settings.Multiplier;
            var targetScale = phaseType == PhaseType.Hide ? baseScale * settings.Multiplier : baseScale;

            return LMotion.Create(startScale, targetScale, duration)
                .WithEase(settings.Ease)
                .Bind(value => transform.localScale = value);
        }

        void CancelRunningMotions()
        {
            moveHandle.TryCancel();
            rotationHandle.TryCancel();
            fadeHandle.TryCancel();
            scaleHandle.TryCancel();
        }

        void CacheChildFadeTargets()
        {
            childFadeTargets.Clear();

            var graphics = GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                childFadeTargets.Add(CreateGraphicFadeTarget(graphic));
            }

            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spriteRenderers)
            {
                childFadeTargets.Add(CreateSpriteFadeTarget(spriteRenderer));
            }
        }

        void ApplyFadeProgress(IReadOnlyList<FadeTarget> fadeTargets, float progress, PhaseType phaseType)
        {
            foreach (var target in fadeTargets)
            {
                var from = phaseType == PhaseType.Hide ? target.BaseAlpha : 0f;
                var to = phaseType == PhaseType.Hide ? 0f : target.BaseAlpha;

                var value = Mathf.Lerp(from, to, progress);
                target.ApplyAlpha(value);
            }
        }

        static FadeTarget CreateCanvasGroupFadeTarget(CanvasGroup target)
        {
            if (target == null)
            {
                return CreateInvalidFadeTarget();
            }

            return new FadeTarget(
                () => target != null,
                value => target.alpha = value,
                target.alpha);
        }

        static FadeTarget CreateGraphicFadeTarget(Graphic target)
        {
            if (target == null)
            {
                return CreateInvalidFadeTarget();
            }

            return new FadeTarget(
                () => target != null,
                value =>
                {
                    var color = target.color;
                    color.a = value;
                    target.color = color;
                },
                target.color.a);
        }

        static FadeTarget CreateSpriteFadeTarget(SpriteRenderer target)
        {
            if (target == null)
            {
                return CreateInvalidFadeTarget();
            }

            return new FadeTarget(
                () => target != null,
                value =>
                {
                    var color = target.color;
                    color.a = value;
                    target.color = color;
                },
                target.color.a);
        }

        static FadeTarget CreateInvalidFadeTarget()
        {
            return new FadeTarget(() => false, _ => { }, 0f);
        }

        readonly struct FadeTarget
        {
            public Func<bool> IsAlive { get; }
            public float BaseAlpha { get; }

            readonly Action<float> writeAlpha;

            public FadeTarget(Func<bool> isAlive, Action<float> writeAlpha, float baseAlpha)
            {
                IsAlive = isAlive;
                BaseAlpha = baseAlpha;
                this.writeAlpha = writeAlpha;
            }

            public void ApplyAlpha(float alpha)
            {
                if (!IsAlive())
                {
                    return;
                }

                writeAlpha(alpha);
            }
        }

        enum PhaseType
        {
            Show,
            Hide
        }
    }
}

