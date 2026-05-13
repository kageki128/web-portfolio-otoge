using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class ScoreTextActor : ActorBase
    {
        [SerializeField] TMP_Text text;

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

        public void SetScore(int score)
        {
            // 7桁表示にする
            text.text = $"{score:D7}";
        }
    }
}
