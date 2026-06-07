using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class OtogeActorHub : RootActorBase
    {
        const float SwitchToNextTypeRemainingBeatThreshold = 1f;

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

        readonly HashSet<OtogeType> updatedOtogeTypes = new();
        OtogeType currentOtogeType = OtogeType.Tetra;
        bool hasAppliedOtogeTypeTransition = false;
        OtogeActions otogeActions;
        Dictionary<OtogeType, OtogeActorBase> otogeTypeToActor = new();
        CancellationTokenSource switchOtogeTypeCts;

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

            updatedOtogeTypes.Clear();
            updatedOtogeTypes.Add(currentOtogeType);
            hasAppliedOtogeTypeTransition = false;

            gameObject.SetActive(true);
        }

        public override UniTask TransitSceneAsync(SceneType sceneType, CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores)
        {
            // ノーツを作成
            foreach (var actor in otogeTypeToActor.Values)
            {
                actor.CreateNotes(noteCores);
            }
        }

        public void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed)
        {
            foreach (var otogeType in updatedOtogeTypes)
            {
                otogeTypeToActor[otogeType].UpdateNotesByTimeline(timeline, currentBeat, currentScroll, scrollSpeed);
            }
        }

        public void ApplyOtogeTypeTransition(OtogeTypeTransition transition)
        {
            var switchTargetType = GetSwitchTargetType(transition);
            if (hasAppliedOtogeTypeTransition && switchTargetType == currentOtogeType)
            {
                return;
            }

            hasAppliedOtogeTypeTransition = true;
            SwitchOtogeType(switchTargetType);
        }

        public UniTask HideAndDestroyNotesAsync(CancellationToken ct)
        {
            var tasks = new List<UniTask>();
            foreach (var actor in otogeTypeToActor.Values)
            {
                tasks.Add(actor.HideAndDestroyNotesAsync(ct));
            }

            return UniTask.WhenAll(tasks);
        }

        void DestroyNotes()
        {
            foreach (var actor in otogeTypeToActor.Values)
            {
                actor.DestroyNotes();
            }
        }

        static OtogeType GetSwitchTargetType(OtogeTypeTransition transition)
        {
            return transition.RemainingBeat <= SwitchToNextTypeRemainingBeatThreshold
                ? transition.NextType ?? transition.CurrentType
                : transition.CurrentType;
        }

        void SwitchOtogeType(OtogeType newType)
        {
            switchOtogeTypeCts?.Cancel();
            switchOtogeTypeCts?.Dispose();
            switchOtogeTypeCts = new CancellationTokenSource();
            ExecuteSwitchOtogeTypeAsync(newType, switchOtogeTypeCts.Token).Forget();
        }

        public void ExecuteEvent()
        {
            otogeTypeToActor[currentOtogeType].ExecuteEvent().Forget();
        }

        public void SetSharedActorsState(OtogeType otogeType)
        {
            foreach (var sharedActor in sharedActors)
            {
                if (sharedActor == null) continue;
                sharedActor.SetState(otogeType);
            }
        }

        async UniTask ExecuteSwitchOtogeTypeAsync(OtogeType newType, CancellationToken ct)
        {
            var oldType = currentOtogeType;

            if (newType == oldType)
            {
                await UniTask.WhenAll
                (
                    otogeTypeToActor[newType].ShowAsync(ct),
                    SetSharedActorsStateAsync(newType, ct)
                );
                return;
            }

            currentOtogeType = newType;

            updatedOtogeTypes.Add(newType);

            await UniTask.WhenAll
            (
                otogeTypeToActor[oldType].HideAsync(ct),
                otogeTypeToActor[newType].ShowAsync(ct),
                SetSharedActorsStateAsync(newType, ct)
            );

            updatedOtogeTypes.Remove(oldType);
        }

        async UniTask SetSharedActorsStateAsync(OtogeType otogeType, CancellationToken ct)
        {
            var tasks = new List<UniTask>();
            foreach (var sharedActor in sharedActors)
            {
                if (sharedActor == null) continue;
                tasks.Add(sharedActor.SetStateAsync(otogeType, ct));
            }

            await UniTask.WhenAll(tasks);
        }

        void OnDestroy()
        {
            switchOtogeTypeCts?.Cancel();
            switchOtogeTypeCts?.Dispose();
            switchOtogeTypeCts = null;
        }
    }
}
