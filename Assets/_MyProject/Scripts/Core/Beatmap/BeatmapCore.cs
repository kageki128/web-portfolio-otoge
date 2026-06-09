using System.Collections.Generic;
using MyProject.Shared;
using R3;

namespace MyProject.Core
{
    public class BeatmapCore
    {
        public BeatmapType BeatmapType { get; }
        public BeatmapMetaData MetaData => metaData;
        public IReadOnlyList<NoteCoreBase> NoteCores => mainData.NoteCores;
        public IReadOnlyList<MeasureLineCore> MeasureLineCores => mainData.MeasureLineCores;
        public ReadOnlyReactiveProperty<float> CurrentBeat => mainData.ConductorCore.CurrentBeat;
        public ReadOnlyReactiveProperty<float> CurrentSec => mainData.ConductorCore.CurrentSec;
        public ReadOnlyReactiveProperty<OtogeTypeTransition> CurrentOtogeTypeTransition => mainData.ConductorCore.CurrentOtogeTypeTransition;
        public Observable<Unit> OtogeEvent => mainData.ConductorCore.OtogeEvent;
        public Observable<Unit> EndReached => mainData.ConductorCore.EndReached;
        public IReadOnlyDictionary<int, ReadOnlyReactiveProperty<float>> TimelineToCurrentScroll => mainData.ConductorCore.TimelineToCurrentScroll;
        public IReadOnlyList<Message> Messages => messages;

        readonly BeatmapMetaData metaData;
        readonly BeatmapMainData mainData;
        readonly IReadOnlyList<Message> messages;

        public BeatmapCore
        (
            BeatmapType beatmapType,
            BeatmapMetaData metaData,
            BeatmapMainData mainData,
            IReadOnlyList<Message> messages
        )
        {
            BeatmapType = beatmapType;
            this.metaData = metaData;
            this.mainData = mainData;
            this.messages = messages;
        }

        public double Start(double delaySec) => mainData.ConductorCore.Start(delaySec);
        public void AdvanceTime(float secOffset) => mainData.ConductorCore.Advance(secOffset);
    }
}
