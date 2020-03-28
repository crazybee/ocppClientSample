using Newtonsoft.Json;
using System.IO;

namespace ocpprequestsender
{
    public class TransportLayerMessage
    {
        public string UniqueId { get; set; }
        public MessageTypes MessageType { get; set; }
        public string Action { get; set; }
        public string Payload { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDetails { get; set; }
        public string ErrorDescription { get; set; }
        public string ToJson()
        {
           var textwriter = new StringWriter();
            var writer = new JsonTextWriter(textwriter);
            writer.WriteStartArray();
            writer.WriteValue((int)this.MessageType);
            writer.WriteValue(this.UniqueId);

            if (this.MessageType == MessageTypes.CALL)
            {
                writer.WriteValue(this.Action);

                writer.WriteRawValue(this.Payload);
            }

            if (this.MessageType == MessageTypes.CALLRESULT)
            {
                writer.WriteRawValue(this.Payload);
            }

            if (this.MessageType == MessageTypes.CALLERROR)
            {
                writer.WriteValue(this.ErrorCode);
                writer.WriteValue(this.ErrorDescription);
                writer.WriteRawValue(this.ErrorDetails);
            }
            writer.WriteEndArray();
            writer.Flush();
            return textwriter.ToString();
        }

    }
}
