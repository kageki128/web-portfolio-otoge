using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Actor
{
    public class KeyIconActor : ActorBase
    {
        [SerializeField] Image background;
        [SerializeField] TMP_Text text;
        [SerializeField] Color backgroundNormalColor = Color.white;
        [SerializeField] Color backgroundLightUpColor = Color.white;
        [SerializeField] Color textNormalColor = Color.white;
        [SerializeField] Color textLightUpColor = Color.white;
        [SerializeField] bool breathingEnabled;

        const float LightDownDuration = 0.033f;
        const float PressedScaleMultiplier = 1.2f;
        const float ScaleDuration = 0.033f;
        const float BreathingCycleDuration = 1.5f;
        const float BreathingScaleMultiplier = 1.06f;

        MotionHandle backgroundHandle;
        MotionHandle textHandle;
        MotionHandle scaleHandle;
        Vector3 baseScale;
        bool isPressed;
        bool isShown;

        public override void Initialize()
        {
            baseScale = transform.localScale;
            SetNormalState();
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            SetNormalState();
            gameObject.SetActive(true);
            isShown = true;
            TryPlayBreathing();
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            SetNormalState();
            isShown = false;
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void LightUp()
        {
            Cancel();
            isPressed = true;
            background.color = backgroundLightUpColor;
            text.color = textLightUpColor;
            scaleHandle = Scale(baseScale * PressedScaleMultiplier);
        }

        public void LightDown()
        {
            Cancel();
            isPressed = false;
            backgroundHandle = Fade(background, backgroundNormalColor);
            textHandle = Fade(text, textNormalColor);
            scaleHandle = Scale(baseScale, TryPlayBreathing);
        }

        void SetNormalState()
        {
            Cancel();
            isPressed = false;
            background.color = backgroundNormalColor;
            text.color = textNormalColor;
            transform.localScale = baseScale;
        }

        void Cancel()
        {
            backgroundHandle.TryCancel();
            textHandle.TryCancel();
            scaleHandle.TryCancel();
        }

        MotionHandle Fade(Graphic graphic, Color color)
        {
            return LMotion.Create(graphic.color, color, LightDownDuration)
                .Bind(value => graphic.color = value)
                .AddTo(this);
        }

        MotionHandle Scale(Vector3 scale, Action onComplete = null)
        {
            return LMotion.Create(transform.localScale, scale, ScaleDuration)
                .WithEase(Ease.OutCubic)
                .WithOnComplete(onComplete)
                .Bind(value => transform.localScale = value)
                .AddTo(this);
        }

        void TryPlayBreathing()
        {
            if (!breathingEnabled || isPressed || !isShown)
            {
                return;
            }

            scaleHandle.TryCancel();
            scaleHandle = LMotion.Create(0f, Mathf.PI * 2f, BreathingCycleDuration)
                .WithEase(Ease.Linear)
                .WithLoops(-1, LoopType.Incremental)
                .Bind(angle =>
                {
                    var ratio = Mathf.Lerp(1f, BreathingScaleMultiplier, (1f - Mathf.Cos(angle)) * 0.5f);
                    transform.localScale = baseScale * ratio;
                })
                .AddTo(this);
        }
    }
}
