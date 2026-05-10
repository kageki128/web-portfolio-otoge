using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public class NoteActorHub : ActorBase
    {
        [SerializeField] NoteActorBase tapPrefab;
        [SerializeField] NoteActorBase holdPrefab;

        readonly List<NoteActorBase> noteActors = new();

        public override void Initialize()
        {
            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public void CreateNotes(IReadOnlyList<NoteCore> noteCores)
        {
            noteActors.Clear();

            foreach (var noteCore in noteCores)
            {
                NoteType type = noteCore.Property.Type;
                NoteActorBase notePrefab = type switch
                {
                    NoteType.Tap => tapPrefab,
                    NoteType.Hold => holdPrefab,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(type), $"Unsupported note type: {type}")
                };

                NoteActorBase noteActor = Instantiate(notePrefab, gameObject.transform);
                noteActor.InstallCore(noteCore);
                noteActors.Add(noteActor);
            }
        }

        public void UpdateNotesByTimeline(int timeline, float currentScroll, float scrollSpeed)
        {
            foreach (var noteActor in noteActors)
            {
                if (noteActor.NoteCore.Property.Timeline == timeline)
                {
                    noteActor.SetPosition(currentScroll, scrollSpeed);
                }
            }
        }
    }
}
