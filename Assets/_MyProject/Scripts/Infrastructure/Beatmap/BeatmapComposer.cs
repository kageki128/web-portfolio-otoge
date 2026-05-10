using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MyProject.Core;
using MyProject.Shared;
using UnityEngine;

namespace MyProject.Infrastructure
{
    /// <summary>
    /// Parserが作った中間データを検証しつつ、最終的なBeatmapを組み立てる。
    /// </summary>
    public class BeatmapComposer
    {
        static readonly Dictionary<char, int> laneMap = new()
        {
            { '0', 0 },
            { '2', 1 },
            { '4', 2 },
            { '6', 3 },
            { '8', 4 },
            { 'A', 5 },
            { 'C', 6 },
            { 'E', 7 },
        };

        static readonly Dictionary<char, int> widthMap = new()
        {
            { '2', 1 },
            { '4', 2 },
            { '6', 3 },
            { '8', 4 },
            { 'A', 5 },
            { 'C', 6 },
            { 'E', 7 },
            { 'G', 8 },
        };

        /// <summary>
        /// Fatal判定を先に行い、問題なければ timing/note を生成する。
        /// </summary>
        public BeatmapCore Compose(AudioClip wave, BeatmapParsedData parsedData, CancellationToken ct)
        {
            var messages = parsedData.Messages;
            // 小節長変化を「小節開始beat」に展開する。
            var measureEntries = BuildMeasureEntries(parsedData.RawMeasureLengthChanges, messages);
            var hasFatal = messages.Any(message => message.Type == MessageType.Fatal);

            // TICKSが無いと beat計算ができないため致命扱い。
            if (!parsedData.HasTicks)
            {
                messages.Add(new Message(MessageType.Fatal, "TICKS が存在しないため譜面を読み込めません。"));
                hasFatal = true;
            }

            // 小節長の基準点（0小節）がない譜面は処理不能。
            if (measureEntries.Count == 0 || measureEntries[0].Measure != 0)
            {
                messages.Add(new Message(MessageType.Fatal, "@BEAT の 0 小節定義が存在しないため譜面を読み込めません。"));
                hasFatal = true;
            }

            // BPMをbeat基準に変換し、0拍定義の有無をチェックする。
            var bpmChanges = new List<BpmChange>();
            if (!hasFatal)
            {
                foreach (var rawBpm in parsedData.RawBpmChanges)
                {
                    var beat = ToBeatFromMeasure(rawBpm.Measure, measureEntries) + (float)rawBpm.Tick / parsedData.Ticks;
                    bpmChanges.Add(new BpmChange(rawBpm.Bpm, beat));
                }
                bpmChanges = bpmChanges.OrderBy(change => change.Beat).ToList();

                if (!bpmChanges.Any(change => Mathf.Approximately(change.Beat, 0f)))
                {
                    messages.Add(new Message(MessageType.Fatal, "@BPM の 0'0 定義が存在しないため譜面を読み込めません。"));
                    hasFatal = true;
                }
            }

            // MetaDataはFatal時にも返すためこの段階で作る。
            var maxBpm = bpmChanges.Count == 0 ? 0f : bpmChanges.Max(change => change.Bpm);
            var minBpm = bpmChanges.Count == 0 ? 0f : bpmChanges.Min(change => change.Bpm);
            var metaData = new BeatmapMetaData
            (
                parsedData.Title,
                parsedData.Artist,
                parsedData.Designers,
                parsedData.Difficulty,
                wave,
                parsedData.WaveOffset,
                maxBpm,
                minBpm
            );

            // 致命エラー時は空譜面で返却する。
            if (hasFatal)
            {
                return new BeatmapCore(metaData, CreateEmptyMainData(), messages);
            }

            // 各タイムラインのハイスピ変化をbeat基準に変換する。
            var timelineToHighSpeedChanges = BuildHighSpeedChanges(parsedData.RawHighSpeedChanges, measureEntries, parsedData.Ticks);
            var measureLengthChanges = measureEntries
                .Select(entry => new MeasureLengthChange(entry.Length, entry.BeatStart))
                .ToList();

            // 中間ノーツから最終ノーツを生成する。
            var noteCores = new List<NoteCore>();
            var layer = 0;
            foreach (var rawNote in parsedData.RawNotes)
            {
                ct.ThrowIfCancellationRequested();

                if (!timelineToHighSpeedChanges.TryGetValue(rawNote.Timeline, out _))
                {
                    messages.Add(new Message(MessageType.Error, $"[{rawNote.LineNum}] タイムライン {rawNote.Timeline} の @TIL が存在しないためノーツを無視しました。"));
                    continue;
                }

                if (!laneMap.TryGetValue(char.ToUpperInvariant(rawNote.Lane), out var lane))
                {
                    messages.Add(new Message(MessageType.Error, $"[{rawNote.LineNum}] 未対応のレーンです: {rawNote.Lane}"));
                    continue;
                }

                if (!widthMap.TryGetValue(char.ToUpperInvariant(rawNote.Width), out var width))
                {
                    messages.Add(new Message(MessageType.Error, $"[{rawNote.LineNum}] 未対応の幅です: {rawNote.Width}"));
                    continue;
                }

                if (rawNote.Length < 0)
                {
                    messages.Add(new Message(MessageType.Error, $"[{rawNote.LineNum}] ノーツ長が不正です: {rawNote.Length}"));
                    continue;
                }

                // Unknown種別はUnsupportedへ落としてMessageを残す。
                var noteType = ParseNoteType(rawNote.NoteType, rawNote.LineNum, messages);
                var beatBegin = ToBeatFromMeasure(rawNote.Measure, measureEntries) + (float)rawNote.Tick / parsedData.Ticks;
                var beatEnd = beatBegin + (float)rawNote.Length / parsedData.Ticks;
                var timingBegin = new NoteTiming(beatBegin, bpmChanges, timelineToHighSpeedChanges, measureLengthChanges);
                var timingEnd = new NoteTiming(beatEnd, bpmChanges, timelineToHighSpeedChanges, measureLengthChanges);
                var scrollBegin = timingBegin.TimelineToScroll[rawNote.Timeline];
                var scrollEnd = timingEnd.TimelineToScroll[rawNote.Timeline];

                noteCores.Add(new NoteCore(new NoteProperty
                (
                    noteType,
                    rawNote.Timeline,
                    timingBegin,
                    timingEnd,
                    scrollBegin,
                    scrollEnd,
                    lane,
                    width,
                    layer
                )));

                layer++;
            }

            // 再生中の時間進行を扱うConductorを構築する。
            var conductorCore = new ConductorCore(new ConductorTiming
            (
                bpmChanges,
                timelineToHighSpeedChanges,
                measureLengthChanges
            ));

            var mainData = new BeatmapMainData(conductorCore, noteCores);
            return new BeatmapCore(metaData, mainData, messages);
        }

