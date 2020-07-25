using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;
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
