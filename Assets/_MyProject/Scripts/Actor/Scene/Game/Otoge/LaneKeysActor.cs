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
        [SerializeField] Transform airKeyParent;

        const float FadeDuration = 0.033f;
        const float ActiveAlpha = 1f;
        const float ActiveScaleMultiplier = 1.1f;

        MotionHandle[] laneColorHandles;
        MotionHandle[] laneScaleHandles;
        MotionHandle airTextHandle;
        MotionHandle airRendererHandle;
        MotionHandle airScaleHandle;
        Color[] baseLaneColors;
        Vector3[] baseLaneScales;
        Color baseAirTextColor;
        Color baseAirRendererColor;
        Vector3 baseAirScale;

        public override void Initialize()
        {
            baseLaneColors = new Color[laneKeyTexts.Count];
            baseLaneScales = new Vector3[laneKeyTexts.Count];
            for (var i = 0; i < laneKeyTexts.Count; i++)
            {
                baseLaneColors[i] = laneKeyTexts[i].color;
                baseLaneScales[i] = laneKeyTexts[i].transform.localScale;
            }

            if (airKeyText != null)
            {
                baseAirTextColor = airKeyText.color;
            }

            if (airKeyRenderer != null)
            {
                baseAirRendererColor = airKeyRenderer.color;
            }

            if (airKeyParent != null)
            {
                baseAirScale = airKeyParent.localScale;
            }

            laneColorHandles = new MotionHandle[laneKeyTexts.Count];
            laneScaleHandles = new MotionHandle[laneKeyTexts.Count];
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
                laneKeyTexts[i].transform.localScale = baseLaneScales[i];
            }

            if (airKeyText != null)
            {
                airKeyText.color = baseAirTextColor;
            }

            if (airKeyRenderer != null)
            {
                airKeyRenderer.color = baseAirRendererColor;
            }

            if (airKeyParent != null)
            {
                airKeyParent.localScale = baseAirScale;
            }

            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public void LightUpLane(int lane)
        {
            if (lane < 0 || lane >= laneKeyTexts.Count) return;

            laneColorHandles[lane].TryCancel();
            laneScaleHandles[lane].TryCancel();
            laneKeyTexts[lane].color = WithAlpha(baseLaneColors[lane], ActiveAlpha);
            laneKeyTexts[lane].transform.localScale = baseLaneScales[lane] * ActiveScaleMultiplier;
        }

        public void LightUpAir()
        {
            airTextHandle.TryCancel();
            airRendererHandle.TryCancel();
            airScaleHandle.TryCancel();
            airKeyText.color = WithAlpha(baseAirTextColor, ActiveAlpha);
            airKeyRenderer.color = WithAlpha(baseAirRendererColor, ActiveAlpha);
            airKeyParent.localScale = baseAirScale * ActiveScaleMultiplier;
        }

        public void LightDownLane(int lane)
        {
            if (lane < 0 || lane >= laneKeyTexts.Count) return;

            laneColorHandles[lane].TryCancel();
            laneScaleHandles[lane].TryCancel();
            laneColorHandles[lane] = FadeToInactive(laneKeyTexts[lane], baseLaneColors[lane]);
            laneScaleHandles[lane] = FadeToBaseScale(laneKeyTexts[lane].transform, baseLaneScales[lane]);
        }

        public void LightDownAir()
        {
            airTextHandle.TryCancel();
            airRendererHandle.TryCancel();
            airScaleHandle.TryCancel();
            airTextHandle = FadeToInactive(airKeyText, baseAirTextColor);
            airRendererHandle = FadeToInactive(airKeyRenderer, baseAirRendererColor);
            airScaleHandle = FadeToBaseScale(airKeyParent, baseAirScale);
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

        MotionHandle FadeToBaseScale(Transform target, Vector3 baseScale)
        {
            return LMotion.Create(target.localScale, baseScale, FadeDuration)
                .Bind(value => target.localScale = value)
                .AddTo(this);
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