        /// <summary>
        /// ノーツ種別文字をCoreのNoteTypeへ変換する。
        /// </summary>
        static NoteType ParseNoteType(char noteType, int lineNum, List<Message> messages)
        {
            var lower = char.ToLowerInvariant(noteType);
            if (lower == 't')
            {
                return NoteType.Tap;
            }

            if (lower == 'h')
            {
                return NoteType.Hold;
            }

            messages.Add(new Message(MessageType.Error, $"[{lineNum}] 未対応のノーツ種別です: {noteType}"));
            return NoteType.Unsupported;
        }

        /// <summary>
        /// 小節長変化を、小節開始beat付きの時系列データへ展開する。
        /// </summary>
        static List<MeasureEntry> BuildMeasureEntries(List<RawMeasureLengthChange> rawChanges, List<Message> messages)
        {
            var latestByMeasure = new Dictionary<int, RawMeasureLengthChange>();
            foreach (var rawChange in rawChanges)
            {
                latestByMeasure[rawChange.Measure] = rawChange;
            }

            var sorted = latestByMeasure.Values.OrderBy(change => change.Measure).ToList();
            if (sorted.Count == 0)
            {
                return new List<MeasureEntry>();
            }

            if (sorted[0].Measure != 0)
            {
                return new List<MeasureEntry>();
            }

            var result = new List<MeasureEntry>(sorted.Count);
            var accumulatedBeat = 0f;
            var currentMeasure = 0;
            var currentLength = sorted[0].Length;
            result.Add(new MeasureEntry(0, currentLength, 0f));

            for (var i = 1; i < sorted.Count; i++)
            {
                var change = sorted[i];
                // 変化点まで現在の小節長でbeatを積算する。
                while (currentMeasure < change.Measure)
                {
                    accumulatedBeat += currentLength;
                    currentMeasure++;
                }

                if (change.Length <= 0)
                {
                    messages.Add(new Message(MessageType.Error, $"[{change.LineNum}] 小節長が0以下のため無視しました。"));
                    continue;
                }

                currentLength = change.Length;
                result.Add(new MeasureEntry(change.Measure, change.Length, accumulatedBeat));
            }

            return result;
        }

