using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public enum GameState
{
    Preparation,
    InBattle,
    GameOver
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Team")]
    public byte myTeamId;
    public SpawnRegion mySpawnRegion;

    [SerializeField] private GameState currentGameState = GameState.Preparation;
    public GameState CurrentGameState => currentGameState;

    [Header("Spawn Region")]
    public SpawnRegion blueSpawnRegion;
    public SpawnRegion redSpawnRegion;

    [Header("UI")]
    public GameObject battleBeginText;
    public GameObject winText;
    public GameObject loseText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        // Keep checking if we can start the game
        if (currentGameState == GameState.Preparation)
        {
            TryStartGame();
        }
    }

    #region === Game State ===
    public void TryStartGame()
    {
        if (currentGameState != GameState.Preparation) return;
        if (!BattleSimulator_Server.Instance.IsGameReady()) return;

        Debug.Log("[GameManager] All players ready. Starting battle...");

        // Update game state
        currentGameState = GameState.InBattle;
        UpdateGameStateClientRpc((byte)GameState.InBattle);

        // Start simulation on server
        BattleSimulator_Server.Instance.StartSimulation();

        // Start visualization on client
        StartBattleVisualizationClientRpc();

        // Start battle BGM
        StartBattleMusicClientRpc();
    }

    public void EndGame(byte losingTeamId)
    {
        if (currentGameState != GameState.InBattle) return;

        Debug.Log($"[GameManager] Game ended. Result: Team {losingTeamId} Lost !");

        // Update game state
        currentGameState = GameState.GameOver;
        UpdateGameStateClientRpc((byte)GameState.GameOver);

        // Stop simulation on server
        BattleSimulator_Server.Instance.StopSimulation();

        // Stop visualization on client
        StopBattleVisualizationClientRpc();

        // End game UI and BGM will be triggered on clients
        SendGameResultClientRpc(losingTeamId);
    }

    [ClientRpc]
    private void StartBattleVisualizationClientRpc()
    {
        battleBeginText.SetActive(true);
        BattleVisualizer_Client.Instance.StartVisualization();
    }

    [ClientRpc]
    private void StopBattleVisualizationClientRpc()
    {
        BattleVisualizer_Client.Instance.StartVisualization();
    }

    [ClientRpc]
    private void UpdateGameStateClientRpc(byte newGameState)
    {
        currentGameState = (GameState)newGameState;
    }

    [ClientRpc]
    private void StartBattleMusicClientRpc()
    {
        BGMManager.Instance?.OnBattleStarted();
    }

    [ClientRpc]
    public void SendGameResultClientRpc(byte losingTeamId)
    {
        if (losingTeamId == myTeamId)
        {
            loseText.SetActive(true);
            Debug.Log($"[GameManager] You Lose!");
            BGMManager.Instance?.PlayLoseBGM();
        }
        else
        {
            winText.SetActive(true);
            Debug.Log($"[GameManager] You Win!");
            BGMManager.Instance?.PlayWinBGM();
        }
    }
    #endregion
}