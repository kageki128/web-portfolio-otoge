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
            Assert.That(tap.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
        }

        [Test]
        public void JudgePress_JudgeRelease_空レーンでも例外を投げない()
        {
            var tap = CreateTap(lane: 0, beat: 0f);
            var scoreCore = CreateScoreCore(tap);

            scoreCore.JudgePressLane(0, 0f);

            Assert.DoesNotThrow(() => scoreCore.JudgePressLane(0, 0f));
            Assert.DoesNotThrow(() => scoreCore.JudgeReleaseLane(0, 0f));
        }

        [Test]
        public void Tap_早Miss押下は無視され_後続の有効押下で判定確定する()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tap);

            scoreCore.JudgePressLane(0, 0.8f);
            Assert.That(tap.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));
            Assert.That(tap.Judge.CurrentValue, Is.EqualTo(JudgeType.None));

            scoreCore.JudgePressLane(0, 1.02f);
            Assert.That(tap.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tap.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Tap_未押下ならBeginMissでMissLate確定する()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tap);

            scoreCore.Update(1.2f);

            Assert.That(tap.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tap.Judge.CurrentValue, Is.EqualTo(JudgeType.MissLate));
        }

        [Test]
        public void Hold_始点押下_離し_再押下でHoldingReleasedHoldingと遷移する()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePressLane(0, 1f);
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));

            scoreCore.JudgeReleaseLane(0, 1.5f);
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Released));

            scoreCore.JudgePressLane(0, 1.6f);
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Hold_未押下でBeginMiss時刻を過ぎるとMissedになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.Update(1.2f);

            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Missed));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(JudgeType.MissLate));
        }

        [Test]
        public void Hold_Missed後でも押し直すとHoldingに戻る()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.Update(1.2f);
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Missed));

            scoreCore.JudgePressLane(0, 1.3f);

            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(JudgeType.MissLate));
        }

        [Test]
        public void Hold始点判定後_BeginMiss時刻を過ぎてもMissedへ上書きされない()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePressLane(0, 1f);
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Holding));
            var judgeAfterPress = hold.Judge.CurrentValue;

            Assert.DoesNotThrow(() => scoreCore.Update(1.2f));
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(judgeAfterPress));
        }

        [Test]
        public void Hold_Holdingで終点を過ぎるとAfterJudgeになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePressLane(0, 1f);
            scoreCore.Update(2f);

            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Hold_Releasedで終点を過ぎるとAfterJudgeになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.JudgePressLane(0, 1f);
            scoreCore.JudgeReleaseLane(0, 1.5f);
            scoreCore.Update(2f);

            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void Hold_Missedのまま終点Miss時刻を過ぎるとAfterJudgeになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var scoreCore = CreateScoreCore(hold);

            scoreCore.Update(1.2f);
            var judgeAfterBeginMiss = hold.Judge.CurrentValue;

            Assert.DoesNotThrow(() => scoreCore.Update(2.2f));
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(judgeAfterBeginMiss));
        }

        [Test]
        public void 同一レーン複数ノーツはBeginBeat順に判定される()
        {
            var tapBeat2 = CreateTap(lane: 0, beat: 2f);
            var tapBeat1 = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tapBeat2, tapBeat1);

            scoreCore.JudgePressLane(0, 1f);
            Assert.That(tapBeat1.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tapBeat2.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));

            scoreCore.JudgePressLane(0, 2f);
            Assert.That(tapBeat2.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
        }

        [Test]
        public void レーンごとに独立して判定される()
        {
            var lane0Tap = CreateTap(lane: 0, beat: 1f);
            var lane1Tap = CreateTap(lane: 1, beat: 1f);
            var scoreCore = CreateScoreCore(lane0Tap, lane1Tap);

            scoreCore.JudgePressLane(1, 1f);

            Assert.That(lane1Tap.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(lane0Tap.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));
        }

        [Test]
        public void 幅付きTapは含まれるレーンならどこからでも押下判定できる()
        {
            var wideTap = CreateTap(lane: 0, beat: 1f, width: 2);
            var scoreCore = CreateScoreCore(wideTap);

            scoreCore.JudgePressLane(1, 1f);
            Assert.That(wideTap.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(wideTap.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));

            scoreCore.JudgePressLane(0, 1f);
            Assert.That(scoreCore.Combo.CurrentValue, Is.EqualTo(1));
        }

        [Test]
        public void 幅付きHoldは別レーン押下と離しでも状態遷移できる()
        {
            var wideHold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f, width: 2);
            var scoreCore = CreateScoreCore(wideHold);

            scoreCore.JudgePressLane(1, 1f);
            Assert.That(wideHold.State.CurrentValue, Is.EqualTo(NoteState.Holding));

            scoreCore.JudgeReleaseLane(0, 1.5f);
            Assert.That(wideHold.State.CurrentValue, Is.EqualTo(NoteState.Released));
        }

        [Test]
        public void Air判定後_再押下しても再判定されない()
        {
            var air = CreateAir(beat: 1f);
            var scoreCore = CreateScoreCore(air);

            scoreCore.JudgePressAir(1f);
            Assert.That(air.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(air.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));

            Assert.DoesNotThrow(() => scoreCore.JudgePressAir(1f));
            Assert.That(scoreCore.Combo.CurrentValue, Is.EqualTo(1));
        }

        [Test]
        public void Air_同時刻ノーツは1回の押下でまとめて判定される()
        {
            var airBeat2 = CreateAir(beat: 2f);
            var airBeat1A = CreateAir(beat: 1f);
            var airBeat1B = CreateAir(beat: 1f);
            var scoreCore = CreateScoreCore(airBeat2, airBeat1A, airBeat1B);

            scoreCore.JudgePressAir(1f);

            Assert.That(airBeat1A.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(airBeat1B.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(airBeat2.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));
            Assert.That(scoreCore.Combo.CurrentValue, Is.EqualTo(2));

            scoreCore.JudgePressAir(2f);
            Assert.That(airBeat2.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
        }

        [Test]
        public void Air_未押下ならBeginMissでMissLate確定する()
        {
            var air = CreateAir(beat: 1f);
            var scoreCore = CreateScoreCore(air);

            scoreCore.Update(1.2f);

            Assert.That(air.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(air.Judge.CurrentValue, Is.EqualTo(JudgeType.MissLate));
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
        public void AllPerfectなら端数補正で1000000点になる()
        {
            var lane0Tap = CreateTap(lane: 0, beat: 1f);
            var lane1Tap = CreateTap(lane: 1, beat: 1f);
            var lane2Tap = CreateTap(lane: 2, beat: 1f);
            var scoreCore = CreateScoreCore(lane0Tap, lane1Tap, lane2Tap);

            scoreCore.JudgePressLane(0, 1.05f);
            scoreCore.JudgePressLane(1, 1.05f);
            scoreCore.JudgePressLane(2, 1.05f);

            Assert.That(scoreCore.Score.CurrentValue, Is.EqualTo(1000000));
        }

        static ScoreCore CreateScoreCore(params NoteCoreBase[] notes)
        {
            var scoreCore = new ScoreCore();
            scoreCore.Initialize(notes);
            return scoreCore;
        }

        static TapCore CreateTap(int lane, float beat, int width = 1)
        {
            var timing = CreateTiming(beat);
            var property = new NoteProperty(NoteType.Tap, 0, timing, timing, 0f, 0f, lane, width, 0);
            return new TapCore(property);
        }

        static HoldCore CreateHold(int lane, float beginBeat, float endBeat, int width = 1)
        {
            var timingBegin = CreateTiming(beginBeat);
            var timingEnd = CreateTiming(endBeat);
            var property = new NoteProperty(NoteType.Hold, 0, timingBegin, timingEnd, 0f, 0f, lane, width, 0);
            return new HoldCore(property);
        }

        static AirCore CreateAir(float beat)
        {
            var timing = CreateTiming(beat);
            var property = new NoteProperty(NoteType.Air, 0, timing, timing, 0f, 0f, 0, 1, 0);
            return new AirCore(property);
        }

        static NoteTiming CreateTiming(float beat)
        {
            return new NoteTiming(beat, BpmChanges, TimelineToHighSpeedChanges, MeasureLengthChanges);
        }
    }
}
