using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class RequestDeniedMessage : MessageHeader
    {
        public override MessageType Type => MessageType.RequestDenied;

        public uint DeniedMessageID { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteUInt(DeniedMessageID);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            DeniedMessageID = reader.ReadUInt();
        }
    }
}
