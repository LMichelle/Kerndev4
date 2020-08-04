using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class HitByMonsterMessage : MessageHeader
    {
        public override MessageType Type => MessageType.HitByMonster;

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
