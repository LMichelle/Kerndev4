using Unity.Networking.Transport;

namespace Assets.Code
{
    public class NewPlayerMessage : MessageHeader
    {
        public override MessageType Type => MessageType.NewPlayer;

        public int PlayerID { get; set; }
        public uint PlayerColour { get; set; }
        public string PlayerName { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteInt(PlayerID);
            writer.WriteUInt(PlayerColour);
            writer.WriteString(PlayerName);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            PlayerID = reader.ReadInt();
            PlayerColour = reader.ReadUInt();
            PlayerName = reader.ReadString().ToString();
        }
    }
}
