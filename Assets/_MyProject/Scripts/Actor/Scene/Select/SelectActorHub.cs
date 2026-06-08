using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;
using VContainer;

namespace MyProject.Actor
{
    [RequireComponent(typeof(ActorAnimationTimeline))]
    public class SelectActorHub : SceneActorHubBase
    {
        public Observable<Unit> StartGame => selectActionsObserver.StartGame;

        [SerializeField] OtogeActorHub otogeActorHub;

        ActorAnimationTimeline animationTimeline;
        SelectActionsObserver selectActionsObserver;

        [Inject]
        public void Construct(SelectActionsObserver selectActionsObserver)
        {
            this.selectActionsObserver = selectActionsObserver;
        }

        public override void Initialize()
        {
            animationTimeline = GetComponent<ActorAnimationTimeline>();

            selectActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            selectActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            selectActionsObserver.Disable();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores) => otogeActorHub.CreateNotes(noteCores);
        public void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed) => otogeActorHub.UpdateNotesByTimeline(timeline, currentBeat, currentScroll, scrollSpeed);
        public void ApplyOtogeTypeTransition(OtogeTypeTransition transition) => otogeActorHub.ApplyOtogeTypeTransition(transition);
        public void ExecuteOtogeEvent() => otogeActorHub.ExecuteEvent();
        public UniTask HideAndDestroyNotesAsync(CancellationToken ct) => otogeActorHub.HideAndDestroyNotesAsync(ct);
    }
}
