using System.Collections.Generic;
using MyProject.Core;
using MyProject.Shared;

namespace MyProject.Infrastructure
{
    public class BeatmapParsedData
    {
        public string Id { get; set; } = string.Empty;
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
}
