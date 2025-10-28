using UnityEngine;

public static class CreateGridsClass
{
    public static void CreateGrids(
        GameObject gridPrefab,
        int gridSize,
        int gridLength,
        RectTransform parentRect,
        Grid[,] board,
        System.Func<bool> ExistsMatch,
        RectTransform gameAreaRect,
        RectTransform dragLayerRect
    )
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Creating a new Grid/orb
                GameObject gridGO = Object.Instantiate(gridPrefab);
                gridGO.name = $"Grid[{i}, {j}]";
                gridGO.transform.SetParent(parentRect);
                Vector2 pos = new Vector2(i * gridLength, j * gridLength * -1);
                gridGO.GetComponent<RectTransform>().anchoredPosition = pos;
                gridGO.transform.localScale = Vector3.one;
                Grid grid = gridGO.GetComponent<Grid>();
                board[i, j] = grid;

                // Prevent matches when creating initial grid
                while (ExistsMatch())
                {
                    grid.orb.ChangeSuitRandom();
                }

                Orb orb = grid.orb;
                if (orb != null)
                {
                    orb.SetRects(gameAreaRect, dragLayerRect);
                }
            }
        }
    }
}