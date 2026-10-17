using NUnit.Framework;

public class CameraLaneMoverTests
{
    [Test]
    public void CenterLane_IsAdjacentToTop()
    {
        Assert.IsTrue(
            CameraLaneMover.IsAdjacent(5, 2));
    }

    [Test]
    public void CenterLane_IsAdjacentToBottom()
    {
        Assert.IsTrue(
            CameraLaneMover.IsAdjacent(5, 8));
    }

    [Test]
    public void CenterLane_IsAdjacentToLeft()
    {
        Assert.IsTrue(
            CameraLaneMover.IsAdjacent(5, 4));
    }

    [Test]
    public void CenterLane_IsAdjacentToRight()
    {
        Assert.IsTrue(
            CameraLaneMover.IsAdjacent(5, 6));
    }

    [Test]
    public void CenterLane_IsAdjacentToDiagonal()
    {
        Assert.IsTrue(
            CameraLaneMover.IsAdjacent(5, 1));
    }

    [Test]
    public void OppositeCorners_AreNotAdjacent()
    {
        Assert.IsFalse(
            CameraLaneMover.IsAdjacent(1, 9));
    }

    [Test]
    public void SameLane_IsNotAdjacent()
    {
        Assert.IsFalse(
            CameraLaneMover.IsAdjacent(5, 5));
    }

    [Test]
    public void InvalidLane_IsRejected()
    {
        Assert.IsFalse(
            CameraLaneMover.IsAdjacent(0, 5));
    }

    [Test]
    public void InvalidDestination_IsRejected()
    {
        Assert.IsFalse(
            CameraLaneMover.IsAdjacent(5, 10));
    }
}