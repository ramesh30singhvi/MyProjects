using SHARP.Common.Enums;

namespace SHARP.Common.Models
{
    public class MessageResponse
    {
        public MessageType Status { get; set; }

        public string Message { get; set; }
    }
}
