using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;

namespace MyProject.Actor
{
    public abstract class OtogeActorBase : ActorBase
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;
        public Observable<Unit> AirPressed;
        public Observable<Unit> AirReleased;

        protected readonly List<NoteActorBase> NoteActors = new();
        protected abstract OtogeType ActorOtogeType { get; }
        protected virtual float ScrollSpeedMultiplier => 1f;
        protected ActionsObserverBase ActionsObserver { get; set; }

        bool acceptsInput;

        public abstract void InstallActions(OtogeActions otogeActions);

        public abstract void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores);

        public void SetAcceptsInput(bool acceptsInput)
        {
            this.acceptsInput = acceptsInput;
            if (!acceptsInput)
            {
                DisableActions();
            }
        }

        public void EnableAcceptedActions()
        {
            EnableActionsIfAccepted();
        }

        protected bool IsOwnedNote(NoteCoreBase noteCore)
        {
            return noteCore.Property.OtogeType == ActorOtogeType;
        }

        public virtual void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed)
        {
            var adjustedScrollSpeed = scrollSpeed * ScrollSpeedMultiplier;
            foreach (var noteActor in NoteActors)
            {
                if (noteActor.NoteCore.Property.Timeline == timeline)
                {
                    noteActor.SetPosition(currentBeat, currentScroll, adjustedScrollSpeed);
                }
            }
        }

        public virtual UniTask ExecuteEvent()
        {
            return UniTask.CompletedTask;
        }

        public void DestroyNotes()
        {
            foreach (var noteActor in NoteActors)
            {
                noteActor.Destroy();
            }
            NoteActors.Clear();
        }

        public async UniTask HideAndDestroyNotesAsync(CancellationToken ct)
        {
            var tasks = new List<UniTask>();
            foreach (var noteActor in NoteActors)
            {
                tasks.Add(noteActor.HideAsync(ct));
            }

            await UniTask.WhenAll(tasks);
            DestroyNotes();
        }

        protected static async UniTask SwitchActionsAfterDelayAsync(Action switchActions, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(OtogeAppearance.SwitchActionsDelay), cancellationToken: ct);
            switchActions();
        }

        protected void EnableActionsIfAccepted()
        {
            if (acceptsInput)
            {
                ActionsObserver.Enable();
            }
        }

        protected void DisableActions()
        {
            ActionsObserver.Disable();
        }
    }
}
