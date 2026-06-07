using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class RootActorHub : RootActorBase
    {
        [SerializeField] OtogeActorHub otogeActorHub;
        [SerializeField] ScrollSpeedSliderActor scrollSpeedSliderActor;
        [SerializeField] NoteOffsetSliderActor noteOffsetSliderActor;

        public Observable<float> ScrollSpeedNormalizedChanged => scrollSpeedSliderActor.ScrollSpeedNormalizedChanged;
        public Observable<float> NoteOffsetNormalizedChanged => noteOffsetSliderActor.NoteOffsetNormalizedChanged;

        readonly List<RootActorBase> rootActors = new();

        public override void Initialize()
        {
            gameObject.SetActive(true);

            rootActors.Clear();
            rootActors.Add(scrollSpeedSliderActor);
            rootActors.Add(noteOffsetSliderActor);
            rootActors.Add(otogeActorHub);

            foreach (var rootActor in rootActors)
            {
                rootActor.Initialize();
            }
        }

        public override UniTask TransitSceneAsync(SceneType sceneType, CancellationToken ct)
        {
            return UniTask.WhenAll(rootActors.Select(actor => actor.TransitSceneAsync(sceneType, ct)));
        }

        public void SetScrollSpeedNormalized(float normalizedValue) => scrollSpeedSliderActor.SetScrollSpeedNormalized(normalizedValue);
        public void SetNoteOffsetNormalized(float normalizedValue) => noteOffsetSliderActor.SetNoteOffsetNormalized(normalizedValue);

        public void SetScrollSpeed(float scrollSpeed) => scrollSpeedSliderActor.SetScrollSpeed(scrollSpeed);
        public void SetNoteOffset(float noteOffset) => noteOffsetSliderActor.SetNoteOffset(noteOffset);
    }
}
