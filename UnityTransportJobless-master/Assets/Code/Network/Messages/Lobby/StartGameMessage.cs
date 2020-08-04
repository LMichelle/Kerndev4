using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class StartGameMessage : MessageHeader
    {
        public override MessageType Type => MessageType.StartGame;

        public ushort StartHP { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteUShort(StartHP);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            StartHP = reader.ReadUShort();
        }
    }
}
