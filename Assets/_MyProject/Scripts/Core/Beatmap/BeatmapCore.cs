using System.Collections.Generic;
using MyProject.Shared;
using R3;

namespace MyProject.Core
{
    public class BeatmapCore
    {
        public BeatmapMetaData MetaData => metaData;
        public IReadOnlyList<NoteCore> NoteCores => mainData.NoteCores;
        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll => mainData.ConductorCore.TimelineToCurrentScroll;
        public IReadOnlyList<Message> Messages => messages;

        readonly BeatmapMetaData metaData;
        readonly BeatmapMainData mainData;
        readonly IReadOnlyList<Message> messages;

        public BeatmapCore
        (
            BeatmapMetaData metaData,
            BeatmapMainData mainData,
            IReadOnlyList<Message> messages
        )
        {
            this.metaData = metaData;
            this.mainData = mainData;
            this.messages = messages;
        }


    }
}
