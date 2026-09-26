using NUnit.Framework;
using System;

[TestFixture]
public class PlayerMovementTests
{
    private PlayerController _controller;

    [SetUp]
    public void SetUp()
    {
        _controller = new PlayerController();
        _controller.CurrentLane = 5; // Start in the middle lane
    }

    [Test]
    [TestCase(0.0f, 1)]
    [TestCase(50.0f, 5)]
    [TestCase(100.0f, 9)]
    public void MapScreenTap_MapsCoordinatesCorrectlyToLane(float tapXCoordinate, int expectedLane)
    {
        // Act
        int mappedLane = _controller.MapScreenTapToLane(tapXCoordinate, screenWidth: 100f);

        // Assert
        Assert.AreEqual(expectedLane, mappedLane);
    }

    [Test]
    [TestCase(0)]
    [TestCase(10)]
    [TestCase(-1)]
    public void SelectLane_RejectsIndexOutsideValidRange(int invalidLane)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _controller.MoveToLane(invalidLane));
    }

    [Test]
    public void MoveToLane_RejectsNonAdjacentDirectMove()
    {
        // Arrange
        _controller.CurrentLane = 9;

        // Act
        bool moveSuccessful = _controller.TryMoveToLane(1);

        // Assert
        Assert.IsFalse(moveSuccessful);
        Assert.AreEqual(9, _controller.CurrentLane);
    }
}

// Supporting Stub Implementation
public class PlayerController
{
    public int CurrentLane { get; set; } = 5;

    public int MapScreenTapToLane(float tapX, float screenWidth)
    {
        float normalized = Math.Clamp(tapX / screenWidth, 0f, 1f);
        // Map 0-1 to lanes 1 through 9
        int lane = (int)Math.Floor(normalized * 9) + 1;
        return Math.Clamp(lane, 1, 9);
    }

    public void MoveToLane(int targetLane)
    {
        if (targetLane < 1 || targetLane > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(targetLane), "Lane must be between 1 and 9.");
        }
        CurrentLane = targetLane;
    }

    public bool TryMoveToLane(int targetLane)
    {
        if (targetLane < 1 || targetLane > 9) return false;

        // Enforce adjacency rule (e.g., can only move +/- 1 lane at a time)
        if (Math.Abs(targetLane - CurrentLane) > 1)
        {
            return false;
        }

        CurrentLane = targetLane;
        return true;
    }
}