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

        public NoteCoreBase NoteCore { get; private set; }

        protected JudgeEffectFactory JudgeEffectFactory;

        public void Install(NoteCoreBase noteCore, JudgeEffectFactory judgeEffectFactory)
        {
            NoteCore = noteCore;
            JudgeEffectFactory = judgeEffectFactory;
            SetWidth(noteCore.Property.Width);
            SetLayer(noteCore.Property.Layer);

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
            if (!gameObject.activeSelf || targets.Length == 0)
            {
                return;
            }

            var duration = OtogeAppearance.StateTransitionDuration;
            var ease = OtogeAppearance.StateTransitionEase;
            var fadeTasks = new List<UniTask>(targets.Length);

            for (var i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                if (fadeHandles.TryGetValue(target, out var currentHandle))
                {
                    currentHandle.TryCancel();
                }

                var to = target.color;
                var from = WithAlpha(to, 0f);
                target.color = from;
                lastFadeColors[target] = from;

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

            await UniTask.WhenAll(fadeTasks);
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
                fadeHandles.Remove(target);
                lastFadeColors.Remove(target);
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

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
