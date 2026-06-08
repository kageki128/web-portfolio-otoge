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
    public class GameActorHub : SceneActorHubBase
    {
        public Observable<Unit> Quit => gameActionsObserver.Quit;

        [SerializeField] OtogeTypeGaugeActor otogeTypeGaugeActor;
        [SerializeField] ScoreTextActor scoreTextActor;
        [SerializeField] ComboTextActor comboTextActor;
        [SerializeField] JudgeTextActor judgeTextActor;
        [SerializeField] MusicTextActor musicTextActor;

        ActorAnimationTimeline animationTimeline;
        GameActionsObserver gameActionsObserver;

        [Inject]
        public void Construct(GameActionsObserver gameActionsObserver)
        {
            this.gameActionsObserver = gameActionsObserver;
        }

        public override void Initialize()
        {
            animationTimeline = GetComponent<ActorAnimationTimeline>();

            gameActionsObserver.Disable();
            animationTimeline.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animationTimeline.ShowAsync(ct);
            gameActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            gameActionsObserver.Disable();
            AudioPlayer.Instance.StopBgm();
            await animationTimeline.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void PlayWave(AudioClip clip, double scheduledDspTime)
        {
            AudioPlayer.Instance.PlayBgm(clip, scheduledDspTime, false);
        }

        public void ApplyOtogeTypeGaugeTransition(OtogeTypeTransition transition) => otogeTypeGaugeActor.ApplyTransition(transition);

        public void SetScore(int score) => scoreTextActor.SetScore(score);
        public void SetCombo(int combo) => comboTextActor.SetCombo(combo);
        public void SetJudgeCounts(IReadOnlyDictionary<JudgeType, int> judgeCounts) => judgeTextActor.SetJudgeCounts(judgeCounts);
        public void SetMetaData(BeatmapMetaData metaData) => musicTextActor.SetMetaData(metaData);
    }
}
