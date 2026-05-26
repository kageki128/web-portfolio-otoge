using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScoreTextActor : ActorBase
    {
        const float ScoreAnimationDuration = 0.3f;

        [SerializeField] TMP_Text text;

        MotionHandle scoreHandle;
        float displayedScore;

        public override void Initialize()
        {
            displayedScore = 0f;
            SetScoreText(0);
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
