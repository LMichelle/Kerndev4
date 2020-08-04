using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class MoveRequestMessage : MessageHeader
    {
        public override MessageType Type => MessageType.MoveRequest;

        public byte Direction { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteByte(Direction);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            Direction = reader.ReadByte();
        }
    }
}
