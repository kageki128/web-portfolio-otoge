using System.Collections.Generic;

namespace MyProject.Core
{
    public class BeatmapMainData
    {
        public ConductorCore ConductorCore { get; }
        public IReadOnlyList<NoteCore> NoteCores { get; }

        public BeatmapMainData(ConductorCore conductorCore, IReadOnlyList<NoteCore> noteCores)
        {
            ConductorCore = conductorCore;
            NoteCores = noteCores;
        }
    }
}
