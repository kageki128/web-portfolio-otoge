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
        public Observable<Unit> ToSelectButtonClicked = Observable.Empty<Unit>();
        public Observable<int> LanePressed => gameActionsObserver.LanePressed;

        [SerializeField] NoteActorHub noteActorHub;
        [SerializeField] ScoreTextActor scoreTextActor;
        [SerializeField] ComboTextActor comboTextActor;
        [SerializeField] JudgeTextActor judgeTextActor;

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
            scoreTextActor.Initialize();
            comboTextActor.Initialize();
            judgeTextActor.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask InitialShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll
            (
                animationTimeline.InitialShowAsync(ct),
                scoreTextActor.InitialShowAsync(ct),
                comboTextActor.InitialShowAsync(ct),
                judgeTextActor.InitialShowAsync(ct)
            );
            gameActionsObserver.Enable();
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll
            (
                animationTimeline.ShowAsync(ct),
                scoreTextActor.ShowAsync(ct),
                comboTextActor.ShowAsync(ct),
                judgeTextActor.ShowAsync(ct)
            );
            gameActionsObserver.Enable();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            gameActionsObserver.Disable();
            AudioPlayer.Instance.StopBgm();
            await UniTask.WhenAll
            (
                scoreTextActor.HideAsync(ct),
                comboTextActor.HideAsync(ct),
                judgeTextActor.HideAsync(ct),
                animationTimeline.HideAsync(ct)
            );
            gameObject.SetActive(false);
        }

        public void PlayWave(AudioClip clip, double scheduledDspTime)
        {
            AudioPlayer.Instance.PlayBgm(clip, scheduledDspTime, false);
        }

        public void SetScore(int score) => scoreTextActor.SetScore(score);
        public void SetCombo(int combo) => comboTextActor.SetCombo(combo);
        public void SetJudgeCounts(IReadOnlyDictionary<JudgeType, int> judgeCounts) => judgeTextActor.SetJudgeCounts(judgeCounts);
        public void CreateNotes(IReadOnlyList<NoteCoreBase> noteCores) => noteActorHub.CreateNotes(noteCores);
        public void UpdateNotesByTimeline(int timeline, float currentScroll, float scrollSpeed) => noteActorHub.UpdateNotesByTimeline(timeline, currentScroll, scrollSpeed);
    }
}
