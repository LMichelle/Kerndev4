﻿using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public class PlayerTurnMessage : MessageHeader
    {
        public override MessageType Type => MessageType.PlayerTurn;

        public int PlayerID { get; set; }

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteInt(PlayerID);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            PlayerID = reader.ReadInt();
        }
    }
}
