using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class RootActorHub : MonoBehaviour
    {
        [SerializeField] ScrollSpeedSliderActor scrollSpeedSliderActor;
        [SerializeField] NoteOffsetSliderActor noteOffsetSliderActor;
        public Observable<float> ScrollSpeedNormalizedChanged => scrollSpeedSliderActor.ScrollSpeedNormalizedChanged;
        public Observable<float> NoteOffsetNormalizedChanged => noteOffsetSliderActor.NoteOffsetNormalizedChanged;

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            scrollSpeedSliderActor.Initialize();
            noteOffsetSliderActor.Initialize();
            await scrollSpeedSliderActor.InitialShowAsync(ct);
            await noteOffsetSliderActor.InitialShowAsync(ct);
        }

        public void SetScrollSpeedNormalized(float normalizedValue) => scrollSpeedSliderActor.SetScrollSpeedNormalized(normalizedValue);
        public void SetNoteOffsetNormalized(float normalizedValue) => noteOffsetSliderActor.SetNoteOffsetNormalized(normalizedValue);

        public void SetScrollSpeed(float scrollSpeed) => scrollSpeedSliderActor.SetScrollSpeed(scrollSpeed);
        public void SetNoteOffset(float noteOffset) => noteOffsetSliderActor.SetNoteOffset(noteOffset);
    }
}
