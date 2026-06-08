using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScrollSpeedSliderActor : RootActorBase
    {
        public Observable<float> ScrollSpeedNormalizedChanged => sliderActor.ValueChanged;

        [SerializeField] StandardSliderActor sliderActor;
        [SerializeField] TMP_Text valueText;

        public override void Initialize()
        {
            gameObject.SetActive(true);
            sliderActor.Initialize();
            sliderActor.gameObject.SetActive(true);
        }

        public override UniTask TransitSceneAsync(SceneType sceneType, CancellationToken ct)
        {
            gameObject.SetActive(true);
            sliderActor.gameObject.SetActive(true);
            return UniTask.CompletedTask;
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
