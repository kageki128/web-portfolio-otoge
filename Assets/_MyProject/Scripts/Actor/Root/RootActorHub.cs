using System.Collections.Generic;
using System.Linq;
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
        public Observable<int> LanePressed => otogeActorHub.LanePressed;
        public Observable<int> LaneReleased => otogeActorHub.LaneReleased;
        public Observable<Unit> AirPressed => otogeActorHub.AirPressed;
        public Observable<Unit> AirReleased => otogeActorHub.AirReleased;

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

        public void SetOtogeInputEnabled(bool enabled) => otogeActorHub.SetAcceptsInput(enabled);
        public void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores) => otogeActorHub.CreateNotes(noteCores);
        public void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed) => otogeActorHub.UpdateNotesByTimeline(timeline, currentBeat, currentScroll, scrollSpeed);
        public void ApplyOtogeTypeTransition(OtogeTypeTransition transition) => otogeActorHub.ApplyOtogeTypeTransition(transition);
        public void ExecuteOtogeEvent() => otogeActorHub.ExecuteEvent();
        public UniTask HideAndDestroyNotesAsync(CancellationToken ct) => otogeActorHub.HideAndDestroyNotesAsync(ct);
    }
}
