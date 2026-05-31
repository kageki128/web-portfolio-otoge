using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class NoteOffsetSliderActor : ActorBase
    {
        public Observable<float> NoteOffsetNormalizedChanged => sliderActor.ValueChanged;

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

        public void SetNoteOffsetNormalized(float normalizedValue)
        {
            sliderActor.SetValue(normalizedValue);
        }

        public void SetNoteOffset(float noteOffset)
        {
            valueText.text = $"{noteOffset:F2}";
        }
    }
}
