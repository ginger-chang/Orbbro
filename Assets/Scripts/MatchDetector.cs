using System.Collections.Generic;

public class MatchDetector
{
    private readonly int _gridSize;

    public int[,] Matches { get; private set; }
    public int NumMatches { get; private set; }

    public MatchDetector(int gridSize)
    {
        _gridSize = gridSize;
    }

    public bool ExistsMatch(Orb.Suits[,] suits)
    {
        UpdateMatches(suits);
        foreach (int v in Matches)
            if (v != 0) return true;
        return false;
    }

    public void UpdateMatches(Orb.Suits[,] suits)
    {
        bool matchExist = false;
        int matchId = 1;
        NumMatches = 0;
        Matches = new int[_gridSize, _gridSize];
        bool[,] visited = new bool[_gridSize, _gridSize];
        var queue = new Queue<(int x, int y)>();

        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                Orb.Suits curSuit = suits[i, j];
                queue.Clear();
                if (curSuit == Orb.Suits.none || visited[i, j]) continue;

                queue.Enqueue((i, j));
                while (queue.Count != 0)
                {
                    var (x, y) = queue.Dequeue();
                    if (visited[x, y]) continue;

                    int up    = CountInDirection(suits, curSuit, x, y,  0, -1);
                    int down  = CountInDirection(suits, curSuit, x, y,  0,  1);
                    int left  = CountInDirection(suits, curSuit, x, y, -1,  0);
                    int right = CountInDirection(suits, curSuit, x, y,  1,  0);

                    visited[x, y] = true;
                    (Matches, matchExist) = DrawMatches(Matches, ref queue, matchId, x, y, up, down, left, right);
                }
                if (matchExist)
                {
                    NumMatches = matchId;
                    matchId++;
                    matchExist = false;
                }
            }
        }
    }

    private (int[,], bool) DrawMatches(int[,] matches, ref Queue<(int x, int y)> queue, int id, int x, int y, int up, int down, int left, int right)
    {
        bool match = false;
        if (up + down >= 2)
        {
            match = true;
            matches[x, y] = id;
            for (int k = 1; k <= up;   k++) { matches[x, y - k] = id; queue.Enqueue((x, y - k)); }
            for (int k = 1; k <= down; k++) { matches[x, y + k] = id; queue.Enqueue((x, y + k)); }
        }
        if (left + right >= 2)
        {
            match = true;
            matches[x, y] = id;
            for (int k = 1; k <= left;  k++) { matches[x - k, y] = id; queue.Enqueue((x - k, y)); }
            for (int k = 1; k <= right; k++) { matches[x + k, y] = id; queue.Enqueue((x + k, y)); }
        }
        return (matches, match);
    }

    private int CountInDirection(Orb.Suits[,] suits, Orb.Suits suit, int x, int y, int dx, int dy)
    {
        int count = 0;
        x += dx; y += dy;
        while (x >= 0 && x < _gridSize && y >= 0 && y < _gridSize)
        {
            if (suits[x, y] == suit) { count++; x += dx; y += dy; }
            else break;
        }
        return count;
    }
}
