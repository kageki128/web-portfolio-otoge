using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class LaneKeysActor : ActorBase
    {
        [SerializeField] List<TMP_Text> laneKeyTexts;
        [SerializeField] TMP_Text airKeyText;
        [SerializeField] SpriteRenderer airKeyRenderer;

        const float FadeDuration = 0.033f;
        const float ActiveAlpha = 1f;

        MotionHandle[] laneHandles;
        MotionHandle airTextHandle;
        MotionHandle airRendererHandle;
        Color[] baseLaneColors;
        Color baseAirTextColor;
        Color baseAirRendererColor;

        public override void Initialize()
        {
            baseLaneColors = new Color[laneKeyTexts.Count];
            for (var i = 0; i < laneKeyTexts.Count; i++)
            {
                baseLaneColors[i] = laneKeyTexts[i].color;
            }

            if (airKeyText != null)
            {
                baseAirTextColor = airKeyText.color;
            }

            if (airKeyRenderer != null)
            {
                baseAirRendererColor = airKeyRenderer.color;
            }

            laneHandles = new MotionHandle[laneKeyTexts.Count];
            gameObject.SetActive(false);
        }

        public override UniTask HideAsync(CancellationToken ct)
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            for (var i = 0; i < laneKeyTexts.Count; i++)
            {
                laneKeyTexts[i].color = baseLaneColors[i];
            }

            if (airKeyText != null)
            {
                airKeyText.color = baseAirTextColor;
            }

            if (airKeyRenderer != null)
            {
                airKeyRenderer.color = baseAirRendererColor;
            }

            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public void LightUpLane(int lane)
        {
            if (lane < 0 || lane >= laneKeyTexts.Count) return;

            laneHandles[lane].TryCancel();
            laneKeyTexts[lane].color = WithAlpha(baseLaneColors[lane], ActiveAlpha);
        }

        public void LightUpAir()
        {
            airTextHandle.TryCancel();
            airRendererHandle.TryCancel();
            airKeyText.color = WithAlpha(baseAirTextColor, ActiveAlpha);
            airKeyRenderer.color = WithAlpha(baseAirRendererColor, ActiveAlpha);
        }

        public void LightDownLane(int lane)
        {
            if (lane < 0 || lane >= laneKeyTexts.Count) return;

            laneHandles[lane].TryCancel();
            laneHandles[lane] = FadeToInactive(laneKeyTexts[lane], baseLaneColors[lane]);
        }

        public void LightDownAir()
        {
            airTextHandle.TryCancel();
            airRendererHandle.TryCancel();
            airTextHandle = FadeToInactive(airKeyText, baseAirTextColor);
            airRendererHandle = FadeToInactive(airKeyRenderer, baseAirRendererColor);
        }

        MotionHandle FadeToInactive(TMP_Text text, Color baseColor)
        {
            return LMotion.Create(text.color, baseColor, FadeDuration)
                .Bind(value => text.color = value)
                .AddTo(this);
        }

        MotionHandle FadeToInactive(SpriteRenderer renderer, Color baseColor)
        {
            return LMotion.Create(renderer.color, baseColor, FadeDuration)
                .Bind(value => renderer.color = value)
                .AddTo(this);
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
