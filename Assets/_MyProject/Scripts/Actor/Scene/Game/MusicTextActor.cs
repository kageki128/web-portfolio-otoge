using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    [RequireComponent(typeof(StandardTransitionAnimator))]
    public class MusicTextActor : ActorBase
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text artistText;

        StandardTransitionAnimator transitionAnimator;

        public override void Initialize()
        {
            transitionAnimator = GetComponent<StandardTransitionAnimator>();
            transitionAnimator.Initialize();

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

        public void SetMetaData(BeatmapMetaData metaData)
        {
            titleText.text = $"♪{metaData.Title}";
            artistText.text = $"{metaData.Artist}";
        }
    }
}
