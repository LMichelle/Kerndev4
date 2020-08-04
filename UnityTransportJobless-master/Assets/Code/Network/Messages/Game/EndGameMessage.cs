using Unity.Networking.Transport;
using System.Collections.Generic;

namespace KernDev.NetworkBehaviour
{
    public class EndGameMessage : MessageHeader
    {
        public override MessageType Type => MessageType.EndGame;

        public byte NumberOfScores { get; set; }
        private List<int> playerID = new List<int>();
        public List<int> PlayerID {
            get { return playerID; }
            set { playerID = value; }
        }

        private List<ushort> highScores = new List<ushort>();
        public List<ushort> HighScores  {
            get { return highScores; }
            set { highScores = value; }
        }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteByte(NumberOfScores);
            for (int i = 0; i <= NumberOfScores; i++)
            {
                writer.WriteInt(PlayerID[i]);
                writer.WriteUShort(HighScores[i]);
            }
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            NumberOfScores = reader.ReadByte();
            for (int i = 0; i <= NumberOfScores; i++)
            {
                PlayerID[i] = reader.ReadInt();
                HighScores[i] = reader.ReadUShort();
            }
        }
    }
}
