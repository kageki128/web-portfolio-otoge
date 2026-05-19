using System.Collections.Generic;
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

        public abstract void InstallActions(OtogeActions otogeActions);

        public abstract void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores);

        public virtual void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed)
        {
            foreach (var noteActor in NoteActors)
            {
                if (noteActor.NoteCore.Property.Timeline == timeline)
                {
                    noteActor.SetPosition(currentBeat, currentScroll, scrollSpeed);
                }
            }
        }

        public void DestroyNotes()
        {
            foreach (var noteActor in NoteActors)
            {
                noteActor.Destroy();
            }
            NoteActors.Clear();
        }
    }
}
