using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class PlayerDefendsMessage : MessageHeader
    {
        public override MessageType Type => MessageType.PlayerDefends;

        public int PlayerID { get; set; }
        public ushort NewHP { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteInt(PlayerID);
            writer.WriteUShort(NewHP);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            PlayerID = reader.ReadInt();
            NewHP = reader.ReadUShort();
        }
    }
}
