using UnityEngine;
using System;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] public RectTransform gameAreaRect;
    [SerializeField] public RectTransform dragLayerRect;
    public float dropTime = 0.24f;

    private readonly int _gridSize = 6;
    private readonly int _gridLength = 167;
    private MatchDetector _matchDetector;

    public Grid[,] Board { get; private set; }
    public int GridSize => _gridSize;
    public int[,] Matches => _matchDetector.Matches;
    public int NumMatches => _matchDetector.NumMatches;

    public void Initialize()
    {
        _matchDetector = new MatchDetector(_gridSize);
        Board = new Grid[_gridSize, _gridSize];
        RectTransform rect = GetComponent<RectTransform>();
        CreateGridsClass.CreateGrids(gridPrefab, _gridSize, _gridLength, rect, Board, ExistsMatch, gameAreaRect, dragLayerRect);
    }

    public void ClearBoard()
    {
        if (Board == null) return;
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                Grid grid = Board[i, j];
                if (grid == null) continue;
                if (grid.orb != null) Destroy(grid.orb.gameObject);
                Destroy(grid.gameObject);
            }
        }
    }

    public bool ExistsMatch() => _matchDetector.ExistsMatch(Board);

    public void UpdateMatches() => _matchDetector.UpdateMatches(Board);

    // Destroys all orbs belonging to matchId, returns the count destroyed.
    public int DestroyMatchedOrbs(int matchId)
    {
        int count = 0;
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (Matches[i, j] == matchId)
                {
                    Destroy(Board[i, j].orb.gameObject);
                    count++;
                }
            }
        }
        return count;
    }

    public IEnumerator FillBoard()
    {
        DropExistingOrbs();
        yield return StartCoroutine(DropNewOrbs());
        yield return new WaitForSeconds(0.2f);
    }

    private void DropExistingOrbs()
    {
        for (int i = 0; i < _gridSize; i++)
        {
            int dropCount = 0;
            for (int j = _gridSize - 1; j >= 0; j--)
            {
                if (Board[i, j].orb == null)
                    dropCount++;
                else
                    Board[i, j + dropCount].assignOrb(Board[i, j].orb, "", dropCount);
            }
        }
    }

    private IEnumerator DropNewOrbs()
    {
        int[] countNeeded = new int[_gridSize];
        int maxCount = 0;
        for (int i = 0; i < _gridSize; i++)
        {
            int count = 0;
            for (int j = 0; j < _gridSize; j++)
                if (Matches[i, j] > 0) count++;
            countNeeded[i] = count;
            maxCount = Math.Max(maxCount, count);
        }

        for (int iter = 0; iter < maxCount; iter++)
        {
            for (int i = 0; i < _gridSize; i++)
            {
                if (countNeeded[i] >= 1)
                {
                    GameObject orbGO = Instantiate(orbPrefab);
                    Orb orb = orbGO.GetComponent<Orb>();
                    orb.SetRects(gameAreaRect, dragLayerRect);
                    Board[i, countNeeded[i] - 1].assignOrb(orb, "", countNeeded[i], true);
                    countNeeded[i]--;
                }
            }
            yield return new WaitForSeconds(dropTime);
        }
    }
}
