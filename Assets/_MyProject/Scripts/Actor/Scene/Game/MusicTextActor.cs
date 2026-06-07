using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(FadeAnimator))]
    public class MusicTextActor : ActorBase
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text artistText;

        FadeAnimator animator;

        public override void Initialize()
        {
            animator = GetComponent<FadeAnimator>();
            animator.Initialize();

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

        public void SetMetaData(BeatmapMetaData metaData)
        {
            titleText.text = $"♪{metaData.Title}";
            artistText.text = $"{metaData.Artist}";
        }
    }
}
