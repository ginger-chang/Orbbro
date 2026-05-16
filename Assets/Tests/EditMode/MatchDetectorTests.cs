using NUnit.Framework;

public class MatchDetectorTests
{
    private const int Size = 6;
    private MatchDetector _detector;

    [SetUp]
    public void SetUp()
    {
        _detector = new MatchDetector(Size);
    }

    // Helpers
    private static Orb.Suits[,] EmptyBoard() => new Orb.Suits[Size, Size]; // all Orb.Suits.none

    private static Orb.Suits[,] CheckerBoard()
    {
        var s = new Orb.Suits[Size, Size];
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                s[i, j] = (i + j) % 2 == 0 ? Orb.Suits.red : Orb.Suits.blue;
        return s;
    }

    // --- ExistsMatch ---

    [Test]
    public void AllNone_NoMatch()
    {
        Assert.IsFalse(_detector.ExistsMatch(EmptyBoard()));
        Assert.AreEqual(0, _detector.NumMatches);
    }

    [Test]
    public void CheckerBoard_NoMatch()
    {
        Assert.IsFalse(_detector.ExistsMatch(CheckerBoard()));
        Assert.AreEqual(0, _detector.NumMatches);
    }

    [Test]
    public void TwoInARow_NoMatch()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        Assert.IsFalse(_detector.ExistsMatch(s));
    }

    [Test]
    public void HorizontalThree_IsMatch()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        s[2, 0] = Orb.Suits.red;
        Assert.IsTrue(_detector.ExistsMatch(s));
    }

    [Test]
    public void VerticalThree_IsMatch()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.blue;
        s[0, 1] = Orb.Suits.blue;
        s[0, 2] = Orb.Suits.blue;
        Assert.IsTrue(_detector.ExistsMatch(s));
    }

    // --- NumMatches ---

    [Test]
    public void HorizontalThree_OneMatch()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        s[2, 0] = Orb.Suits.red;
        _detector.UpdateMatches(s);
        Assert.AreEqual(1, _detector.NumMatches);
    }

    [Test]
    public void TwoSeparateMatches_TwoMatchCount()
    {
        var s = EmptyBoard();
        // horizontal match in top row
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        s[2, 0] = Orb.Suits.red;
        // vertical match far away
        s[5, 3] = Orb.Suits.blue;
        s[5, 4] = Orb.Suits.blue;
        s[5, 5] = Orb.Suits.blue;
        _detector.UpdateMatches(s);
        Assert.AreEqual(2, _detector.NumMatches);
    }

    [Test]
    public void FourInARow_SingleMatchGroup()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.green;
        s[1, 0] = Orb.Suits.green;
        s[2, 0] = Orb.Suits.green;
        s[3, 0] = Orb.Suits.green;
        _detector.UpdateMatches(s);
        Assert.AreEqual(1, _detector.NumMatches);
    }

    // --- Matches array ---

    [Test]
    public void HorizontalThree_CorrectCellsMarked()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        s[2, 0] = Orb.Suits.red;
        _detector.UpdateMatches(s);
        int id = _detector.Matches[0, 0];
        Assert.AreNotEqual(0, id);
        Assert.AreEqual(id, _detector.Matches[1, 0]);
        Assert.AreEqual(id, _detector.Matches[2, 0]);
    }

    [Test]
    public void VerticalThree_CorrectCellsMarked()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.blue;
        s[0, 1] = Orb.Suits.blue;
        s[0, 2] = Orb.Suits.blue;
        _detector.UpdateMatches(s);
        int id = _detector.Matches[0, 0];
        Assert.AreNotEqual(0, id);
        Assert.AreEqual(id, _detector.Matches[0, 1]);
        Assert.AreEqual(id, _detector.Matches[0, 2]);
    }

    [Test]
    public void NonMatchCells_RemainZero()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        s[2, 0] = Orb.Suits.red;
        _detector.UpdateMatches(s);
        Assert.AreEqual(0, _detector.Matches[3, 0]);
        Assert.AreEqual(0, _detector.Matches[0, 1]);
        Assert.AreEqual(0, _detector.Matches[5, 5]);
    }

    [Test]
    public void TwoSeparateMatches_HaveDistinctIds()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        s[2, 0] = Orb.Suits.red;
        s[5, 3] = Orb.Suits.blue;
        s[5, 4] = Orb.Suits.blue;
        s[5, 5] = Orb.Suits.blue;
        _detector.UpdateMatches(s);
        int firstId  = _detector.Matches[0, 0];
        int secondId = _detector.Matches[5, 3];
        Assert.AreNotEqual(0, firstId);
        Assert.AreNotEqual(0, secondId);
        Assert.AreNotEqual(firstId, secondId);
        Assert.AreEqual(firstId,  _detector.Matches[1, 0]);
        Assert.AreEqual(firstId,  _detector.Matches[2, 0]);
        Assert.AreEqual(secondId, _detector.Matches[5, 4]);
        Assert.AreEqual(secondId, _detector.Matches[5, 5]);
    }

    [Test]
    public void LShapedMatch_AllCellsSameId()
    {
        var s = EmptyBoard();
        // vertical part: x=0, y=0..2
        s[0, 0] = Orb.Suits.yellow;
        s[0, 1] = Orb.Suits.yellow;
        s[0, 2] = Orb.Suits.yellow;
        // horizontal extension: y=2, x=1..2
        s[1, 2] = Orb.Suits.yellow;
        s[2, 2] = Orb.Suits.yellow;
        _detector.UpdateMatches(s);
        Assert.AreEqual(1, _detector.NumMatches);
        int id = _detector.Matches[0, 0];
        Assert.AreNotEqual(0, id);
        Assert.AreEqual(id, _detector.Matches[0, 1]);
        Assert.AreEqual(id, _detector.Matches[0, 2]);
        Assert.AreEqual(id, _detector.Matches[1, 2]);
        Assert.AreEqual(id, _detector.Matches[2, 2]);
    }

    [Test]
    public void UpdateMatchesTwice_SecondCallResets()
    {
        var s = EmptyBoard();
        s[0, 0] = Orb.Suits.red;
        s[1, 0] = Orb.Suits.red;
        s[2, 0] = Orb.Suits.red;
        _detector.UpdateMatches(s);
        Assert.AreEqual(1, _detector.NumMatches);

        // Second call with empty board — should reset
        _detector.UpdateMatches(EmptyBoard());
        Assert.AreEqual(0, _detector.NumMatches);
        foreach (int v in _detector.Matches)
            Assert.AreEqual(0, v);
    }
}
