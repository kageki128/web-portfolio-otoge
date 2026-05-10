using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using MyProject.Shared;
using System;
using UnityEngine;

namespace MyProject.Infrastructure
{
    /// <summary>
    /// Beatmap読み込みのエントリーポイント。ParseとComposeを順に実行する。
    /// </summary>
    public class BeatmapRepository : IBeatmapRepository
    {
        readonly BeatmapFilesSO beatmapFiles;
        readonly BeatmapParser parser = new();
        readonly BeatmapComposer composer = new();

        public BeatmapRepository(BeatmapFilesSO beatmapFiles)
        {
            this.beatmapFiles = beatmapFiles;
        }

        /// <summary>
        /// ScriptableObjectから譜面テキストと音源を取得し、Beatmapへ変換する。
        /// </summary>
        public UniTask<BeatmapCore> GetAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (beatmapFiles.Wave == null)
            {
                throw new InvalidOperationException("BeatmapFilesSO.Wave is not assigned.");
            }

            if (beatmapFiles.Beatmap == null)
            {
                throw new InvalidOperationException("BeatmapFilesSO.Beatmap is not assigned.");
            }

            // 1) テキストを中間データにパース
            var parsedData = parser.Parse(beatmapFiles.Beatmap.text, ct);
            // 2) 中間データから最終Beatmapを組み立て
            var beatmap = composer.Compose(beatmapFiles.Wave, parsedData, ct);
            DebugBeatmap(beatmap);

            return UniTask.FromResult(beatmap);
        }

        void DebugBeatmap(BeatmapCore beatmap)
        {
            var meta = beatmap.MetaData;
            var notes = beatmap.NoteCores;
            var messages = beatmap.Messages;
            var timelines = beatmap.TimelineToCurrentScroll.Keys.OrderBy(timeline => timeline).ToArray();

            var noteTypeSummary = string.Join(", ", notes
                .GroupBy(note => note.Property.Type)
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
            LogAllNotes(notes);
            LogMessages(messages);
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
                $"  clip={meta.Wave.name}\n" +
                $"  length={meta.Wave.length:F3}s\n" +
                $"  samples={meta.Wave.samples}\n" +
                $"  frequency={meta.Wave.frequency}\n" +
                $"  channels={meta.Wave.channels}"
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

        void LogAllNotes(IReadOnlyList<NoteCoreBase> notes)
        {
            for (var i = 0; i < notes.Count; i++)
            {
                var property = notes[i].Property;
                Debug.Log
                (
                    $"[BeatmapRepository] Note[{i}] " +
                    $"type={property.Type}, " +
                    $"timeline={property.Timeline}, " +
                    $"lane={property.Lane}, " +
                    $"width={property.Width}, " +
                    $"layer={property.Layer}, " +
                    $"beat={property.TimingBegin.Beat:F3}->{property.TimingEnd.Beat:F3}, " +
                    $"sec={property.TimingBegin.Sec:F3}->{property.TimingEnd.Sec:F3}, " +
                    $"measure={property.TimingBegin.Measure}->{property.TimingEnd.Measure}, " +
                    $"scroll={property.ScrollBegin:F3}->{property.ScrollEnd:F3}"
                );
            }
        }

        void LogMessages(IReadOnlyList<Message> messages)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                Debug.Log($"[BeatmapRepository] Message[{i}] type={message.Type}, content={message.Content}");
            }
        }
    }
}
