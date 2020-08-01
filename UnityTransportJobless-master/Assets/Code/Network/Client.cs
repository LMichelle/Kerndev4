using UnityEngine;
using Unity.Networking.Transport;

public class Client
{
    public int PlayerID;
    public string PlayerName;
    public uint PlayerColour;
    public NetworkConnection Connection;
    public bool Host;

    public int StartHP { get; set; } 

    public Client(int playerID, string playerName, NetworkConnection connection, bool host)
    {
        PlayerID = playerID;
        PlayerName = playerName;
        Color32 color = (Color32)Random.ColorHSV(0f, 1f, .7f, 1f, 0.7f, 1f, 1f, 1f);
        PlayerColour = ColorExtensions.ToUInt(color);
        Connection = connection;
        Host = host;
    }
}
