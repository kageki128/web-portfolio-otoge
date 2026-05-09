using System.Collections.Generic;
using MyProject.Core;
using MyProject.Shared;

namespace MyProject.Infrastructure
{
    internal sealed class BeatmapParsedData
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Designers { get; set; } = string.Empty;
        public DifficultyType Difficulty { get; set; } = DifficultyType.Normal;
        public float WaveOffset { get; set; }

        public int CurrentTimeline { get; set; }
        public bool HasTicks { get; set; }
        public int Ticks { get; set; }

        public List<Message> Messages { get; } = new();
        public List<RawMeasureLengthChange> RawMeasureLengthChanges { get; } = new();
        public List<RawBpmChange> RawBpmChanges { get; } = new();
        public List<RawHighSpeedChange> RawHighSpeedChanges { get; } = new();
        public List<RawNote> RawNotes { get; } = new();
    }

    internal readonly struct RawMeasureLengthChange
    {
        public int Measure { get; }
        public int Length { get; }
        public int LineNum { get; }

        public RawMeasureLengthChange(int measure, int length, int lineNum)
        {
            Measure = measure;
            Length = length;
            LineNum = lineNum;
        }
    }

    internal readonly struct RawBpmChange
    {
        public int Measure { get; }
        public int Tick { get; }
        public float Bpm { get; }

        public RawBpmChange(int measure, int tick, float bpm)
        {
            Measure = measure;
            Tick = tick;
            Bpm = bpm;
        }
    }

    internal readonly struct RawHighSpeedChange
    {
        public int Timeline { get; }
        public int Measure { get; }
        public int Tick { get; }
        public float HighSpeed { get; }

        public RawHighSpeedChange(int timeline, int measure, int tick, float highSpeed)
        {
            Timeline = timeline;
            Measure = measure;
            Tick = tick;
            HighSpeed = highSpeed;
        }
    }

    internal readonly struct RawNote
    {
        public int Timeline { get; }
        public int Measure { get; }
        public int Tick { get; }
        public char NoteType { get; }
        public char Lane { get; }
        public char Width { get; }
        public int Length { get; }
        public int LineNum { get; }

        public RawNote(int timeline, int measure, int tick, char noteType, char lane, char width, int length, int lineNum)
        {
            Timeline = timeline;
            Measure = measure;
            Tick = tick;
            NoteType = noteType;
            Lane = lane;
            Width = width;
            Length = length;
            LineNum = lineNum;
        }
    }

    internal readonly struct MeasureEntry
    {
        public int Measure { get; }
        public int Length { get; }
        public float BeatStart { get; }

        public MeasureEntry(int measure, int length, float beatStart)
        {
            Measure = measure;
            Length = length;
            BeatStart = beatStart;
        }
    }
}
