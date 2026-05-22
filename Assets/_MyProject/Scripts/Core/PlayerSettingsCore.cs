using R3;
using UnityEngine;

namespace MyProject.Core
{
    public class PlayerSettingsCore
    {
        public ReadOnlyReactiveProperty<float> ScrollSpeed => scrollSpeed;
        readonly ReactiveProperty<float> scrollSpeed = new(10f);

        public ReadOnlyReactiveProperty<float> SecOffset => secOffset;
        readonly ReactiveProperty<float> secOffset = new(-0.05f);

        const float MinScrollSpeed = 1f;
        const float MaxScrollSpeed = 20f;

        const float MinSecOffset = -1f;
        const float MaxSecOffset = 1f;

        public void SetScrollSpeed(float newScrollSpeed)
        {
            scrollSpeed.Value = Mathf.Clamp(newScrollSpeed, MinScrollSpeed, MaxScrollSpeed);
        }

        public void SetSecOffset(float newSecOffset)
        {
            secOffset.Value = Mathf.Clamp(newSecOffset, MinSecOffset, MaxSecOffset);
        }
    }
}
