using NUnit.Framework;

public class CameraLaneMoverTests
{
    [Test]
    public void CenterLane_HasAdjacentTopLane()
    {
        Assert.IsTrue(CameraLaneMover.IsAdjacent(5, 2));
    }

    [Test]
    public void CenterLane_HasAdjacentDiagonalLane()
    {
        Assert.IsTrue(CameraLaneMover.IsAdjacent(5, 1));
    }

    [Test]
    public void CenterLane_HasAdjacentRightLane()
    {
        Assert.IsTrue(CameraLaneMover.IsAdjacent(5, 6));
    }

    [Test]
    public void OppositeCorners_AreNotAdjacent()
    {
        Assert.IsFalse(CameraLaneMover.IsAdjacent(1, 9));
    }

    [Test]
    public void EdgeToFarEdge_IsNotAdjacent()
    {
        Assert.IsFalse(CameraLaneMover.IsAdjacent(2, 8));
    }

    [Test]
    public void SameLane_IsNotMovementAdjacent()
    {
        Assert.IsFalse(CameraLaneMover.IsAdjacent(5, 5));
    }

    [Test]
    public void InvalidLowLane_IsRejected()
    {
        Assert.IsFalse(CameraLaneMover.IsAdjacent(0, 1));
    }

    [Test]
    public void InvalidHighLane_IsRejected()
    {
        Assert.IsFalse(CameraLaneMover.IsAdjacent(1, 10));
    }
}