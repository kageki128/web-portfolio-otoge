using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using MyProject.Core;
using MyProject.Shared;
using UnityEngine;

namespace MyProject.Infrastructure
{
    /// <summary>
    /// UGCテキストを1行ずつ走査し、Beatmap組み立て用の中間データへ変換する。
    /// </summary>
    internal sealed class BeatmapParser
    {
        static readonly Regex noteRegex = new(@"^#(?<measure>-?\d+)'(?<tick>-?\d+):(?<type>.)(?<lane>.)(?<width>.)(?<attr>.)?$", RegexOptions.Compiled);
        static readonly Regex holdLengthRegex = new(@"^#(?<length>-?\d+)>s$", RegexOptions.Compiled);

        /// <summary>
        /// ヘッダ/タイミング/ノーツを読み取り、中間データとMessageを作る。
        /// </summary>
        public BeatmapParsedData Parse(string beatmapText, CancellationToken ct)
        {
            var parsedData = new BeatmapParsedData();
            // 改行コードを統一して行単位で扱えるようにする。
            var lines = beatmapText
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                var lineNum = i + 1;
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("'"))
                {
                    continue;
                }

                // ヘッダ行とノーツ行で処理を分岐する。
                if (line.StartsWith("@"))
                {
                    ParseDirective(line, lineNum, parsedData);
                    continue;
                }

                ParseNote(lines, ref i, line, lineNum, parsedData);
            }

