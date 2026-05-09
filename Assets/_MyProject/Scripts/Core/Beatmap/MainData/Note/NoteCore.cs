namespace MyProject.Core
{
    public class NoteCore
    {
        public NoteProperty Property { get; }

        public NoteCore(NoteProperty property)
        {
            Property = property;
        }
    }
}
