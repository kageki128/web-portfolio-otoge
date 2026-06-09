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

        MotionHandle backgroundHandle;
        MotionHandle textHandle;

        public override void Initialize()
        {
            SetNormalColor();
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            SetNormalColor();
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            Cancel();
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void LightUp()
        {
            Cancel();
            background.color = backgroundLightUpColor;
            text.color = textLightUpColor;
        }

        public void LightDown()
        {
            Cancel();
            backgroundHandle = Fade(background, backgroundNormalColor);
            textHandle = Fade(text, textNormalColor);
        }

        void SetNormalColor()
        {
            Cancel();
            background.color = backgroundNormalColor;
            text.color = textNormalColor;
        }

        void Cancel()
        {
            backgroundHandle.TryCancel();
            textHandle.TryCancel();
        }

        MotionHandle Fade(Graphic graphic, Color color)
        {
            return LMotion.Create(graphic.color, color, LightDownDuration)
                .Bind(value => graphic.color = value)
                .AddTo(this);
        }
    }
}
