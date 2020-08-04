using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class HitMonsterMessage : MessageHeader
    {
        public override MessageType Type => MessageType.HitMonster;

        public int PlayerID { get; set; }
        public ushort DamageDealt { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteInt(PlayerID);
            writer.WriteUShort(DamageDealt);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            PlayerID = reader.ReadInt();
            DamageDealt = reader.ReadUShort();
        }
    }
}
