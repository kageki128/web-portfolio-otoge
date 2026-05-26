using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public abstract class NoteActorBase : ActorBase
    {
        readonly Dictionary<SpriteRenderer, MotionHandle> fadeHandles = new();

        public NoteCoreBase NoteCore { get; private set; }

        public void InstallCore(NoteCoreBase noteCore)
        {
            NoteCore = noteCore;
            SetWidth(noteCore.Property.Width);
            SetLayer(noteCore.Property.Layer);

            noteCore.State.Subscribe(state => SetAppearance(state)).AddTo(this);
        }

        public abstract void SetPosition(float currentBeat, float currentScroll, float scrollSpeed);

        public void Destroy()
        {
            Destroy(gameObject);
        }

        protected abstract void SetWidth(int width);
        protected abstract void SetLayer(int layer);
        protected abstract void SetAppearance(NoteState state);

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
                target.color = WithAlpha(to, 0f);

                var handle = LMotion.Create(target.color, to, duration)
                    .WithEase(ease)
                    .Bind(value => target.color = value)
                    .AddTo(this);
                fadeHandles[target] = handle;
                fadeTasks.Add(handle.ToUniTask(CancelBehavior.Cancel, false, ct));
            }

            await UniTask.WhenAll(fadeTasks);
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
