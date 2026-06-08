using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using UnityEngine;
using R3;

namespace MyProject.Actor
{
    public abstract class NoteActorBase : ActorBase
    {
        readonly Dictionary<SpriteRenderer, MotionHandle> fadeHandles = new();
        readonly Dictionary<SpriteRenderer, Color> lastFadeColors = new();
        readonly Dictionary<SpriteRenderer, Color> visibleColors = new();

        public NoteCoreBase NoteCore { get; private set; }

        protected JudgeEffectFactory JudgeEffectFactory;

        public void Install(NoteCoreBase noteCore, JudgeEffectFactory judgeEffectFactory)
        {
            NoteCore = noteCore;
            JudgeEffectFactory = judgeEffectFactory;
            SetWidth(noteCore.Property.Width);
            SetLayer(noteCore.Property.Layer);
            CacheVisibleColors();

            noteCore.State.Subscribe(state => SetAppearance(state)).AddTo(this);
            noteCore.Judge
                .Skip(1)
                .Subscribe(PlayJudgeEffect)
                .AddTo(this);
        }

        public abstract void SetPosition(float currentBeat, float currentScroll, float scrollSpeed);

        public void Destroy()
        {
            Destroy(gameObject);
        }

        protected abstract void SetWidth(int width);
        protected abstract void SetLayer(int layer);
        protected abstract void SetAppearance(NoteState state);
        protected virtual void PlayJudgeEffect(JudgeType judgeType)
        {
        }

        protected static float CalculateCenterX(int lane, int width)
        {
            return lane + ((width - 1) * 0.5f);
        }

        protected static bool IsHoldStartFixed(NoteState state)
        {
            return state is NoteState.Holding or NoteState.Released;
        }

        protected static float GetHoldStartScroll(float scrollBegin, float currentScroll, NoteState state)
        {
            return IsHoldStartFixed(state) ? currentScroll : scrollBegin;
        }

        protected static float CalculateCenterY(float scrollBegin, float scrollEnd, float currentScroll, float scrollSpeed)
        {
            float beginY = (scrollBegin - currentScroll) * scrollSpeed;
            float endY = (scrollEnd - currentScroll) * scrollSpeed;
            return (beginY + endY) / 2f;
        }
        protected static float CalculateHeight(float scrollBegin, float scrollEnd, float scrollSpeed)
        {
            float beginY = scrollBegin * scrollSpeed;
            float endY = scrollEnd * scrollSpeed;
            return Mathf.Abs(endY - beginY);
        }

        protected async UniTask ShowWithFadeAsync(CancellationToken ct, params SpriteRenderer[] targets)
        {
            gameObject.SetActive(true);
            SetAppearance(NoteCore.State.CurrentValue);
            await FadeAsync(true, false, ct, targets);
        }

        protected async UniTask HideWithFadeAsync(CancellationToken ct, params SpriteRenderer[] targets)
        {
            await FadeAsync(false, true, ct, targets);
            gameObject.SetActive(false);
        }

        async UniTask FadeAsync(bool fadeIn, bool restoreVisibleColor, CancellationToken ct, params SpriteRenderer[] targets)
        {
            if (!gameObject.activeSelf || targets.Length == 0)
            {
                return;
            }

            var duration = OtogeAppearance.StateTransitionDuration;
            var ease = OtogeAppearance.StateTransitionEase;
            var fadeTasks = new List<UniTask>(targets.Length);
            var colors = new Dictionary<SpriteRenderer, Color>(targets.Length);

            for (var i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                if (fadeHandles.TryGetValue(target, out var currentHandle))
                {
                    currentHandle.TryCancel();
                }

                var visibleColor = GetVisibleColor(target);
                var hiddenColor = WithAlpha(visibleColor, 0f);
                var from = fadeIn ? hiddenColor : visibleColor;
                var to = fadeIn ? visibleColor : hiddenColor;
                colors[target] = visibleColor;
                lastFadeColors[target] = from;
                target.color = from;

                MotionHandle handle = default;
                handle = LMotion.Create(from, to, duration)
                    .WithEase(ease)
                    .Bind(value =>
                    {
                        if (IsColorOverridden(target))
                        {
                            handle.TryCancel();
                            return;
                        }

                        target.color = value;
                        lastFadeColors[target] = value;
                    })
                    .AddTo(this);
                fadeHandles[target] = handle;
                fadeTasks.Add(AwaitFadeTaskAsync(target, handle, ct));
            }

            try
            {
                await UniTask.WhenAll(fadeTasks);
            }
            finally
            {
                if (restoreVisibleColor)
                {
                    foreach (var (target, color) in colors)
                    {
                        SetVisibleColor(target, color);
                    }
                }
            }
        }

        async UniTask AwaitFadeTaskAsync(SpriteRenderer target, MotionHandle handle, CancellationToken ct)
        {
            try
            {
                await handle.ToUniTask(CancelBehavior.Cancel, false, ct);
            }
            catch (System.OperationCanceledException) when (!ct.IsCancellationRequested)
            {
            }
            finally
            {
                if (fadeHandles.TryGetValue(target, out var currentHandle) && currentHandle == handle)
                {
                    fadeHandles.Remove(target);
                    lastFadeColors.Remove(target);
                }
            }
        }

        bool IsColorOverridden(SpriteRenderer target)
        {
            if (!lastFadeColors.TryGetValue(target, out var lastColor))
            {
                return false;
            }

            return target.color != lastColor;
        }

        void CacheVisibleColors()
        {
            foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>(true))
            {
                visibleColors[spriteRenderer] = spriteRenderer.color;
            }
        }

        Color GetVisibleColor(SpriteRenderer target)
        {
            var color = target.color;
            if (color.a > 0f || !visibleColors.TryGetValue(target, out var visibleColor))
            {
                visibleColors[target] = color;
                return color;
            }

            return visibleColor;
        }

        void SetVisibleColor(SpriteRenderer target, Color color)
        {
            target.color = color;
            visibleColors[target] = color;
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
