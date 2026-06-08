using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class ScoreTextActor : ActorBase
    {
        const float ScoreAnimationDuration = 0.3f;

        [SerializeField] TMP_Text text;

        StandardTransitionAnimator transitionAnimator;
        MotionHandle scoreHandle;
        float displayedScore;

        public override void Initialize()
        {
            transitionAnimator = GetComponent<StandardTransitionAnimator>();
            transitionAnimator.Initialize();

            displayedScore = 0f;
            SetScoreText(0);
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await transitionAnimator.ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await transitionAnimator.HideAsync(ct);
            gameObject.SetActive(false);
        }

        public void SetScore(int score)
        {
            if (scoreHandle != null)
            {
                scoreHandle.TryCancel();
            }

            scoreHandle = LMotion.Create(displayedScore, score, ScoreAnimationDuration)
                .WithEase(Ease.OutCubic)
                .Bind(value =>
                {
                    displayedScore = value;
                    SetScoreText(Mathf.RoundToInt(value));
                })
                .AddTo(this);
        }

        void SetScoreText(int score)
        {
            text.text = $"{score:D7}";
        }
    }
}
