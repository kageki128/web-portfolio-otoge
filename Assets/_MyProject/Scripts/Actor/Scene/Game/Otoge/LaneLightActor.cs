using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using LitMotion;

namespace MyProject.Actor
{
    public class LaneLightActor : ActorBase
    {
        [SerializeField] List<SpriteRenderer> laneLightRenderers;

        readonly Color activeColor = new(1f, 1f, 1f, 1f);
        readonly Color inactiveColor = new(1f, 1f, 1f, 0f);
        readonly float fadeDuration = 0.033f;

        MotionHandle[] laneHandles;

        public override void Initialize()
        {
            laneHandles = new MotionHandle[laneLightRenderers.Count];
            gameObject.SetActive(false);
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            foreach (var renderer in laneLightRenderers)
            {
                renderer.color = inactiveColor;
            }
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public void LightUp(int lane)
        {
            if (lane < 0 || lane >= laneLightRenderers.Count) return;

            var renderer = laneLightRenderers[lane];
            var currentHandle = laneHandles[lane];

            // 既存のハンドルがあればキャンセル
            if (currentHandle != null)
            {
                currentHandle.TryCancel();
            }

            // 即時でアクティブ色にする
            renderer.color = activeColor;
        }

        public void LightDown(int lane)
        {
            if (lane < 0 || lane >= laneLightRenderers.Count) return;

            var renderer = laneLightRenderers[lane];
            var currentHandle = laneHandles[lane];

            // 既存のハンドルがあればキャンセル
            if (currentHandle != null)
            {
                currentHandle.TryCancel();
            }

            // フェードを開始（現在の色 -> inactiveColor）。
            var newHandle = LMotion.Create(renderer.color, inactiveColor, fadeDuration)
                .Bind(value => renderer.color = value)
                .AddTo(this);

            laneHandles[lane] = newHandle;
        }
    }
}
