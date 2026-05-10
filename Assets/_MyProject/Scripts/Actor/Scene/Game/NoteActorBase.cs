using MyProject.Core;
using UnityEngine;

namespace MyProject.Actor
{
    public abstract class NoteActorBase : ActorBase
    {
        public NoteCore NoteCore { get; private set; }

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

        protected static float CalculateCenterX(int lane, int width)
        {
            return lane + ((width - 1) * 0.5f);
        }
        protected static float CalculateCenterY(float scrollBegin, float scrollEnd, float currentScroll, float scrollSpeed)
        {
            float beginY = (scrollBegin - currentScroll) * scrollSpeed;
            float endY = (scrollEnd - currentScroll) * scrollSpeed;
            return (beginY + endY) / 2f;
        }
        protected static float CalculateHeight(float scrollBegin, float scrollEnd, float scrollSpeed)
        {
            float beginY = scrollBegin * scrollSpeed;
            float endY = scrollEnd * scrollSpeed;
            return Mathf.Abs(endY - beginY);
        }
    }
}
