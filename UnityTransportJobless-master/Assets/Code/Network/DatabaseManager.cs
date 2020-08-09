using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace KernDev.NetworkBehaviour
{
    /// <summary>
    /// Thank you Geoffrey for showing me this code!!
    /// </summary>
    public static class DatabaseManager
    {
        public static string response;
        public readonly static string homeURL = "https://studenthome.hku.nl/~lisa.perelaer/kerndev4/";
        public static int serverID = 1;
        public static string serverPassword = "servTest396";
        public static string sessionID;
        public static UserData userData;

        public static IEnumerator GetHTTP(string url = "url")
        {
            var request = UnityWebRequest.Get(homeURL + url);
            {
                yield return request.SendWebRequest();

                if (request.isDone && !request.isHttpError)
                    response = request.downloadHandler.text;
            }
        }
    }

    [System.Serializable]
    public struct UserData
    {
        public int id;
        public string FirstName;
        public string LastName;
    }
}
