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
        Connection = connection;
        Host = host;
    }

    /// <summary>
    /// Takes a random color and assigns it to the property PlayerColour.
    /// </summary>
    public void AssignRandomColor()
    {
        Color32 color = (Color32)Random.ColorHSV(0f, 1f, .3f, 1f, 1f, 1f, 1f, 1f);
        PlayerColour = ColorExtensions.ColorToUInt(color);
    }
}
