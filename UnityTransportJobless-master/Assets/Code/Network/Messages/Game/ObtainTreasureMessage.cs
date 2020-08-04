using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class ObtainTreasureMessage : MessageHeader
    {
        public override MessageType Type => MessageType.ObtainTreasure;

        public ushort Amount { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteUShort(Amount);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            Amount = reader.ReadUShort();
        }
    }
}
