using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.Core
{
    public class PlayerSettingsCore
    {
        static readonly BeatmapType[] selectableBeatmapTypes =
        {
            BeatmapType.Normal,
            BeatmapType.Hard,
        };

        const float MinScrollSpeed = 1f;
        const float MaxScrollSpeed = 20f;
        const float ScrollSpeedStep = 0.1f;
        const float DefaultScrollSpeed = 6f;
        const BeatmapType DefaultBeatmapType = BeatmapType.Normal;

        const float MinNoteOffset = -0.2f;
        const float MaxNoteOffset = 0.2f;
        const float NoteOffsetStep = 0.001f;
        const float DefaultNoteOffset = 0f;

        readonly ISaveDataRepository saveDataRepository;

        public ReadOnlyReactiveProperty<float> ScrollSpeed => scrollSpeed;
        readonly ReactiveProperty<float> scrollSpeed = new(DefaultScrollSpeed);

        public ReadOnlyReactiveProperty<float> ScrollSpeedNormalized => scrollSpeedNormalized;
        readonly ReactiveProperty<float> scrollSpeedNormalized = new(Mathf.InverseLerp(MinScrollSpeed, MaxScrollSpeed, DefaultScrollSpeed));

        public ReadOnlyReactiveProperty<float> NoteOffset => noteOffset;
        readonly ReactiveProperty<float> noteOffset = new(DefaultNoteOffset);

        public ReadOnlyReactiveProperty<float> NoteOffsetNormalized => noteOffsetNormalized;
        readonly ReactiveProperty<float> noteOffsetNormalized = new(Mathf.InverseLerp(MinNoteOffset, MaxNoteOffset, DefaultNoteOffset));

        public ReadOnlyReactiveProperty<BeatmapType> SelectedBeatmapType => selectedBeatmapType;
        readonly ReactiveProperty<BeatmapType> selectedBeatmapType = new(DefaultBeatmapType);

        public PlayerSettingsCore(ISaveDataRepository saveDataRepository)
        {
            this.saveDataRepository = saveDataRepository;
        }

        public async UniTask LoadSavedSettingsAsync(CancellationToken ct)
        {
            var saveData = await saveDataRepository.LoadPlayerSettingsAsync(ct);
            if (saveData == null)
            {
                return;
            }

            SetScrollSpeed(saveData.ScrollSpeed);
            SetNoteOffset(saveData.NoteOffset);
        }

        public UniTask SaveCurrentSettingsAsync(CancellationToken ct)
        {
            var saveData = new PlayerSettingsSaveDataCore(scrollSpeed.CurrentValue, noteOffset.CurrentValue);
            return saveDataRepository.SavePlayerSettingsAsync(saveData, ct);
        }

        public void SetBeatmapType(BeatmapType newBeatmapType)
        {
            if (Array.IndexOf(selectableBeatmapTypes, newBeatmapType) < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newBeatmapType), newBeatmapType, "This beatmap type is not selectable.");
            }

            selectedBeatmapType.Value = newBeatmapType;
        }

        public void ChangeBeatmapType(int direction)
        {
            var step = Math.Sign(direction);
            if (step == 0)
            {
                return;
            }

            var currentIndex = Array.IndexOf(selectableBeatmapTypes, selectedBeatmapType.Value);
            var nextIndex = Mathf.Clamp(currentIndex + step, 0, selectableBeatmapTypes.Length - 1);
            SetBeatmapType(selectableBeatmapTypes[nextIndex]);
        }

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
