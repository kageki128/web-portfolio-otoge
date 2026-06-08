using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using MyProject.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Actor
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class OtogeTypeGaugeActor : ActorBase
    {
        const float GaugeAnimationDuration = 0.1f;

        [SerializeField] Image gaugeImage;

        StandardTransitionAnimator transitionAnimator;
        MotionHandle gaugeHandle;
        float displayedFillAmount;

        public override void Initialize()
        {
            transitionAnimator = GetComponent<StandardTransitionAnimator>();
            transitionAnimator.Initialize();

            gaugeImage.type = Image.Type.Filled;
            SetFillAmount(0f);
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await transitionAnimator.ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            gaugeHandle.TryCancel();
            await transitionAnimator.HideAsync(ct);
            gameObject.SetActive(false);
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
            if (!transition.HasNextType || transition.DurationBeat <= 1f || transition.RemainingBeat < 1f)
            {
                return 0f;
            }

            return Mathf.Clamp01((transition.DurationBeat - transition.RemainingBeat) / (transition.DurationBeat - 1f));
        }

        void SetFillAmount(float fillAmount)
        {
            displayedFillAmount = fillAmount;
            gaugeImage.fillAmount = fillAmount;
        }
    }
}
