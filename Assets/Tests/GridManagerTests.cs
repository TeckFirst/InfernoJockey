using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class GridManagerTests
{
    private GridManager gridManager;

    [SetUp]
    public void Setup()
    {
        // Create a temporary GameObject to host the GridManager for testing
        GameObject go = new GameObject("GridManagerTestObject");
        gridManager = go.AddComponent<GridManager>();
        gridManager.InitializeGrid();
    }

    [TearDown]
    public void TearDown()
    {
        if (gridManager != null)
        {
            Object.DestroyImmediate(gridManager.gameObject);
        }
    }

    [Test]
    public void GridInitialization_VerifiesAllLanesAndCenter()
    {
        // Test that lanes 1 through 9 are correctly initialized
        for (int i = 1; i <= 9; i++)
        {
            Assert.IsTrue(gridManager.HasLane(i), $"Lane {i} should be initialized on the grid.");
        }

        // Verify Lane 5 is positioned at the center (0,0)
        Vector2 lane5Position = gridManager.GetLanePosition(5);
        Assert.AreEqual(Vector2.zero, lane5Position, "Lane 5 should act as the center coordinate (0,0).");
    }

    [Test]
    public void AdjacencyLogic_Lane5To6_IsDirectlyAdjacent()
    {
        // Verify that moving from Lane 5 to Lane 6 registers as a direct adjacent move
        bool isAdjacent = gridManager.IsAdjacent(5, 6);
        Assert.IsTrue(isAdjacent, "Moving from Lane 5 to Lane 6 should be a direct adjacent move.");
    }

    [Test]
    public void AdjacencyLogic_Lane9To1_IsNonAdjacent()
    {
        // Verify that moving diagonally/across the 3x3 grid from Lane 9 to Lane 1 is non-adjacent
        bool isAdjacent = gridManager.IsAdjacent(9, 1);
        Assert.IsFalse(isAdjacent, "Moving from Lane 9 to Lane 1 should register as non-adjacent, requiring a multi-step path.");
    }
}