using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerTests
{
    private GameObject gameManagerGameObject;
    private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        // Create a fresh GameObject and attach the GameManager component before each test
        gameManagerGameObject = new GameObject();
        gameManager = gameManagerGameObject.AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the GameObject after each test to avoid memory leaks in the scene
        Object.Destroy(gameManagerGameObject);
    }

    [Test]
    public void GameManager_InitializesInMenuState()
    {
        Assert.AreEqual(GameManager.GameState.Menu, gameManager.CurrentState);
    }

    [Test]
    public void StartGame_TransitionsToPlayingState()
    {
        gameManager.StartGame();

        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentState);
    }

    [UnityTest]
    public IEnumerator DistanceScore_IncrementsOverTime()
    {
        gameManager.StartGame();
        float initialScore = gameManager.DistanceScore;

        // Wait for half a second to allow time-based score increments to process
        yield return new WaitForSeconds(0.5f);

        Assert.Greater(gameManager.DistanceScore, initialScore);
    }

    [Test]
    public void Crash_TriggersGameOverState()
    {
        gameManager.StartGame();

        gameManager.TriggerCrash();

        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentState);
    }
}