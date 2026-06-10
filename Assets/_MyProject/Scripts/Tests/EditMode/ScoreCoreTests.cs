using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        public void HoldTick_PressReleaseでは直接判定されない()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var scoreCore = CreateScoreCore(hold, holdTick);

            scoreCore.JudgePressLane(0, 1f);
            scoreCore.JudgeReleaseLane(0, 1.1f);

            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));
            Assert.That(holdTick.Judge.CurrentValue, Is.EqualTo(JudgeType.None));
        }

        [Test]
        public void HoldTick_判定ライン通過後に押下中かつ親HoldHoldingならPerfectCriticalLateになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var scoreCore = CreateScoreCore(hold, holdTick);

            scoreCore.JudgePressLane(0, 1f);
            scoreCore.Update(1.55f);

            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(holdTick.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void HoldTick_親HoldがReleasedなら押下中でも判定されない()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var scoreCore = CreateScoreCore(hold, holdTick);

            scoreCore.JudgePressLane(0, 1f);
            hold.JudgeRelease(1.1f);
            scoreCore.Update(1.55f);

            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));
            Assert.That(holdTick.Judge.CurrentValue, Is.EqualTo(JudgeType.None));
        }

        [Test]
        public void HoldTick_親HoldがHoldingへ戻ればMiss前なら判定される()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var scoreCore = CreateScoreCore(hold, holdTick);

            scoreCore.JudgePressLane(0, 1f);
            hold.JudgeRelease(1.1f);
            scoreCore.Update(1.55f);
            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));

            hold.JudgePress(1.56f);
            scoreCore.Update(1.57f);

            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(holdTick.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void HoldTick_Miss時刻到達でMissLateになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var scoreCore = CreateScoreCore(hold, holdTick);

            scoreCore.JudgePressLane(0, 1f);
            scoreCore.Update(1.6f);

            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(holdTick.Judge.CurrentValue, Is.EqualTo(JudgeType.MissLate));
        }

        [Test]
        public void HoldTick_幅付きは被覆レーンのどれか押下で判定される()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f, width: 2);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var scoreCore = CreateScoreCore(hold, holdTick);

            scoreCore.JudgePressLane(1, 1f);
            scoreCore.Update(1.55f);

            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(holdTick.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void HoldTickが先頭でもJudgePressLaneは後続ノーツを正しく判定できる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 1.55f);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var tap = CreateTap(lane: 0, beat: 1.56f);
            var scoreCore = CreateScoreCore(hold, holdTick, tap);

            scoreCore.JudgePressLane(0, 1f);
            scoreCore.JudgeReleaseLane(0, 1.1f);
            scoreCore.Update(1.55f);
            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.BeforeJudge));

            scoreCore.JudgePressLane(0, 1.56f);

            Assert.That(tap.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tap.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
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
        public void AutoPlay_Tapは未入力でもPerfectCriticalになる()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(tap);

            scoreCore.Update(1.05f, true);

            Assert.That(tap.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(tap.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void AutoPlay_HoldTickは親Holdを押下扱いでPerfectCriticalになる()
        {
            var hold = CreateHold(lane: 0, beginBeat: 1f, endBeat: 2f);
            var holdTick = CreateHoldTick(hold, beat: 1.5f);
            var scoreCore = CreateScoreCore(hold, holdTick);

            scoreCore.Update(1.55f, true);

            Assert.That(hold.State.CurrentValue, Is.EqualTo(NoteState.Holding));
            Assert.That(hold.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
            Assert.That(holdTick.State.CurrentValue, Is.EqualTo(NoteState.AfterJudge));
            Assert.That(holdTick.Judge.CurrentValue, Is.EqualTo(JudgeType.PerfectCriticalLate));
        }

        [Test]
        public void 初期状態でないノーツをInitializeすると例外になる()
        {
            var tap = CreateTap(lane: 0, beat: 1f);
            tap.JudgePress(1f);
            var scoreCore = new ScoreCore(new FakeSaveDataRepository(), new FakeRankingRegisterer());

            Assert.Throws<System.InvalidOperationException>(() => scoreCore.InitializeAsync(new List<NoteCoreBase> { tap }, BeatmapType.Normal, CancellationToken.None).GetAwaiter().GetResult());
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

        [Test]
        public void SaveHighScoreAsync_NormalならNormalHighScoreだけ更新する()
        {
            var repository = new FakeSaveDataRepository
            {
                LoadedScoreData = new ScoreSaveDataCore(10, 20),
            };
            var tap = CreateTap(lane: 0, beat: 1f);
            var scoreCore = CreateScoreCore(repository, tap);
            scoreCore.JudgePressLane(0, 1.05f);

            scoreCore.SaveHighScoreAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(repository.SavedScoreData.NormalHighScore, Is.EqualTo(scoreCore.Score.CurrentValue));
            Assert.That(repository.SavedScoreData.HardHighScore, Is.EqualTo(20));
            Assert.That(scoreCore.HighScore, Is.EqualTo(10));
        }

        [Test]
        public void SaveHighScoreAsync_ランキングに現在スコアを登録する()
        {
            var rankingRegisterer = new FakeRankingRegisterer();
            var scoreCore = CreateScoreCore(new FakeSaveDataRepository(), rankingRegisterer, BeatmapType.Normal, CreateTap(lane: 0, beat: 1f));
            scoreCore.JudgePressLane(0, 1.05f);

            scoreCore.SaveHighScoreAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(rankingRegisterer.RegisteredResult.BeatmapType, Is.EqualTo(BeatmapType.Normal));
            Assert.That(rankingRegisterer.RegisteredResult.Score, Is.EqualTo(scoreCore.Score.CurrentValue));
        }

        [Test]
        public void SaveHighScoreAsync_既存ハイスコア以下なら更新しない()
        {
            var repository = new FakeSaveDataRepository
            {
                LoadedScoreData = new ScoreSaveDataCore(10, 20),
            };
            var scoreCore = CreateScoreCore(repository, BeatmapType.Hard, CreateTap(lane: 0, beat: 1f));

            scoreCore.SaveHighScoreAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(repository.SavedScoreData.NormalHighScore, Is.EqualTo(10));
            Assert.That(repository.SavedScoreData.HardHighScore, Is.EqualTo(20));
            Assert.That(scoreCore.HighScore, Is.EqualTo(20));
        }

        [Test]
        public void InitializeAsync_現在の難易度のハイスコアを保持する()
        {
            var repository = new FakeSaveDataRepository
            {
                LoadedScoreData = new ScoreSaveDataCore(10, 20),
            };

            var scoreCore = CreateScoreCore(repository, BeatmapType.Hard, CreateTap(lane: 0, beat: 1f));

            Assert.That(scoreCore.HighScore, Is.EqualTo(20));
        }

        static ScoreCore CreateScoreCore(params NoteCoreBase[] notes)
        {
            return CreateScoreCore(new FakeSaveDataRepository(), BeatmapType.Normal, notes);
        }

        static ScoreCore CreateScoreCore(FakeSaveDataRepository repository, params NoteCoreBase[] notes)
        {
            return CreateScoreCore(repository, BeatmapType.Normal, notes);
        }

        static ScoreCore CreateScoreCore(FakeSaveDataRepository repository, BeatmapType beatmapType, params NoteCoreBase[] notes)
        {
            return CreateScoreCore(repository, new FakeRankingRegisterer(), beatmapType, notes);
        }

        static ScoreCore CreateScoreCore(FakeSaveDataRepository repository, FakeRankingRegisterer rankingRegisterer, BeatmapType beatmapType, params NoteCoreBase[] notes)
        {
            var scoreCore = new ScoreCore(repository, rankingRegisterer);
            scoreCore.InitializeAsync(notes, beatmapType, CancellationToken.None).GetAwaiter().GetResult();
            return scoreCore;
        }

        static TapCore CreateTap(int lane, float beat, int width = 1)
        {
            var timing = CreateTiming(beat);
            var property = new NoteProperty(NoteType.Tap, OtogeType.Tetra, 0, timing, timing, 0f, 0f, lane, width, 0);
            return new TapCore(property);
        }

        static HoldCore CreateHold(int lane, float beginBeat, float endBeat, int width = 1)
        {
            var timingBegin = CreateTiming(beginBeat);
            var timingEnd = CreateTiming(endBeat);
            var property = new NoteProperty(NoteType.Hold, OtogeType.Tetra, 0, timingBegin, timingEnd, 0f, 0f, lane, width, 0);
            return new HoldCore(property);
        }

        static AirCore CreateAir(float beat)
        {
            var timing = CreateTiming(beat);
            var property = new NoteProperty(NoteType.Air, OtogeType.Tetra, 0, timing, timing, 0f, 0f, 0, 1, 0);
            return new AirCore(property);
        }

        static HoldTickCore CreateHoldTick(HoldCore parentHold, float beat)
        {
            var timing = CreateTiming(beat);
            var parentProperty = parentHold.Property;
            var property = new NoteProperty(NoteType.HoldTick, parentProperty.OtogeType, parentProperty.Timeline, timing, timing, 0f, 0f, parentProperty.Lane, parentProperty.Width, 0);
            return new HoldTickCore(property, parentHold);
        }

        static NoteTiming CreateTiming(float beat)
        {
            return new NoteTiming(beat, BpmChanges, TimelineToHighSpeedChanges, MeasureLengthChanges);
        }

        class FakeSaveDataRepository : ISaveDataRepository
        {
            public ScoreSaveDataCore LoadedScoreData { get; set; }
            public ScoreSaveDataCore SavedScoreData { get; private set; }

            public UniTask SavePlayerSettingsAsync(PlayerSettingsSaveDataCore saveData, CancellationToken ct)
            {
                return UniTask.CompletedTask;
            }

            public UniTask<PlayerSettingsSaveDataCore> LoadPlayerSettingsAsync(CancellationToken ct)
            {
                return UniTask.FromResult<PlayerSettingsSaveDataCore>(null);
            }

            public UniTask SaveScoreAsync(ScoreSaveDataCore saveData, CancellationToken ct)
            {
                SavedScoreData = saveData;
                return UniTask.CompletedTask;
            }

            public UniTask<ScoreSaveDataCore> LoadScoreAsync(CancellationToken ct)
            {
                return UniTask.FromResult(LoadedScoreData);
            }
        }

        class FakeRankingRegisterer : IRankingRegisterer
        {
            public ResultCore RegisteredResult { get; private set; }

            public UniTask RegisterAsync(ResultCore result, CancellationToken ct)
            {
                RegisteredResult = result;
                return UniTask.CompletedTask;
            }
        }
    }
}
