using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    public class OtogeActorHub : ActorBase
    {
        [SerializeField] OtogeActorBase tetraActor;
        [SerializeField] OtogeActorBase octaActor;
        [SerializeField] OtogeActorBase airActor;
        [SerializeField] OtogeActorBase laundryActor;
        [SerializeField] OtogeActorBase idolActor;
        [SerializeField] OtogeActorBase effectActor;
        [SerializeField] OtogeActorBase masterActor;
        [SerializeField] OtogeActorBase runActor;
        [SerializeField] OtogeActorBase scanActor;

        Dictionary<OtogeType, OtogeActorBase> otogeTypeToActor = new();

        OtogeType currentOtogeType = OtogeType.Tetra;

        public override void Initialize()
        {
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
                actor.Initialize();
            }

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
    }
}
