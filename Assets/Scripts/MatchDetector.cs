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

    public bool ExistsMatch(Grid[,] board)
    {
        UpdateMatches(board);
        foreach (int v in Matches)
            if (v != 0) return true;
        return false;
    }

    public void UpdateMatches(Grid[,] board)
    {
        bool matchExist = false;
        int matchId = 1;
        Matches = new int[_gridSize, _gridSize];
        bool[,] visited = new bool[_gridSize, _gridSize];
        var queue = new Queue<(int x, int y)>();
        ClearMatchesHint(board);

        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (board[i, j] == null) continue;
                Orb.Suits curSuit = board[i, j].orb.suit;
                queue.Clear();
                if (curSuit == Orb.Suits.none || visited[i, j]) continue;

                queue.Enqueue((i, j));
                while (queue.Count != 0)
                {
                    var (x, y) = queue.Dequeue();
                    if (visited[x, y]) continue;

                    int up    = CountInDirection(board, curSuit, x, y,  0, -1);
                    int down  = CountInDirection(board, curSuit, x, y,  0,  1);
                    int left  = CountInDirection(board, curSuit, x, y, -1,  0);
                    int right = CountInDirection(board, curSuit, x, y,  1,  0);

                    visited[x, y] = true;
                    (Matches, matchExist) = DrawMatches(board, Matches, ref queue, matchId, x, y, up, down, left, right);
                }
                if (matchExist)
                {
                    NumMatches = matchId;
                    matchId++;
                }
            }
        }
    }

    private (int[,], bool) DrawMatches(Grid[,] board, int[,] matches, ref Queue<(int x, int y)> queue, int id, int x, int y, int up, int down, int left, int right)
    {
        bool match = false;
        if (up + down >= 2)
        {
            match = true;
            matches[x, y] = id;
            for (int k = 1; k <= up;    k++) { matches[x, y - k] = id; queue.Enqueue((x, y - k)); board[x, y - k].setMatch(id); }
            for (int k = 1; k <= down;  k++) { matches[x, y + k] = id; queue.Enqueue((x, y + k)); board[x, y + k].setMatch(id); }
        }
        if (left + right >= 2)
        {
            match = true;
            matches[x, y] = id;
            for (int k = 1; k <= left;  k++) { matches[x - k, y] = id; queue.Enqueue((x - k, y)); board[x - k, y].setMatch(id); }
            for (int k = 1; k <= right; k++) { matches[x + k, y] = id; queue.Enqueue((x + k, y)); board[x + k, y].setMatch(id); }
        }
        return (matches, match);
    }

    private int CountInDirection(Grid[,] board, Orb.Suits suit, int x, int y, int dx, int dy)
    {
        int count = 0;
        x += dx; y += dy;
        while (x >= 0 && x < _gridSize && y >= 0 && y < _gridSize)
        {
            if (board[x, y] != null && board[x, y].orb.suit == suit) { count++; x += dx; y += dy; }
            else break;
        }
        return count;
    }

    private void ClearMatchesHint(Grid[,] board)
    {
        for (int i = 0; i < _gridSize; i++)
            for (int j = 0; j < _gridSize; j++)
                if (board[i, j] != null) board[i, j].setMatch(0);
    }
}
