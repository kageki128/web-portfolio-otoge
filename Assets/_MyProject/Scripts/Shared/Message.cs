namespace MyProject.Shared
{
    public class Message
    {
        public MessageType Type { get; }
        public string Content { get; }

        public Message(MessageType type, string content)
        {
            Type = type;
            Content = content;
        }
    }
}
