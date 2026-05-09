using System.Collections.Generic;
using MyProject.Shared;

namespace MyProject.Core
{
    public class Beatmap
    {
        public BeatmapMetaData MetaData { get; }
        public BeatmapMainData MainData { get; }

        public IReadOnlyList<Message> Messages { get; }

        public Beatmap
        (
            BeatmapMetaData metaData,
            BeatmapMainData mainData,
            IReadOnlyList<Message> messages
        )
        {
            MetaData = metaData;
            MainData = mainData;
            Messages = messages;
        }
    }
}
