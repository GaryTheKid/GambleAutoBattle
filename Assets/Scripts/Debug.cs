using UnityEngine;
using Unity.Netcode;
using System;

public class DEBUG : MonoBehaviour
{
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    internal static void Log(string v)
    {
        throw new NotImplementedException();
    }
}
