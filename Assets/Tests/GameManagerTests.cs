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
        Object.Destroy(gameManagerGameObject);
        GameManager.ResetForTesting();
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

        // Let Unity process one frame so Update() and Time.deltaTime kick in cleanly
        yield return null;

        float initialScore = gameManager.DistanceScore;

        // Now wait for half a second
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