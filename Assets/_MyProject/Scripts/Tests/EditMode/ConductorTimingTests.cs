using System.Collections.Generic;
using MyProject.Core;
using NUnit.Framework;
using R3;

namespace MyProject.Tests.EditMode
{
    public class ConductorTimingTests
    {
        static readonly IReadOnlyList<BpmChange> BpmChanges = new List<BpmChange>
        {
            new(60f, 0f),
        };

        static readonly IReadOnlyDictionary<int, IReadOnlyList<HighSpeedChange>> TimelineToHighSpeedChanges =
            new Dictionary<int, IReadOnlyList<HighSpeedChange>>
            {
                { 0, new List<HighSpeedChange> { new(1f, 0f) } },
            };

        static readonly IReadOnlyList<MeasureLengthChange> MeasureLengthChanges = new List<MeasureLengthChange>
        {
            new(4, 0f),
        };

        static readonly IReadOnlyList<OtogeChange> OtogeChanges = new List<OtogeChange>
        {
            new(0f, OtogeType.Tetra),
        };

        [Test]
        public void SetTimeBySec_イベントBeat到達時に1回発火する()
        {
            var timing = CreateTiming(new[] { 2f });
            var emitCount = 0;
            using var subscription = timing.OtogeEvent.Subscribe(_ => emitCount++);

            timing.SetTimeBySec(1f);
            timing.SetTimeBySec(2.1f);

            Assert.That(emitCount, Is.EqualTo(1));
        }

        [Test]
        public void SetTimeBySec_巻き戻し後に再到達しても再発火しない()
        {
            var timing = CreateTiming(new[] { 2f });
            var emitCount = 0;
            using var subscription = timing.OtogeEvent.Subscribe(_ => emitCount++);

            timing.SetTimeBySec(2.1f);
            timing.SetTimeBySec(1f);
            timing.SetTimeBySec(2.2f);

            Assert.That(emitCount, Is.EqualTo(1));
        }

        [Test]
        public void SetTimeBySec_OtogeTypeTransitionを更新する()
        {
            var timing = CreateTiming
            (
                new[] { 2f },
                new List<OtogeChange>
                {
                    new(0f, OtogeType.Tetra),
                    new(4f, OtogeType.Octa),
                    new(8f, OtogeType.Air),
                }
            );

            timing.SetTimeBySec(3f);
            var transitionAt3Sec = timing.CurrentOtogeTypeTransition.CurrentValue;
            Assert.That(transitionAt3Sec.CurrentType, Is.EqualTo(OtogeType.Tetra));
            Assert.That(transitionAt3Sec.NextType, Is.EqualTo(OtogeType.Octa));
            Assert.That(transitionAt3Sec.RemainingBeat, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(transitionAt3Sec.RemainingSec, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(transitionAt3Sec.DurationSec, Is.EqualTo(4f).Within(0.0001f));

            timing.SetTimeBySec(4.2f);
            var transitionAt4_2Sec = timing.CurrentOtogeTypeTransition.CurrentValue;
            Assert.That(transitionAt4_2Sec.CurrentType, Is.EqualTo(OtogeType.Octa));
            Assert.That(transitionAt4_2Sec.NextType, Is.EqualTo(OtogeType.Air));
            Assert.That(transitionAt4_2Sec.RemainingBeat, Is.EqualTo(3.8f).Within(0.0001f));
            Assert.That(transitionAt4_2Sec.RemainingSec, Is.EqualTo(3.8f).Within(0.0001f));
            Assert.That(transitionAt4_2Sec.DurationSec, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void SetTimeBySec_最終タイプ到達後は次タイプがnullで残りBeatと残り秒は0()
        {
            var timing = CreateTiming
            (
                new[] { 2f },
                new List<OtogeChange>
                {
                    new(0f, OtogeType.Tetra),
                    new(4f, OtogeType.Octa),
                    new(8f, OtogeType.Air),
                }
            );

            timing.SetTimeBySec(9f);

            var transition = timing.CurrentOtogeTypeTransition.CurrentValue;
            Assert.That(transition.CurrentType, Is.EqualTo(OtogeType.Air));
            Assert.That(transition.NextType, Is.Null);
            Assert.That(transition.RemainingBeat, Is.EqualTo(0f));
            Assert.That(transition.RemainingSec, Is.EqualTo(0f));
        }

        [Test]
        public void CalculateOtogeTypeFromBeat_切り替えBeatちょうどは新タイプを返す()
        {
            var otogeChanges = new List<OtogeChange>
            {
                new(0f, OtogeType.Tetra),
                new(4f, OtogeType.Octa),
                new(8f, OtogeType.Air),
            };

            var typeAt4Beat = TimingCalculator.CalculateOtogeTypeFromBeat(4f, otogeChanges);
            var typeAt8Beat = TimingCalculator.CalculateOtogeTypeFromBeat(8f, otogeChanges);

            Assert.That(typeAt4Beat, Is.EqualTo(OtogeType.Octa));
            Assert.That(typeAt8Beat, Is.EqualTo(OtogeType.Air));
        }

        static ConductorTiming CreateTiming(IReadOnlyList<float> otogeEventBeats, IReadOnlyList<OtogeChange> otogeChanges = null)
        {
            return new ConductorTiming(BpmChanges, TimelineToHighSpeedChanges, MeasureLengthChanges, otogeChanges ?? OtogeChanges, otogeEventBeats);
        }
    }
}
