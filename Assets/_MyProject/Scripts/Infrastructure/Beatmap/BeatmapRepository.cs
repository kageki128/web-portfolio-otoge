using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using MyProject.Shared;
using UnityEngine;

namespace MyProject.Infrastructure
{
    /// <summary>
    /// Beatmap読み込みのエントリーポイント。ParseとComposeを順に実行する。
    /// </summary>
    public class BeatmapRepository : IBeatmapRepository
    {
        readonly BeatmapListSO beatmapList;
        readonly BeatmapParser parser = new();
        readonly BeatmapComposer composer = new();

        public BeatmapRepository(BeatmapListSO beatmapList)
        {
            this.beatmapList = beatmapList;
        }

        /// <summary>
        /// ScriptableObjectから譜面テキストと音源を取得し、Beatmapへ変換する。
        /// </summary>
        public async UniTask<BeatmapCore> GetAsync(BeatmapType type, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (beatmapList == null)
            {
                throw new InvalidOperationException("BeatmapListSO is not assigned.");
            }

            var beatmapFiles = beatmapList.Get(type);
            if (beatmapFiles == null)
            {
                throw new InvalidOperationException($"BeatmapListSO.{type} is not assigned.");
            }

            if (beatmapFiles.Beatmap == null)
            {
                throw new InvalidOperationException("BeatmapFilesSO.Beatmap is not assigned.");
            }

            if (beatmapFiles.OtogeChanges == null)
            {
                throw new InvalidOperationException("BeatmapFilesSO.OtogeChanges is not assigned.");
            }

            if (beatmapFiles.OtogeEvents == null)
            {
                throw new InvalidOperationException("BeatmapFilesSO.OtogeEvents is not assigned.");
            }

            var wave = beatmapFiles.Wave;
            var beatmapText = beatmapFiles.Beatmap.text;
            var otogeChanges = beatmapFiles.OtogeChanges.OtogeChanges;
            var otogeEventBeats = beatmapFiles.OtogeEvents.OtogeEventBeats.ToArray();

            BeatmapCore beatmap;
#if UNITY_WEBGL && !UNITY_EDITOR
            var parsedData = parser.Parse(beatmapText, ct);
            beatmap = composer.Compose(type, wave, parsedData, otogeChanges, otogeEventBeats, ct);
            await UniTask.CompletedTask;
#else
            beatmap = await UniTask.RunOnThreadPool
            (
                () =>
                {
                    // 1) テキストを中間データにパース
                    var parsedData = parser.Parse(beatmapText, ct);
                    // 2) 中間データから最終Beatmapを組み立て
                    return composer.Compose(type, wave, parsedData, otogeChanges, otogeEventBeats, ct);
                },
                configureAwait: true,
                cancellationToken: ct
            );
#endif
            DebugBeatmap(beatmap);

            return beatmap;
        }

        void DebugBeatmap(BeatmapCore beatmap)
        {
            var meta = beatmap.MetaData;
            var notes = beatmap.NoteCores;
            var messages = beatmap.Messages;
            var timelines = beatmap.TimelineToCurrentScroll.Keys.OrderBy(timeline => timeline).ToArray();

            var noteTypeSummary = string.Join(", ", notes
                .GroupBy(note => note.Property.NoteType)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Key}:{group.Count()}"));
            var timelineSummary = string.Join(", ", notes
                .GroupBy(note => note.Property.Timeline)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Key}:{group.Count()}"));
            var laneSummary = string.Join(", ", notes
                .GroupBy(note => note.Property.Lane)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Key}:{group.Count()}"));
            var messageTypeSummary = string.Join(", ", messages
                .GroupBy(message => message.Type)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Key}:{group.Count()}"));

            LogMeta(meta);
            LogAudio(meta);
            LogSummary(notes, messages, timelines, noteTypeSummary, timelineSummary, laneSummary, messageTypeSummary);
        }

        void LogMeta(BeatmapMetaData meta)
        {
            Debug.Log
            (
                "[BeatmapRepository] Beatmap Meta\n" +
                $"  title={meta.Title}\n" +
                $"  artist={meta.Artist}\n" +
                $"  designers={meta.NoteDesigners}\n" +
                $"  difficulty={meta.Difficulty}\n" +
                $"  waveOffset={meta.WaveOffset:F3}s\n" +
                $"  bpm(min/max)={meta.MinBpm:F3}/{meta.MaxBpm:F3}"
            );
        }

        void LogAudio(BeatmapMetaData meta)
        {
            Debug.Log
            (
                "[BeatmapRepository] Beatmap Audio\n" +
                (meta.Wave == null
                    ? "  clip=(none)"
                    : $"  clip={meta.Wave.name}\n" +
                      $"  length={meta.Wave.length:F3}s\n" +
                      $"  samples={meta.Wave.samples}\n" +
                      $"  frequency={meta.Wave.frequency}\n" +
                      $"  channels={meta.Wave.channels}")
            );
        }

        void LogSummary
        (
            IReadOnlyList<NoteCoreBase> notes,
            IReadOnlyList<Message> messages,
            int[] timelines,
            string noteTypeSummary,
            string timelineSummary,
            string laneSummary,
            string messageTypeSummary
        )
        {
            Debug.Log
            (
                "[BeatmapRepository] Beatmap Summary\n" +
                $"  notes={notes.Count}\n" +
                $"  noteTypes=[{noteTypeSummary}]\n" +
                $"  timelines=[{timelineSummary}]\n" +
                $"  lanes=[{laneSummary}]\n" +
                $"  conductorTimelines=[{string.Join(", ", timelines)}]\n" +
                $"  messages={messages.Count} ({messageTypeSummary})"
            );
        }

    }
}
