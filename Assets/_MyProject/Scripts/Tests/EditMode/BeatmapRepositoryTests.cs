using System.Linq;
using System.Reflection;
using System.Threading;
using MyProject.Core;
using MyProject.Infrastructure;
using MyProject.Shared;
using NUnit.Framework;
using UnityEngine;

namespace MyProject.Tests.EditMode
{
    public class BeatmapRepositoryTests
    {
        const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void GetAsync_正常UGCをBeatmapへ変換できる()
        {
            var ugc = string.Join("\n",
                "@SONGID test-song-id",
                "@TITLE Test Song",
                "@ARTIST Test Artist",
                "@DESIGN Tester",
                "@DIFF 0",
                "@BGMOFS 0.250",
                "@TICKS 480",
                "@BEAT 0 4 4",
                "@BPM 0'0 120",
                "@TIL 0 0'0 1.0",
                "@MAINTIL 0",
                "@ENDHEAD",
                "#0'0:t02",
                "#1'0:h24",
                "#480>s"
            );

            using var fixture = CreateFixture(ugc);
            var beatmap = fixture.Repository.GetAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(beatmap.MetaData.Id, Is.EqualTo("test-song-id"));
            Assert.That(beatmap.MetaData.Title, Is.EqualTo("Test Song"));
            Assert.That(beatmap.MetaData.Difficulty, Is.EqualTo(DifficultyType.Normal));
            Assert.That(beatmap.NoteCores.Count, Is.EqualTo(2));
            Assert.That(beatmap.NoteCores[0], Is.TypeOf<TapCore>());
            Assert.That(beatmap.NoteCores[1], Is.TypeOf<HoldCore>());
            Assert.That(beatmap.NoteCores[0].Property.Type, Is.EqualTo(NoteType.Tap));
            Assert.That(beatmap.NoteCores[1].Property.Type, Is.EqualTo(NoteType.Hold));
            Assert.That(beatmap.NoteCores[1].Property.TimingEnd.Beat, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(beatmap.Messages.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAsync_未対応ノーツ種別はスキップしてMessageを返す()
        {
            var ugc = string.Join("\n",
                "@TITLE Test Song",
                "@ARTIST Test Artist",
                "@DESIGN Tester",
                "@DIFF 0",
                "@BGMOFS 0",
                "@TICKS 480",
                "@BEAT 0 4 4",
                "@BPM 0'0 120",
                "@TIL 0 0'0 1.0",
                "@MAINTIL 0",
                "@ENDHEAD",
                "#0'0:x02"
            );

            using var fixture = CreateFixture(ugc);
            var beatmap = fixture.Repository.GetAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(beatmap.NoteCores.Count, Is.EqualTo(0));
            Assert.That(beatmap.Messages.Any(message => message.Type == MessageType.Error && message.Content.Contains("未対応のノーツ種別")), Is.True);
        }

        [Test]
        public void GetAsync_lane不正ノーツはスキップしてMessageを返す()
        {
            var ugc = string.Join("\n",
                "@TITLE Test Song",
                "@ARTIST Test Artist",
                "@DESIGN Tester",
                "@DIFF 0",
                "@BGMOFS 0",
                "@TICKS 480",
                "@BEAT 0 4 4",
                "@BPM 0'0 120",
                "@TIL 0 0'0 1.0",
                "@MAINTIL 0",
                "@ENDHEAD",
                "#0'0:t12"
            );

            using var fixture = CreateFixture(ugc);
            var beatmap = fixture.Repository.GetAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(beatmap.NoteCores.Count, Is.EqualTo(0));
            Assert.That(beatmap.Messages.Any(message => message.Type == MessageType.Error && message.Content.Contains("未対応のレーン")), Is.True);
        }

        [Test]
        public void GetAsync_TICKS欠落時はFatalMessage付き空譜面を返す()
        {
            var ugc = string.Join("\n",
                "@TITLE Test Song",
                "@ARTIST Test Artist",
                "@DESIGN Tester",
                "@DIFF 0",
                "@BGMOFS 0",
                "@BEAT 0 4 4",
                "@BPM 0'0 120",
                "@TIL 0 0'0 1.0",
                "@MAINTIL 0",
                "@ENDHEAD",
                "#0'0:t02"
            );

            using var fixture = CreateFixture(ugc);
            var beatmap = fixture.Repository.GetAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(beatmap.NoteCores.Count, Is.EqualTo(0));
            Assert.That(beatmap.Messages.Any(message => message.Type == MessageType.Fatal), Is.True);
        }

        static TestFixture CreateFixture(string ugc)
        {
            var beatmapFiles = ScriptableObject.CreateInstance<BeatmapFilesSO>();
            var wave = AudioClip.Create("test", 44100, 1, 44100, false);
            var text = new TextAsset(ugc);

            SetBackingField(beatmapFiles, "<Wave>k__BackingField", wave);
            SetBackingField(beatmapFiles, "<Beatmap>k__BackingField", text);

            var repository = new BeatmapRepository(beatmapFiles);
            return new TestFixture(beatmapFiles, wave, text, repository);
        }

        static void SetBackingField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, InstanceNonPublic);
            Assert.That(field, Is.Not.Null, $"{fieldName} の設定に失敗しました。");
            field!.SetValue(target, value);
        }

        sealed class TestFixture : System.IDisposable
        {
            public BeatmapFilesSO BeatmapFiles { get; }
            public AudioClip Wave { get; }
            public TextAsset Text { get; }
            public BeatmapRepository Repository { get; }

            public TestFixture(BeatmapFilesSO beatmapFiles, AudioClip wave, TextAsset text, BeatmapRepository repository)
            {
                BeatmapFiles = beatmapFiles;
                Wave = wave;
                Text = text;
                Repository = repository;
            }

            public void Dispose()
            {
                Object.DestroyImmediate(Text);
                Object.DestroyImmediate(Wave);
                Object.DestroyImmediate(BeatmapFiles);
            }
        }
    }
}
