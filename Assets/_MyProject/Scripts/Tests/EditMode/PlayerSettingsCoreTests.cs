using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using NUnit.Framework;

namespace MyProject.Tests.EditMode
{
    public class PlayerSettingsCoreTests
    {
        [Test]
        public void SelectedBeatmapType_初期値はNormal()
        {
            var playerSettingsCore = CreatePlayerSettingsCore();

            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Normal));
        }

        [Test]
        public void ChangeBeatmapType_正方向でHard_負方向でNormalへ変わる()
        {
            var playerSettingsCore = CreatePlayerSettingsCore();

            playerSettingsCore.ChangeBeatmapType(1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Hard));

            playerSettingsCore.ChangeBeatmapType(-1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Normal));
        }

        [Test]
        public void ChangeBeatmapType_上下端で止まる()
        {
            var playerSettingsCore = CreatePlayerSettingsCore();

            playerSettingsCore.ChangeBeatmapType(-1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Normal));

            playerSettingsCore.ChangeBeatmapType(1);
            playerSettingsCore.ChangeBeatmapType(1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Hard));
        }

        [Test]
        public void SetBeatmapType_Demoは選べない()
        {
            var playerSettingsCore = CreatePlayerSettingsCore();

            Assert.Throws<ArgumentOutOfRangeException>(() => playerSettingsCore.SetBeatmapType(BeatmapType.Demo));
        }

        [Test]
        public void LoadSavedSettingsAsync_セーブデータが無い場合はデフォルト値のまま()
        {
            var playerSettingsCore = CreatePlayerSettingsCore();

            playerSettingsCore.LoadSavedSettingsAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(playerSettingsCore.ScrollSpeed.CurrentValue, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(playerSettingsCore.NoteOffset.CurrentValue, Is.EqualTo(-0.045f).Within(0.0001f));
        }

        [Test]
        public void LoadSavedSettingsAsync_セーブデータがある場合は値を反映する()
        {
            var repository = new FakeSaveDataRepository
            {
                LoadResult = new PlayerSettingsSaveDataCore(12.34f, 0.1234f),
            };
            var playerSettingsCore = CreatePlayerSettingsCore(repository);

            playerSettingsCore.LoadSavedSettingsAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(playerSettingsCore.ScrollSpeed.CurrentValue, Is.EqualTo(12.3f).Within(0.0001f));
            Assert.That(playerSettingsCore.NoteOffset.CurrentValue, Is.EqualTo(0.123f).Within(0.0001f));
        }

        [Test]
        public void SaveCurrentSettingsAsync_現在のScrollSpeedとNoteOffsetを保存する()
        {
            var repository = new FakeSaveDataRepository();
            var playerSettingsCore = CreatePlayerSettingsCore(repository);
            playerSettingsCore.SetScrollSpeed(7.89f);
            playerSettingsCore.SetNoteOffset(0.01234f);

            playerSettingsCore.SaveCurrentSettingsAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(repository.SavedData.ScrollSpeed, Is.EqualTo(7.9f).Within(0.0001f));
            Assert.That(repository.SavedData.NoteOffset, Is.EqualTo(0.012f).Within(0.0001f));
        }

        PlayerSettingsCore CreatePlayerSettingsCore(FakeSaveDataRepository repository = null)
        {
            return new PlayerSettingsCore(repository ?? new FakeSaveDataRepository());
        }

        class FakeSaveDataRepository : ISaveDataRepository
        {
            public PlayerSettingsSaveDataCore LoadResult { get; set; }
            public PlayerSettingsSaveDataCore SavedData { get; private set; }

            public UniTask SavePlayerSettingsAsync(PlayerSettingsSaveDataCore saveData, CancellationToken ct)
            {
                SavedData = saveData;
                return UniTask.CompletedTask;
            }

            public UniTask<PlayerSettingsSaveDataCore> LoadPlayerSettingsAsync(CancellationToken ct)
            {
                return UniTask.FromResult(LoadResult);
            }
        }
    }
}
