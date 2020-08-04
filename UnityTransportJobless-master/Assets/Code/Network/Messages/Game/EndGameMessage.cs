using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class EndGameMessage : MessageHeader
    {
        public override MessageType Type => MessageType.EndGame;

        public byte NumberOfScores { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteByte(NumberOfScores);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            NumberOfScores = reader.ReadByte();
        }
    }
}