            return parsedData;
        }

        /// <summary>
        /// @から始まるヘッダ行をキーごとに振り分ける。
        /// </summary>
        static void ParseDirective(string line, int lineNum, BeatmapParsedData parsedData)
        {
            if (!TryParseDirective(line, out var key, out var payload))
            {
                return;
            }

            switch (key)
            {
                case "TITLE":
                    // メタ情報
                    parsedData.Title = payload;
                    break;
                case "ARTIST":
                    parsedData.Artist = payload;
                    break;
                case "DESIGN":
                    parsedData.Designers = payload;
                    break;
                case "DIFF":
                    // 未対応難易度はUnsupportedに落としてMessageに残す。
                    parsedData.Difficulty = ParseDifficulty(payload, lineNum, parsedData);
                    break;
                case "BGMOFS":
                    if (!TryParseFloat(payload, out var waveOffset))
                    {
                        parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] BGMOFS が不正です: {payload}"));
                        waveOffset = 0f;
                    }
                    parsedData.WaveOffset = waveOffset;
                    break;
                case "TICKS":
                    // tick解像度。全体計算に必須。
                    if (!int.TryParse(payload, out var ticks) || ticks <= 0)
                    {
                        parsedData.Messages.Add(new Message(MessageType.Fatal, $"[{lineNum}] TICKS が不正です: {payload}"));
                        parsedData.HasTicks = false;
                    }
                    else
                    {
                        parsedData.HasTicks = true;
                        parsedData.Ticks = ticks;
                    }
                    break;
                case "MAINTIL":
                    // ノーツのデフォルトタイムラインを更新。
                    if (!int.TryParse(payload, out var mainTimeline) || mainTimeline < 0)
                    {
                        parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] MAINTIL が不正です: {payload}"));
                        mainTimeline = 0;
                    }
                    parsedData.CurrentTimeline = mainTimeline;
                    break;
                case "USETIL":
                    // 以降のノーツに適用するタイムラインを切り替える。
                    if (!int.TryParse(payload, out var useTimeline) || useTimeline < 0)
                    {
                        parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] USETIL が不正です: {payload}"));
                        break;
                    }
                    parsedData.CurrentTimeline = useTimeline;
                    break;
                case "BEAT":
                    ParseMeasureLength(payload, lineNum, parsedData);
                    break;
                case "BPM":
                    ParseBpm(payload, lineNum, parsedData);
                    break;
                case "TIL":
                    ParseHighSpeed(payload, lineNum, parsedData);
                    break;
            }
        }

        /// <summary>
        /// DIFF値をDifficultyTypeへ変換する。
        /// </summary>
        static DifficultyType ParseDifficulty(string value, int lineNum, BeatmapParsedData parsedData)
        {
            return value switch
            {
                "0" => DifficultyType.Normal,
                "1" => DifficultyType.Hard,
                _ => AddUnsupportedDifficulty(value, lineNum, parsedData),
            };
        }

        /// <summary>
        /// 未対応難易度の記録を残しつつ、DifficultyType.Unsupportedに変換する。
        /// </summary>
        static DifficultyType AddUnsupportedDifficulty(string value, int lineNum, BeatmapParsedData parsedData)
        {
            parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] 未対応の難易度です: {value}"));
            return DifficultyType.Unsupported;
        }

        /// <summary>
        /// ノーツ行を読み取る。必要なら次行の #N>s を長さとして取り込む。
        /// </summary>
        static void ParseNote(string[] lines, ref int index, string line, int lineNum, BeatmapParsedData parsedData)
        {
            // ノーツ基本形式にマッチするか判定する。
            var match = noteRegex.Match(line);
            if (!match.Success)
            {
                if (!holdLengthRegex.IsMatch(line))
                {
                    parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] 未対応のノーツ行です: {line}"));
                }
                return;
            }

            if (!int.TryParse(match.Groups["measure"].Value, out var measure) || measure < 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] ノーツ小節番号が不正です: {match.Groups["measure"].Value}"));
                return;
            }

            if (!int.TryParse(match.Groups["tick"].Value, out var tick) || tick < 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] ノーツtickが不正です: {match.Groups["tick"].Value}"));
                return;
            }

            var noteType = match.Groups["type"].Value[0];
            var lane = match.Groups["lane"].Value[0];
            var width = match.Groups["width"].Value[0];

            var length = 0;
            // hold/unsupported系は次行の #N>s を長さとして取り込む。
            var shouldReadHoldLength = char.ToLowerInvariant(noteType) == 'h' || !IsSupportedNoteType(noteType);
            if (shouldReadHoldLength && index + 1 < lines.Length)
            {
                var nextLine = lines[index + 1].Trim();
                var holdMatch = holdLengthRegex.Match(nextLine);
                if (holdMatch.Success)
                {
                    if (!int.TryParse(holdMatch.Groups["length"].Value, out length) || length < 0)
                    {
                        parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum + 1}] ホールド長が不正です: {holdMatch.Groups["length"].Value}"));
                        length = 0;
                    }
                    index++;
                }
            }

            // Composer側で使う中間ノーツとして保持する。
            parsedData.RawNotes.Add(new RawNote(parsedData.CurrentTimeline, measure, tick, noteType, lane, width, length, lineNum));
        }

        /// <summary>
        /// 既知ノーツ種別か判定する。未知種別はComposerでUnsupported化する。
        /// </summary>
        static bool IsSupportedNoteType(char noteType)
        {
            var lower = char.ToLowerInvariant(noteType);
            return lower == 't' || lower == 'h';
        }

        /// <summary>
        /// @BEAT行を小節長変化の中間データへ変換する。
        /// </summary>
        static void ParseMeasureLength(string payload, int lineNum, BeatmapParsedData parsedData)
        {
            var parts = SplitByWhitespace(payload);
            if (parts.Length != 3)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BEAT の形式が不正です: {payload}"));
                return;
            }

            if (!int.TryParse(parts[0], out var measure) || measure < 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BEAT の小節番号が不正です: {parts[0]}"));
                return;
            }

            if (!int.TryParse(parts[1], out var numerator) || numerator <= 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BEAT の分子が不正です: {parts[1]}"));
                return;
            }

            if (!int.TryParse(parts[2], out var denominator) || denominator <= 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BEAT の分母が不正です: {parts[2]}"));
                return;
            }

            var lengthBeat = numerator * 4f / denominator;
            if (!Mathf.Approximately(lengthBeat, Mathf.Round(lengthBeat)))
            {
                // Core側は小節長をintで扱うため、小数拍はこの実装では非対応。
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] 整数拍でない小節長は未対応です: {payload}"));
                return;
            }

            parsedData.RawMeasureLengthChanges.Add(new RawMeasureLengthChange(measure, (int)Mathf.Round(lengthBeat), lineNum));
        }

        /// <summary>
        /// @BPM行をBPM変化の中間データへ変換する。
        /// </summary>
        static void ParseBpm(string payload, int lineNum, BeatmapParsedData parsedData)
        {
            var parts = SplitByWhitespace(payload);
            if (parts.Length != 2)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BPM の形式が不正です: {payload}"));
                return;
            }

            if (!TryParseMeasureTick(parts[0], out var measure, out var tick))
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BPM の小節/tick が不正です: {parts[0]}"));
                return;
            }

            if (measure < 0 || tick < 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BPM の小節/tick が範囲外です: {parts[0]}"));
                return;
            }

            if (!TryParseFloat(parts[1], out var bpm) || bpm <= 0f)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @BPM の値が不正です: {parts[1]}"));
                return;
            }

            parsedData.RawBpmChanges.Add(new RawBpmChange(measure, tick, bpm));
        }

        /// <summary>
        /// @TIL行をハイスピ変化の中間データへ変換する。
        /// </summary>
        static void ParseHighSpeed(string payload, int lineNum, BeatmapParsedData parsedData)
        {
            var parts = SplitByWhitespace(payload);
            if (parts.Length != 3)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @TIL の形式が不正です: {payload}"));
                return;
            }

            if (!int.TryParse(parts[0], out var timeline) || timeline < 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @TIL のタイムラインが不正です: {parts[0]}"));
                return;
            }

            if (!TryParseMeasureTick(parts[1], out var measure, out var tick))
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @TIL の小節/tick が不正です: {parts[1]}"));
                return;
            }

            if (measure < 0 || tick < 0)
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @TIL の小節/tick が範囲外です: {parts[1]}"));
                return;
            }

            if (!TryParseFloat(parts[2], out var highSpeed))
            {
                parsedData.Messages.Add(new Message(MessageType.Error, $"[{lineNum}] @TIL の値が不正です: {parts[2]}"));
                return;
            }

            parsedData.RawHighSpeedChanges.Add(new RawHighSpeedChange(timeline, measure, tick, highSpeed));
        }

        /// <summary>
        /// @KEY VALUE... の形式をキーとペイロードに分解する。
        /// </summary>
        static bool TryParseDirective(string line, out string key, out string payload)
        {
            key = string.Empty;
            payload = string.Empty;

            if (!line.StartsWith("@"))
            {
                return false;
            }

            var separatorIndex = line.IndexOfAny(new[] { ' ', '\t' });
            if (separatorIndex < 0)
            {
                key = line[1..].ToUpperInvariant();
                return true;
            }

            key = line[1..separatorIndex].ToUpperInvariant();
            payload = line[(separatorIndex + 1)..].Trim();
            return true;
        }

        /// <summary>
        /// measure'tick 形式を数値へ分解する。
        /// </summary>
        static bool TryParseMeasureTick(string token, out int measure, out int tick)
        {
            measure = 0;
            tick = 0;

            var separatorIndex = token.IndexOf('\'');
            if (separatorIndex < 0)
            {
                return false;
            }

            var measureText = token[..separatorIndex];
            var tickText = token[(separatorIndex + 1)..];

            return int.TryParse(measureText, out measure) && int.TryParse(tickText, out tick);
        }

        /// <summary>
        /// 小数をカルチャ非依存でパースする。
        /// </summary>
        static bool TryParseFloat(string value, out float parsed)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
        }

        /// <summary>
        /// 連続空白を無視してトークン分割する。
        /// </summary>
        static string[] SplitByWhitespace(string value)
        {
            return value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
