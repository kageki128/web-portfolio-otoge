using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class MusicTextActor : ActorBase
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text artistText;

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

        public void SetMetaData(BeatmapMetaData metaData)
        {
            titleText.text = $"♪{metaData.Title}";
            artistText.text = $"{metaData.Artist}";
        }
    }
}
