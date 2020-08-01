using Unity.Networking.Transport;

namespace Assets.Code
{
    public class SetNameMessage : MessageHeader
    {
        public override MessageType Type => MessageType.SetName;

        public string Name { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);

            writer.WriteString(Name);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);

            Name = reader.ReadString().ToString();
        }
    }
}
