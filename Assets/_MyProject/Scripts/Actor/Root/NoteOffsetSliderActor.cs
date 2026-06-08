using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class NoteOffsetSliderActor : RootActorBase
    {
        public Observable<float> NoteOffsetNormalizedChanged => sliderActor.ValueChanged;
        public Observable<Unit> NoteOffsetResetRequested => sliderActor.HandleDoubleClicked;

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

        public void SetNoteOffsetNormalized(float normalizedValue)
        {
            sliderActor.SetValue(normalizedValue);
        }

        public void SetNoteOffset(float noteOffset)
        {
            valueText.text = Mathf.RoundToInt(noteOffset * 1000f).ToString();
        }
    }
}
