using NUnit.Framework;
using UnityEngine;

public class PlayerInputTests
{
    private GameObject testObject;
    private PlayerInput playerInput;

    [SetUp]
    public void SetUp()
    {
        testObject = new GameObject("TestLaneGrid");
        // Give it a known scale so position math is predictable
        testObject.transform.localScale = new Vector3(0.07f, 0.07f, 1f);
        playerInput = testObject.AddComponent<PlayerInput>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void StartsOnLane7()
    {
        Assert.AreEqual(7, playerInput.currentLane);
    }

    [Test]
    public void MovingToDifferentLane_UpdatesCurrentLane()
    {
        playerInput.OnLaneButtonPressed(3);
        Assert.AreEqual(3, playerInput.currentLane);

        playerInput.OnLaneButtonPressed(9);
        Assert.AreEqual(9, playerInput.currentLane);
    }

    [Test]
    public void PressingCurrentLane_DoesNotChangeLane()
    {
        // Starts at 7
        playerInput.OnLaneButtonPressed(7);
        Assert.AreEqual(7, playerInput.currentLane);

        // Move somewhere else, then press it again
        playerInput.OnLaneButtonPressed(2);
        playerInput.OnLaneButtonPressed(2);
        Assert.AreEqual(2, playerInput.currentLane);
    }

    [Test]
    public void InvalidLaneNumbers_AreIgnored()
    {
        int before = playerInput.currentLane;

        playerInput.OnLaneButtonPressed(0);
        playerInput.OnLaneButtonPressed(10);
        playerInput.OnLaneButtonPressed(-1);

        Assert.AreEqual(before, playerInput.currentLane);
    }

    [Test]
    public void MoveToLane_SetsExpectedLocalPosition()
    {
        // With scale 0.07:
        // cellWorldWidth  = 50 * 0.07 = 3.5
        // cellWorldHeight = 100 * 0.07 = 7.0

        // Lane 5 (center) → col 1, row 1 → x = 0, y = 0
        playerInput.OnLaneButtonPressed(5);
        Assert.AreEqual(0f, playerInput.transform.localPosition.x, 0.001f);
        Assert.AreEqual(0f, playerInput.transform.localPosition.y, 0.001f);

        // Lane 1 (top-left) → col 0, row 0 → x = +3.5, y = -7.0  (because of the -y)
        playerInput.OnLaneButtonPressed(1);
        Assert.AreEqual(3.5f, playerInput.transform.localPosition.x, 0.001f);
        Assert.AreEqual(-7.0f, playerInput.transform.localPosition.y, 0.001f);

        // Lane 9 (bottom-right) → col 2, row 2 → x = -3.5, y = +7.0
        playerInput.OnLaneButtonPressed(9);
        Assert.AreEqual(-3.5f, playerInput.transform.localPosition.x, 0.001f);
        Assert.AreEqual(7.0f, playerInput.transform.localPosition.y, 0.001f);
    }

    [Test]
    public void GoToSafeLane_MovesToSafeLane()
    {
        playerInput.safeLane = 4;
        playerInput.GoToSafeLane();
        Assert.AreEqual(4, playerInput.currentLane);
    }

    [Test]
    public void ZPosition_IsNeverChanged()
    {
        float originalZ = playerInput.transform.localPosition.z;

        playerInput.OnLaneButtonPressed(1);
        playerInput.OnLaneButtonPressed(5);
        playerInput.OnLaneButtonPressed(9);

        Assert.AreEqual(originalZ, playerInput.transform.localPosition.z, 0.001f);
    }
}