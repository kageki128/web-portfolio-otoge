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
        public Observable<int> LanePressed => otogeActorHub.LanePressed;
        public Observable<int> LaneReleased => otogeActorHub.LaneReleased;
        public Observable<Unit> AirPressed => otogeActorHub.AirPressed;
        public Observable<Unit> AirReleased => otogeActorHub.AirReleased;

        [SerializeField] OtogeActorHub otogeActorHub;
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
            otogeActorHub.Initialize();
            scoreTextActor.Initialize();
            comboTextActor.Initialize();
            judgeTextActor.Initialize();
            musicTextActor.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask InitialShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll
            (
                animationTimeline.InitialShowAsync(ct),
                otogeActorHub.InitialShowAsync(ct),
                scoreTextActor.InitialShowAsync(ct),
                comboTextActor.InitialShowAsync(ct),
                judgeTextActor.InitialShowAsync(ct),
                musicTextActor.InitialShowAsync(ct)
            );
            gameActionsObserver.Enable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll
            (
                animationTimeline.ShowAsync(ct),
                otogeActorHub.ShowAsync(ct),
                scoreTextActor.ShowAsync(ct),
                comboTextActor.ShowAsync(ct),
                judgeTextActor.ShowAsync(ct),
                musicTextActor.ShowAsync(ct)
            );
            gameActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            gameActionsObserver.Disable();
            AudioPlayer.Instance.StopBgm();
            await UniTask.WhenAll
            (
                animationTimeline.HideAsync(ct),
                otogeActorHub.HideAsync(ct),
                scoreTextActor.HideAsync(ct),
                comboTextActor.HideAsync(ct),
                judgeTextActor.HideAsync(ct),
                musicTextActor.HideAsync(ct)
            );
            gameObject.SetActive(false);
        }

        public void PlayWave(AudioClip clip, double scheduledDspTime)
        {
            AudioPlayer.Instance.PlayBgm(clip, scheduledDspTime, false);
        }

        public void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores) => otogeActorHub.CreateNotes(noteCores);
        public void UpdateNotesByTimeline(int timeline, float currentBeat, float currentScroll, float scrollSpeed) => otogeActorHub.UpdateNotesByTimeline(timeline, currentBeat, currentScroll, scrollSpeed);
        public void SwitchOtogeType(OtogeType otogeType) => otogeActorHub.SwitchOtogeType(otogeType);

        public void SetScore(int score) => scoreTextActor.SetScore(score);
        public void SetCombo(int combo) => comboTextActor.SetCombo(combo);
        public void SetJudgeCounts(IReadOnlyDictionary<JudgeType, int> judgeCounts) => judgeTextActor.SetJudgeCounts(judgeCounts);
        public void SetMetaData(BeatmapMetaData metaData) => musicTextActor.SetMetaData(metaData);
    }
}
