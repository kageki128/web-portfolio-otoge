using System.Collections.Generic;
using MyProject.Core;
using NUnit.Framework;

namespace MyProject.Tests.EditMode
{
    public class ScoreCoreTests
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

        [Test]
        public void Update_判定中にAfterJudgeノーツが出ても例外を投げない()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tap);

            Assert.DoesNotThrow(() => scoreCore.Update(1.2f));
            Assert.That(tap.State, Is.EqualTo(NoteState.AfterJudge));
        }

        [Test]
        public void JudgePress_JudgeRelease_空レーンでも例外を投げない()
        {
            var tap = CreateTap(lane: 0, beat: 0f);
            var scoreCore = CreateScoreCore(tap);

            scoreCore.JudgePress(0, 0f);

            Assert.DoesNotThrow(() => scoreCore.JudgePress(0, 0f));
            Assert.DoesNotThrow(() => scoreCore.JudgeRelease(0, 0f));
        }

        [Test]
        public void Tap_早Miss押下は無視され_後続の有効押下で判定確定する()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tap);

            scoreCore.JudgePress(0, 0.8f);
            Assert.That(tap.State, Is.EqualTo(NoteState.BeforeJudge));
            Assert.That(tap.Judge, Is.EqualTo(JudgeType.None));

            scoreCore.JudgePress(0, 1.02f);
            Assert.That(tap.State, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tap.Judge, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Tap_未押下ならBeginMissでMissLate確定する()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tap);

            scoreCore.Update(1.2f);

            Assert.That(tap.State, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tap.Judge, Is.EqualTo(JudgeType.MissLate));
        }

        [Test]
        public void Hold_始点押下_離し_再押下でHoldingReleasedHoldingと遷移する()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePress(0, 1f);
            Assert.That(hold.State, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge, Is.EqualTo(JudgeType.PerfectCriticalLate));

            scoreCore.JudgeRelease(0, 1.5f);
            Assert.That(hold.State, Is.EqualTo(NoteState.Released));

            scoreCore.JudgePress(0, 1.6f);
            Assert.That(hold.State, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Hold_未押下でBeginMiss時刻を過ぎるとMissedになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.Update(1.2f);

            Assert.That(hold.State, Is.EqualTo(NoteState.Missed));
            Assert.That(hold.Judge, Is.EqualTo(JudgeType.MissLate));
        }

        [Test]
        public void Hold_Missed後でも押し直すとHoldingに戻る()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.Update(1.2f);
            Assert.That(hold.State, Is.EqualTo(NoteState.Missed));

            scoreCore.JudgePress(0, 1.3f);

            Assert.That(hold.State, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge, Is.EqualTo(JudgeType.MissLate));
        }

        [Test]
        public void Hold始点判定後_BeginMiss時刻を過ぎてもMissedへ上書きされない()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePress(0, 1f);
            Assert.That(hold.State, Is.EqualTo(NoteState.Holding));
            var judgeAfterPress = hold.Judge;

            Assert.DoesNotThrow(() => scoreCore.Update(1.2f));
            Assert.That(hold.State, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge, Is.EqualTo(judgeAfterPress));
        }

        [Test]
        public void Hold_Holdingで終点を過ぎるとAfterJudgeになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePress(0, 1f);
            scoreCore.Update(2f);

            Assert.That(hold.State, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(hold.Judge, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Hold_Releasedで終点を過ぎるとAfterJudgeになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePress(0, 1f);
            scoreCore.JudgeRelease(0, 1.5f);
            scoreCore.Update(2f);

            Assert.That(hold.State, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(hold.Judge, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Hold_Missedのまま終点Miss時刻を過ぎるとAfterJudgeになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.Update(1.2f);
            var judgeAfterBeginMiss = hold.Judge;

            Assert.DoesNotThrow(() => scoreCore.Update(2.2f));
            Assert.That(hold.State, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(hold.Judge, Is.EqualTo(judgeAfterBeginMiss));
        }

        [Test]
        public void 同一レーン複数ノーツはBeginBeat順に判定される()
        {
            var tapBeat2 = CreateTap(lane: 0, beat: 2f);
            var tapBeat1 = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tapBeat2, tapBeat1);

            scoreCore.JudgePress(0, 1f);
            Assert.That(tapBeat1.State, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tapBeat2.State, Is.EqualTo(NoteState.BeforeJudge));

            scoreCore.JudgePress(0, 2f);
            Assert.That(tapBeat2.State, Is.EqualTo(NoteState.AfterJudge));
        }

        [Test]
        public void レーンごとに独立して判定される()
        {
            var lane0Tap = CreateTap(lane: 0, beat: 1f);
            var lane1Tap = CreateTap(lane: 1, beat: 1f);
            var scoreCore = CreateScoreCore(lane0Tap, lane1Tap);

            scoreCore.JudgePress(1, 1f);

            Assert.That(lane1Tap.State, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(lane0Tap.State, Is.EqualTo(NoteState.BeforeJudge));
        }

        [Test]
        public void 初期状態でないノーツをInitializeすると例外になる()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            tap.JudgePress(1f);
            var scoreCore = new ScoreCore();

            Assert.Throws<System.InvalidOperationException>(() => scoreCore.Initialize(new List<NoteCoreBase> { tap }));
        }

        [Test]
        public void ノーツ0件でInitializeすると例外になる()
        {
            var scoreCore = new ScoreCore();

            Assert.Throws<System.ArgumentException>(() => scoreCore.Initialize(new List<NoteCoreBase>()));
        }

        [Test]
        public void AllPerfectなら端数補正で1000000点になる()
        {
            var lane0Tap = CreateTap(lane: 0, beat: 1f);
            var lane1Tap = CreateTap(lane: 1, beat: 1f);
            var lane2Tap = CreateTap(lane: 2, beat: 1f);
            var scoreCore = CreateScoreCore(lane0Tap, lane1Tap, lane2Tap);

            scoreCore.JudgePress(0, 1.05f);
            scoreCore.JudgePress(1, 1.05f);
            scoreCore.JudgePress(2, 1.05f);

            Assert.That(scoreCore.Score.CurrentValue, Is.EqualTo(1000000));
        }

        static ScoreCore CreateScoreCore(params NoteCoreBase[] notes)
        {
            var scoreCore = new ScoreCore();
            scoreCore.Initialize(notes);
            return scoreCore;
        }

        static TapCore CreateTap(int lane, float beat)
        {
            var timing = CreateTiming(beat);
            var property = new NoteProperty(NoteType.Tap, 0, timing, timing, 0f, 0f, lane, 1, 0);
            return new TapCore(property);
        }

        static HoldCore CreateHold(int lane, float beginBeat, float endBeat)
        {
            var timingBegin = CreateTiming(beginBeat);
            var timingEnd = CreateTiming(endBeat);
            var property = new NoteProperty(NoteType.Hold, 0, timingBegin, timingEnd, 0f, 0f, lane, 1, 0);
            return new HoldCore(property);
        }

        static NoteTiming CreateTiming(float beat)
        {
            return new NoteTiming(beat, BpmChanges, TimelineToHighSpeedChanges, MeasureLengthChanges);
        }
    }
}
