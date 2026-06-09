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

        const float LightDownDuration = 0.033f;
        const float PressedScaleMultiplier = 1.2f;
        const float ScaleDuration = 0.033f;

        MotionHandle backgroundHandle;
        MotionHandle textHandle;
        MotionHandle scaleHandle;
        Vector3 baseScale;

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
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            SetNormalState();
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void LightUp()
        {
            Cancel();
            background.color = backgroundLightUpColor;
            text.color = textLightUpColor;
            scaleHandle = Scale(baseScale * PressedScaleMultiplier);
        }

        public void LightDown()
        {
            Cancel();
            backgroundHandle = Fade(background, backgroundNormalColor);
            textHandle = Fade(text, textNormalColor);
            scaleHandle = Scale(baseScale);
        }

        void SetNormalState()
        {
            Cancel();
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

        MotionHandle Scale(Vector3 scale)
        {
            return LMotion.Create(transform.localScale, scale, ScaleDuration)
                .WithEase(Ease.OutCubic)
                .Bind(value => transform.localScale = value)
                .AddTo(this);
        }
    }
}
