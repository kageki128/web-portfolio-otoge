using System;
using MyProject.Core;
using NUnit.Framework;

namespace MyProject.Tests.EditMode
{
    public class PlayerSettingsCoreTests
    {
        [Test]
        public void SelectedBeatmapType_初期値はNormal()
        {
            var playerSettingsCore = new PlayerSettingsCore();

            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Normal));
        }

        [Test]
        public void ChangeBeatmapType_正方向でHard_負方向でNormalへ変わる()
        {
            var playerSettingsCore = new PlayerSettingsCore();

            playerSettingsCore.ChangeBeatmapType(1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Hard));

            playerSettingsCore.ChangeBeatmapType(-1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Normal));
        }

        [Test]
        public void ChangeBeatmapType_上下端で止まる()
        {
            var playerSettingsCore = new PlayerSettingsCore();

            playerSettingsCore.ChangeBeatmapType(-1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Normal));

            playerSettingsCore.ChangeBeatmapType(1);
            playerSettingsCore.ChangeBeatmapType(1);
            Assert.That(playerSettingsCore.SelectedBeatmapType.CurrentValue, Is.EqualTo(BeatmapType.Hard));
        }

        [Test]
        public void SetBeatmapType_Demoは選べない()
        {
            var playerSettingsCore = new PlayerSettingsCore();

            Assert.Throws<ArgumentOutOfRangeException>(() => playerSettingsCore.SetBeatmapType(BeatmapType.Demo));
        }
    }
}
