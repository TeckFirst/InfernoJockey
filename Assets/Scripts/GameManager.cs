using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, GameOver }

    // Properties matching the test expectations
    public GameState CurrentState { get; private set; } = GameState.Menu;
    public float DistanceScore { get; private set; } = 0f;
    public int KytheriumCollected { get; private set; } = 0;

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            // Increment distance score over time during gameplay
            DistanceScore += Time.deltaTime * 10f;
        }
    }

    public void StartGame()
    {
        DistanceScore = 0f;
        KytheriumCollected = 0;
        CurrentState = GameState.Playing;
    }

    public void TriggerCrash()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.GameOver;
        }
    }

    public void AddKytherium(int amount)
    {
        KytheriumCollected += amount;
    }

#if UNITY_EDITOR
    // Allows unit tests to clear the singleton instance between test runs
    public static void ResetForTesting()
    {
        Instance = null;
    }
#endif
}