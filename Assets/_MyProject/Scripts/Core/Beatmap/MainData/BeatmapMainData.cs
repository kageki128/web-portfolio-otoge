using System.Collections.Generic;

namespace MyProject.Core
{
    public class BeatmapMainData
    {
        public ConductorCore ConductorCore { get; }
        public IReadOnlyList<NoteCoreBase> NoteCores { get; }

        public BeatmapMainData(ConductorCore conductorCore, IReadOnlyList<NoteCoreBase> noteCores)
        {
            ConductorCore = conductorCore;
            NoteCores = noteCores;
        }
    }
}
