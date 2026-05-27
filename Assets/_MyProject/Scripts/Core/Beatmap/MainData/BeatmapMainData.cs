using System.Collections.Generic;

namespace MyProject.Core
{
    public class BeatmapMainData
    {
        public ConductorCore ConductorCore { get; }
        public IReadOnlyList<NoteCoreBase> NoteCores { get; }
        public IReadOnlyList<MeasureLineCore> MeasureLineCores { get; }

        public BeatmapMainData
        (
            ConductorCore conductorCore,
            IReadOnlyList<NoteCoreBase> noteCores,
            IReadOnlyList<MeasureLineCore> measureLineCores
        )
        {
            ConductorCore = conductorCore;
            NoteCores = noteCores;
            MeasureLineCores = measureLineCores;
        }
    }
}
