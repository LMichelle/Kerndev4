using Unity.Networking.Transport;

namespace KernDev.NetworkBehaviour
{
    public abstract class MessageHeader
    {
        private static uint nextID = 0;
        public static uint NextID => ++nextID;

        public enum MessageType
        {
            None = 0,
            NewPlayer, // Lobby begin
            Welcome,
            SetName,
            RequestDenied,
            PlayerLeft,
            StartGame, // Lobby end
            PlayerTurn, // Game Protocol
            RoomInfo,
            PlayerEnterRoom,
            PlayerLeaveRoom,
            ObtainTreasure,
            HitMonster,
            HitByMonster,
            PlayerDefends,
            PlayerLeftDungeon,
            PlayerDies,
            EndGame,
            MoveRequest,
            AttackRequest,
            DefendRequest,
            ClaimTreasureRequest,
            LeaveDungeonRequest, // Game End
            Count
        }

        public abstract MessageType Type { get; }
        public uint ID { get; private set; } = NextID;

        public virtual void SerializeObject(ref DataStreamWriter writer)
        {
            writer.WriteUShort((ushort)Type);
            writer.WriteUInt(ID);
        }

        public virtual void DeserializeObject(ref DataStreamReader reader)
        {
            ID = reader.ReadUInt();
        }
    }
}
