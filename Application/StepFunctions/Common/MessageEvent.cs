namespace Common
{
    public class MessageEvent
    {
        public MessageEvent()
        {
        }

        public MessageEvent(string targetUser, string resourceId)
        {
            TargetUser = targetUser;
            ResourceId = resourceId;
        }

        public string TargetUser { get; set; }

        public string ResourceId { get; set; }

        public string Message { get; set; }

        public string Data { get; set; }

        public bool CompleteEvent { get; set; }
    }
}