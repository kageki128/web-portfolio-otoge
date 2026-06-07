using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(FadeAnimator))]
    public class ScoreTextActor : ActorBase
    {
        const float ScoreAnimationDuration = 0.3f;

        [SerializeField] TMP_Text text;

        FadeAnimator animator;
        MotionHandle scoreHandle;
        float displayedScore;

        public override void Initialize()
        {
            animator = GetComponent<FadeAnimator>();
            animator.Initialize();

            displayedScore = 0f;
            SetScoreText(0);
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await animator.ShowAsync(ct);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await animator.HideAsync(ct);
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
