using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class PlayerLeftMessage : MessageHeader
    {
        public override MessageType Type => MessageType.PlayerLeft;

        public int PlayerLeftID { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteInt(PlayerLeftID);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            PlayerLeftID = reader.ReadInt();
        }
    }
}
