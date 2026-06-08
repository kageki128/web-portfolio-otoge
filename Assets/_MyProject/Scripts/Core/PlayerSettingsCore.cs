using R3;
using UnityEngine;

namespace MyProject.Core
{
    public class PlayerSettingsCore
    {
        const float MinScrollSpeed = 1f;
        const float MaxScrollSpeed = 20f;
        const float ScrollSpeedStep = 0.1f;
        const float DefaultScrollSpeed = 10f;

        const float MinNoteOffset = -0.2f;
        const float MaxNoteOffset = 0.2f;
        const float NoteOffsetStep = 0.001f;
        const float DefaultNoteOffset = -0.045f;

        public ReadOnlyReactiveProperty<float> ScrollSpeed => scrollSpeed;
        readonly ReactiveProperty<float> scrollSpeed = new(DefaultScrollSpeed);

        public ReadOnlyReactiveProperty<float> ScrollSpeedNormalized => scrollSpeedNormalized;
        readonly ReactiveProperty<float> scrollSpeedNormalized = new(Mathf.InverseLerp(MinScrollSpeed, MaxScrollSpeed, DefaultScrollSpeed));

        public ReadOnlyReactiveProperty<float> NoteOffset => noteOffset;
        readonly ReactiveProperty<float> noteOffset = new(DefaultNoteOffset);

        public ReadOnlyReactiveProperty<float> NoteOffsetNormalized => noteOffsetNormalized;
        readonly ReactiveProperty<float> noteOffsetNormalized = new(Mathf.InverseLerp(MinNoteOffset, MaxNoteOffset, DefaultNoteOffset));

        public void SetScrollSpeed(float newScrollSpeed)
        {
            var clamped = Mathf.Clamp(newScrollSpeed, MinScrollSpeed, MaxScrollSpeed);
            var stepped = Mathf.Round(clamped / ScrollSpeedStep) * ScrollSpeedStep;
            scrollSpeed.Value = stepped;
            scrollSpeedNormalized.Value = NormalizeScrollSpeed(stepped);
        }

        public void SetScrollSpeedNormalized(float normalizedScrollSpeed)
        {
            var normalized = Mathf.Clamp01(normalizedScrollSpeed);
            SetScrollSpeed(DenormalizeScrollSpeed(normalized));
        }

        public void ResetScrollSpeed()
        {
            SetScrollSpeed(DefaultScrollSpeed);
        }

        public void SetNoteOffset(float newNoteOffset)
        {
            var clamped = Mathf.Clamp(newNoteOffset, MinNoteOffset, MaxNoteOffset);
            var stepped = Mathf.Round(clamped / NoteOffsetStep) * NoteOffsetStep;
            noteOffset.Value = stepped;
            noteOffsetNormalized.Value = NormalizeNoteOffset(stepped);
        }

        public void SetNoteOffsetNormalized(float normalizedNoteOffset)
        {
            var normalized = Mathf.Clamp01(normalizedNoteOffset);
            SetNoteOffset(DenormalizeNoteOffset(normalized));
        }

        public void ResetNoteOffset()
        {
            SetNoteOffset(DefaultNoteOffset);
        }

        float NormalizeScrollSpeed(float scrollSpeedValue)
        {
            return Mathf.InverseLerp(MinScrollSpeed, MaxScrollSpeed, scrollSpeedValue);
        }

        float DenormalizeScrollSpeed(float normalizedScrollSpeed)
        {
            return Mathf.Lerp(MinScrollSpeed, MaxScrollSpeed, normalizedScrollSpeed);
        }

        float NormalizeNoteOffset(float noteOffsetValue)
        {
            return Mathf.InverseLerp(MinNoteOffset, MaxNoteOffset, noteOffsetValue);
        }

        float DenormalizeNoteOffset(float normalizedNoteOffset)
        {
            return Mathf.Lerp(MinNoteOffset, MaxNoteOffset, normalizedNoteOffset);
        }
    }
}
