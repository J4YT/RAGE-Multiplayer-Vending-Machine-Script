using RAGE;

namespace Attachment_Sync.Handlers
{
    public static class AttachmentHandler
    {
        public static void Add(string attachment)
        {
            Events.CallRemote("staticAttachments.Add", attachment);
        }
        public static void Remove(string attachment)
        {
            Events.CallRemote("staticAttachments.Remove", attachment);
        }
    }
}
