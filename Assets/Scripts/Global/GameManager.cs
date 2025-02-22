using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Team")]
    public byte teamId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
