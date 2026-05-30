using R3;
using UnityEngine;

namespace MyProject.Core
{
    public class PlayerSettingsCore
    {
        const float MinScrollSpeed = 1f;
        const float MaxScrollSpeed = 20f;

        const float MinSecOffset = -1f;
        const float MaxSecOffset = 1f;

        public ReadOnlyReactiveProperty<float> ScrollSpeed => scrollSpeed;
        readonly ReactiveProperty<float> scrollSpeed = new(10f);

        public ReadOnlyReactiveProperty<float> ScrollSpeedNormalized => scrollSpeedNormalized;
        readonly ReactiveProperty<float> scrollSpeedNormalized = new(Mathf.InverseLerp(MinScrollSpeed, MaxScrollSpeed, 10f));

        public ReadOnlyReactiveProperty<float> SecOffset => secOffset;
        readonly ReactiveProperty<float> secOffset = new(-0.05f);

        public void SetScrollSpeed(float newScrollSpeed)
        {
            var clamped = Mathf.Clamp(newScrollSpeed, MinScrollSpeed, MaxScrollSpeed);
            scrollSpeed.Value = clamped;
            scrollSpeedNormalized.Value = NormalizeScrollSpeed(clamped);
        }

        public void SetScrollSpeedNormalized(float normalizedScrollSpeed)
        {
            var normalized = Mathf.Clamp01(normalizedScrollSpeed);
            SetScrollSpeed(DenormalizeScrollSpeed(normalized));
        }

        public void SetSecOffset(float newSecOffset)
        {
            secOffset.Value = Mathf.Clamp(newSecOffset, MinSecOffset, MaxSecOffset);
        }

        float NormalizeScrollSpeed(float scrollSpeedValue)
        {
            return Mathf.InverseLerp(MinScrollSpeed, MaxScrollSpeed, scrollSpeedValue);
        }

        float DenormalizeScrollSpeed(float normalizedScrollSpeed)
        {
            return Mathf.Lerp(MinScrollSpeed, MaxScrollSpeed, normalizedScrollSpeed);
        }
    }
}
