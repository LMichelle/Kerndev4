﻿using Unity.Networking.Transport;
using Assets.Code;
public class MessageConnection
{
    public NetworkConnection connection;
    public MessageHeader messageHeader;

    public MessageConnection(NetworkConnection connection, MessageHeader message)
    {
        this.connection = connection;
        this.messageHeader = message;
    }
}
