using MyProject.Core;

namespace MyProject.Actor
{
    public abstract class NoteActorBase : ActorBase
    {
        protected NoteCore NoteCore;

        public void InstallCore(NoteCore noteCore)
        {
            NoteCore = noteCore;
            SetWidth(noteCore.Property.Width);
            SetLayer(noteCore.Property.Layer);
        }

        public abstract void SetPosition(float currentScroll, float scrollSpeed);

        public void Destroy()
        {
            Destroy(gameObject);
        }

        protected abstract void SetWidth(int width);
        protected abstract void SetLayer(int layer);
    }
}
