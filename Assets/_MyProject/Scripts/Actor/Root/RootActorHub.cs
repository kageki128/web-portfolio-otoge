using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class RootActorHub : MonoBehaviour
    {
        [SerializeField] StandardSliderActor scrollSpeedSlider;
        public Observable<float> ScrollSpeedNormalizedChanged => scrollSpeedSlider.ValueChanged;

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            scrollSpeedSlider.Initialize();
            await scrollSpeedSlider.InitialShowAsync(ct);
        }

        public void SetScrollSpeedNormalized(float normalizedValue)
        {
            scrollSpeedSlider.SetValue(normalizedValue);
        }
    }
}
