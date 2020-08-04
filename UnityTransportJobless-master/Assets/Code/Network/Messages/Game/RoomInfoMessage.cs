using Unity.Networking.Transport;
using System.Collections.Generic;

namespace KernDev.NetworkBehaviour
{
    public class RoomInfoMessage : MessageHeader
    {
        public override MessageType Type => MessageType.RoomInfo;

        public byte MoveDirections { get; set; }
        public ushort TreasureInRoom { get; set; }
        public byte ContainsMonster { get; set; }
        public byte ContainsExit { get; set; }
        public byte NumberOfOtherPlayers { get; set; }

        // the data stream gets longer with the amount 
        private List<int> otherPlayerIDs = new List<int>();
        public List<int> OtherPlayerIDs { 
            get { 
                return otherPlayerIDs;
            }
            set { otherPlayerIDs = value; } }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteByte(MoveDirections);
            writer.WriteUShort(TreasureInRoom);
            writer.WriteByte(ContainsMonster);
            writer.WriteByte(ContainsExit);
            writer.WriteByte(NumberOfOtherPlayers);
            for (int i = 0; i < NumberOfOtherPlayers; i++)
            {
                writer.WriteInt(OtherPlayerIDs[i]);
            }
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            MoveDirections = reader.ReadByte();
            TreasureInRoom = reader.ReadUShort();
            ContainsMonster = reader.ReadByte();
            ContainsExit = reader.ReadByte();
            NumberOfOtherPlayers = reader.ReadByte();
            OtherPlayerIDs.Clear();

            for (int i = 0; i < NumberOfOtherPlayers; i++)
            {
                OtherPlayerIDs.Add(reader.ReadInt());
            }
        }
    }
}
