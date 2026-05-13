using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class OtogeActorHub : ActorBase
    {
        public Observable<int> LanePressed;
        public Observable<int> LaneReleased;
        public Observable<Unit> AirPressed;
        public Observable<Unit> AirReleased;

        [Header("Otoge Actors")]
        [SerializeField] TetraActor tetraActor;
        [SerializeField] OctaActor octaActor;
        [SerializeField] AirActor airActor;
        [SerializeField] LaundryActor laundryActor;
        [SerializeField] IdolActor idolActor;
        [SerializeField] EffectActor effectActor;
        [SerializeField] MasterActor masterActor;
        [SerializeField] RunActor runActor;
        [SerializeField] ScanActor scanActor;

        [Header("Shared Actors")]
        [SerializeField] OtogeSharedActorBase[] sharedActors;

        [Header("Settings")]
        [SerializeField] OtogeType currentOtogeType = OtogeType.Tetra;

        OtogeActions otogeActions;
        Dictionary<OtogeType, OtogeActorBase> otogeTypeToActor = new();



        public override void Initialize()
        {
            otogeActions = new OtogeActions();
            otogeTypeToActor = new Dictionary<OtogeType, OtogeActorBase>
            {
                { OtogeType.Tetra, tetraActor },
                { OtogeType.Octa, octaActor },
                { OtogeType.Air, airActor },
                { OtogeType.Laundry, laundryActor },
                { OtogeType.Idol, idolActor },
                { OtogeType.Effect, effectActor },
                { OtogeType.Master, masterActor },
                { OtogeType.Run, runActor },
                { OtogeType.Scan, scanActor },
            };
            foreach (var actor in otogeTypeToActor.Values)
            {
                actor.InstallActions(otogeActions);
                actor.Initialize();
            }

            LanePressed = Observable.Merge(otogeTypeToActor.Values.Select(actor => actor.LanePressed));
            LaneReleased = Observable.Merge(otogeTypeToActor.Values.Select(actor => actor.LaneReleased));
            AirPressed = Observable.Merge(otogeTypeToActor.Values.Select(actor => actor.AirPressed));
            AirReleased = Observable.Merge(otogeTypeToActor.Values.Select(actor => actor.AirReleased));

            DestroyNotes();

            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await otogeTypeToActor[currentOtogeType].ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await otogeTypeToActor[currentOtogeType].HideAsync(ct);

            DestroyNotes();

            gameObject.SetActive(false);
        }

        public void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
            foreach (var actor in otogeTypeToActor.Values)
            {
                actor.CreateNotes(noteCores);
            }
        }

        public void UpdateNotesByTimeline(int timeline, float currentScroll, float scrollSpeed)
        {
            otogeTypeToActor[currentOtogeType].UpdateNotesByTimeline(timeline, currentScroll, scrollSpeed);
        }

        public void SetSharedActorsState(OtogeType otogeType)
        {
            foreach (var sharedActor in sharedActors)
            {
                if (sharedActor == null) continue;
                sharedActor.SetState(otogeType);
            }
        }

        void DestroyNotes()
        {
            foreach (var actor in otogeTypeToActor.Values)
            {
                actor.DestroyNotes();
            }
        }

        async UniTask SwitchOtogeTypeAsync(OtogeType newType, CancellationToken ct)
        {
            await UniTask.WhenAll
            (
                otogeTypeToActor[currentOtogeType].HideAsync(ct),
                otogeTypeToActor[newType].ShowAsync(ct)
            );

            currentOtogeType = newType;
        }
    }
}
