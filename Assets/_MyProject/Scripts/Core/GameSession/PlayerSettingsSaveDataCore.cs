namespace MyProject.Core
{
    public class PlayerSettingsSaveDataCore
    {
        public float ScrollSpeed { get; }
        public float NoteOffset { get; }

        public PlayerSettingsSaveDataCore(float scrollSpeed, float noteOffset)
        {
            ScrollSpeed = scrollSpeed;
            NoteOffset = noteOffset;
        }
    }
}
