using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScrollSpeedSliderActor : ActorBase
    {
        public Observable<float> ScrollSpeedNormalizedChanged => sliderActor.ValueChanged;

        [SerializeField] StandardSliderActor sliderActor;
        [SerializeField] TMP_Text valueText;

        public override void Initialize()
        {
            sliderActor.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await sliderActor.InitialShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await sliderActor.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void SetScrollSpeedNormalized(float normalizedValue)
        {
            sliderActor.SetValue(normalizedValue);
        }

        public void SetScrollSpeed(float scrollSpeed)
        {
            valueText.text = $"{scrollSpeed:F1}";
        }
    }
}
