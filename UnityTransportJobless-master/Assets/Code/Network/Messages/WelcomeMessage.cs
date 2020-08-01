using Unity.Networking.Transport;

namespace Assets.Code
{
    public class WelcomeMessage : MessageHeader
    {
        public override MessageType Type => MessageType.Welcome;

        public int PlayerID { get; set; }
        public uint PlayerColour { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteInt(PlayerID);
            writer.WriteUInt(PlayerColour);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            PlayerID = reader.ReadInt();
            PlayerColour = reader.ReadUInt();
        }
    }
}
