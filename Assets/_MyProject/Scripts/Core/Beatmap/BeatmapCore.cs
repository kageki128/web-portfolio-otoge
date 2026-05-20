using System.Collections.Generic;
using MyProject.Shared;
using R3;

namespace MyProject.Core
{
    public class BeatmapCore
    {
        public BeatmapMetaData MetaData => metaData;
        public IReadOnlyList<NoteCoreBase> NoteCores => mainData.NoteCores;
        public ReadOnlyReactiveProperty<float> CurrentBeat => mainData.ConductorCore.CurrentBeat;
        public ReadOnlyReactiveProperty<float> CurrentSec => mainData.ConductorCore.CurrentSec;
        public ReadOnlyReactiveProperty<OtogeType> CurrentOtogeType => mainData.ConductorCore.CurrentOtogeType;
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

        public double Start(double delaySec) => mainData.ConductorCore.Start(delaySec);
        public void AdvanceTime() => mainData.ConductorCore.Advance();
    }
}
