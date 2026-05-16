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

        readonly float fadeDuration = 0.033f;

        MotionHandle[] laneHandles;
        Color[] baseLaneColors;

        public override void Initialize()
        {
            baseLaneColors = new Color[laneLightRenderers.Count];
            for (var i = 0; i < laneLightRenderers.Count; i++)
            {
                baseLaneColors[i] = laneLightRenderers[i].color;
            }

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
            for (var i = 0; i < laneLightRenderers.Count; i++)
            {
                laneLightRenderers[i].color = WithAlpha(baseLaneColors[i], 0f);
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

            renderer.color = baseLaneColors[lane];
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

            var targetColor = WithAlpha(baseLaneColors[lane], 0f);
            var newHandle = LMotion.Create(renderer.color, targetColor, fadeDuration)
                .Bind(value => renderer.color = value)
                .AddTo(this);

            laneHandles[lane] = newHandle;
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