        /// <summary>
        /// 指定小節の先頭beatを取得する。
        /// </summary>
        static float ToBeatFromMeasure(int measure, IReadOnlyList<MeasureEntry> measureEntries)
        {
            // 直近の小節長変化点から相対計算する。
            for (var i = measureEntries.Count - 1; i >= 0; i--)
            {
                var entry = measureEntries[i];
                if (measure >= entry.Measure)
                {
                    return entry.BeatStart + (measure - entry.Measure) * entry.Length;
                }
            }

            return measure * measureEntries[0].Length;
        }

        /// <summary>
        /// ハイスピ変化をタイムライン別に集約してbeat順に並べる。
        /// </summary>
        static Dictionary<int, IReadOnlyList<HighSpeedChange>> BuildHighSpeedChanges
        (
            List<RawHighSpeedChange> rawHighSpeedChanges,
            IReadOnlyList<MeasureEntry> measureEntries,
            int ticks
        )
        {
            var timelineToChanges = new Dictionary<int, List<HighSpeedChange>>();

            foreach (var rawHighSpeedChange in rawHighSpeedChanges)
            {
                var beat = ToBeatFromMeasure(rawHighSpeedChange.Measure, measureEntries) + (float)rawHighSpeedChange.Tick / ticks;
                // タイムラインごとに配列を用意して追加する。
                if (!timelineToChanges.TryGetValue(rawHighSpeedChange.Timeline, out var changes))
                {
                    changes = new List<HighSpeedChange>();
                    timelineToChanges[rawHighSpeedChange.Timeline] = changes;
                }

                changes.Add(new HighSpeedChange(rawHighSpeedChange.HighSpeed, beat));
            }

            var result = new Dictionary<int, IReadOnlyList<HighSpeedChange>>();
            foreach (var pair in timelineToChanges)
            {
                result[pair.Key] = pair.Value.OrderBy(change => change.Beat).ToList();
            }

            return result;
        }

        /// <summary>
        /// Fatal時に返す最小構成のMainDataを作る。
        /// </summary>
        static BeatmapMainData CreateEmptyMainData()
        {
            var bpmChanges = new List<BpmChange>
            {
                new(120f, 0f),
            };
            var highSpeedChanges = new Dictionary<int, IReadOnlyList<HighSpeedChange>>
            {
                { 0, new List<HighSpeedChange> { new(1f, 0f) } },
            };
            var measureLengthChanges = new List<MeasureLengthChange>
            {
                new(4, 0f),
            };

            var conductorCore = new ConductorCore(new ConductorTiming(bpmChanges, highSpeedChanges, measureLengthChanges));
            return new BeatmapMainData(conductorCore, Array.Empty<NoteCore>());
        }
    }
}
