using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Actor
{
    public class OtogeTypeGaugeActor : ActorBase
    {
        const float GaugeAnimationDuration = 0.1f;

        [SerializeField] Image gaugeImage;

        MotionHandle gaugeHandle;
        float displayedFillAmount;

        public override void Initialize()
        {
            gaugeImage.type = Image.Type.Filled;
            SetFillAmount(0f);
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            gaugeHandle.TryCancel();
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void ApplyTransition(OtogeTypeTransition transition)
        {
            var fillAmount = GetFillAmount(transition);

            gaugeHandle.TryCancel();
            gaugeHandle = LMotion.Create(displayedFillAmount, fillAmount, GaugeAnimationDuration)
                .Bind(SetFillAmount)
                .AddTo(this);
        }

        static float GetFillAmount(OtogeTypeTransition transition)
        {
            return transition.DurationSec <= 0f
                ? 0f
                : Mathf.Clamp01(1f - transition.RemainingSec / transition.DurationSec);
        }

        void SetFillAmount(float fillAmount)
        {
            displayedFillAmount = fillAmount;
            gaugeImage.fillAmount = fillAmount;
        }
    }
}
