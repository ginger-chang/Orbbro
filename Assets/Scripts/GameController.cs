using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameController : MonoBehaviour
{
    public GameObject gridPrefab;
    public GameObject orbPrefab;

    private RectTransform rect;
    public RectTransform gameAreaRect;
    public RectTransform dragLayerRect;

    private int gridSize = 6;
    private int gridLength = 167;

    private Grid[,] board; 
    private int numMatches = 0;

    public GameState state = GameState.MainMenu;
    public GameMode mode = GameMode.Classic;
    public float dropTime = 0.24f;

    private int[,] matches;

    private AudioManager audioManager;
    private UIManager uiManager;
    private GameModeManager gameModeManager;

    public enum GameState
    {
        PlayMode,
        Resolving,
        GameOver,
        Pause,
        Revive,
        MainMenu
    }

    public enum GameMode
    {
        Classic,
        Endless,
        Adventure
    }

    //--------------------STARTING------------------------

    public TextMeshProUGUI highScoreText;
    private int highScore;

    private void Awake()
    {
        this.audioManager = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();
        this.uiManager = GameObject.FindGameObjectWithTag("UI Manager").GetComponent<UIManager>();
        this.gameModeManager = GameObject.FindGameObjectWithTag("Game Mode Manager").GetComponent<GameModeManager>();
        Time.timeScale = 0f;
    }

    public void Start()
    {
        clearStuff();
        DOTween.KillAll();

        this.rect = GetComponent<RectTransform>();
        board = new Grid[gridSize, gridSize];
        CreateGrids();

        gameModeManager.setupGameMode();

        
        // resetting level and score.
        curLevel = 1;
        score = 0;
        curLevelScoreGoal = 0;
        scoreGoal = 0;
        scoreText.text = $"Score: {score}";
        setupLevel();
        comboText.gameObject.SetActive(false);

        revived = false;

        // (for adventure mode) resetting skills and multipliers
        comboMultiplier = 1.3f;
        iterMultiplier = 1.2f;
        baseMultiplier = 100;

    // load high score
    highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = $"High score: {highScore}";
    }

    void CreateGrids()
    {
        for (int i = 0; i < gridSize; i ++)
        {
            for(int j = 0; j < gridSize; j ++)
            {
                // Creating a new Grid/orb
                GameObject gridGO = Instantiate(gridPrefab);
                gridGO.name = $"Grid[{i}, {j}]";
                gridGO.transform.SetParent(this.rect);
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

    void Update()
    {
        if (state == GameState.PlayMode && gameModeManager.timerEnabled)
        {
            timeRemaining -= Time.deltaTime;
            timeRemaining = Mathf.Max(0f, timeRemaining);
            timerBar.value = timeRemaining / timeLimit;

            if (timeRemaining <= 0)
            {
                levelTimeUp();
            }
        } else if (state == GameState.Pause || state == GameState.MainMenu)
        {
            Time.timeScale = 0f;
        }
        // if in ui mode (such as pause and main menu and stuff, don't make time move)
    }

    //-----------------------RESTARTING-------------------------

    public void clearStuff()
    {
        if (board == null)
        {
            return;
        }
        // 1. clear all grids and orbs
        for (int i = 0; i < gridSize; i ++)
        {
            for(int j = 0; j < gridSize; j ++)
            {
                Grid grid = board[i, j];
                if (grid != null)
                {
                    Orb orb = grid.orb;
                    if (orb != null)
                    {
                        GameObject.Destroy(orb.gameObject);
                    }
                    GameObject.Destroy(grid.gameObject);
                }
            }
        }
    }

    public void killCoroutines()
    {
        StopAllCoroutines();
    }

    //----------------------PUBLIC (UI)-------------------------

    public void setGameState(GameState state)
    {
        this.state = state;
    }

    public void setGameMode(GameMode mode)
    {
        this.mode = mode;
    }

    //----------------------LEVELS--------------------------

    public int curLevel;
    private int curLevelScoreGoal = 0;
    public int scoreGoal = 0;
    private float timeLimit = 0f;
    private float timeRemaining = 0f;

    public Slider timerBar;
    public TextMeshProUGUI levelText;
    public RectMask2D scoreBarMask;

    private void setupLevel()
    {
        LevelParams levelParams = new LevelParams(curLevel);
        curLevelScoreGoal = levelParams.scoreGoal;
        scoreGoal += levelParams.scoreGoal;
        timeLimit = levelParams.timeLimit;
        timeRemaining = timeLimit;

        // set up ui
        timerBar.value = 1;
        levelText.text = $"Level {curLevel}";

        if (curLevel == 1)
        {
            var scoreBarMaskPadding = scoreBarMask.padding;
            scoreBarMaskPadding.w = 1000;
            scoreBarMask.padding = scoreBarMaskPadding;
        }
    }

    private void levelGoalReached()
    {
        curLevel++;
        setupLevel();
    }

    //---------------------GAME OVER & REVIVAL-----------------------

    private Orb orbInSwap;
    private bool revived = false;

    public void setOrbInSwap(Orb orb)
    {
        orbInSwap = orb;
    }

    private void levelTimeUp()
    {
        // TODO: Forcibly stop swapping, then resolve again, then if level isn't reached then game over.
        if (orbInSwap != null)
        {
            orbInSwap.curGrid.assignOrb(orbInSwap, "just go there");
            orbInSwap.validDrag = false;
            Resolve();
        } else
        {
            // TODO: revival chance
            if (!revived)
            {
                uiManager.activateRevivePopup();
                this.state = GameState.Revive;
                revived = true;
            } else
            {
                Debug.Log("Game over!!!");
                this.state = GameState.GameOver;
                uiManager.GameOver();
            }
        }
    }

    public void Revive()
    {
        Debug.Log("revive in game controller!");
        timeRemaining = timeLimit;
        this.state = GameState.PlayMode;
        revived = true;
    }


    //--------------------RESOLVING-------------------------

    // Called by drop orb, changes game state to resolving
    public void Resolve()
    {
        this.state = GameState.Resolving;
        // set score parameters
        curScore = 0;
        curCombo = 0;
        curIter = 0;
        StartCoroutine(ResolveEnumerator());
    }

    public IEnumerator ResolveEnumerator()
    {
        yield return new WaitForSeconds(0.1f);
        while (ExistsMatch())
        {
            yield return StartCoroutine(DisappearAllMatches());
        }
        comboText.gameObject.SetActive(false);
        this.state = GameState.PlayMode;
    }

    // Make all matches disappear. (using destroy) will also do score calculations (to be coded)
    public IEnumerator DisappearAllMatches()
    {
        // update score parameters
        curIter++;

        UpdateMatches();
        for(int matchId = 1; matchId <= numMatches; matchId ++)
        {
            // for every match
            curCombo++;
            int numOrb = 0;
            for(int i = 0; i < gridSize; i ++)
            {
                for(int j = 0; j < gridSize; j ++)
                {
                    if (matches[i, j] == matchId)
                    {
                        // for every orb in a match
                        Destroy(board[i, j].orb.gameObject);
                        numOrb++;
                    }
                }
            }
            this.audioManager.PlayDisappearSFX();
            updateScore(numOrb);
            yield return new WaitForSeconds(0.5f);
        }
        yield return StartCoroutine(FillBoard());
    }

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    private int score = 0;
    private int curScore = 0;

    public float comboMultiplier = 1.3f;
    public float iterMultiplier = 1.2f;
    public int baseMultiplier = 100;
    private int curCombo = 0;
    private int curIter = 0;

    void updateScore(int numOrb)
    {
        float s = curScore * comboMultiplier +
                  numOrb * baseMultiplier * Mathf.Pow(comboMultiplier, curCombo - 1) * Mathf.Pow(iterMultiplier, curIter - 1);
        int newCurScore = Mathf.FloorToInt(s);
        int dif = newCurScore - curScore;
        curScore = newCurScore;
        score += dif;
        scoreText.text = $"Score: {score}";

        comboText.gameObject.SetActive(true);
        comboText.text = $"Combo x{curCombo}";        

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
            highScoreText.text = $"High score: {highScore}";
        }

        var padding = scoreBarMask.padding;
        if (score >= scoreGoal)
        {
            levelGoalReached();

            int numerator = scoreGoal - score;
            int denominator = curLevelScoreGoal;
            float scoreBarOffset = 1000 * ((float)numerator / (float)denominator);
            var seq = DOTween.Sequence();
            seq.Append(DOTween.To(
            () => padding.w,
            x =>
            {
                padding.w = x;
                scoreBarMask.padding = padding;
            },
            0f,
            0.25f
            ));
            seq.AppendCallback(() => {
                padding.w = 1000f;
                scoreBarMask.padding = padding;
            });
            seq.Append(DOTween.To(
            () => padding.w,
            x =>
            {
                padding.w = x;
                scoreBarMask.padding = padding;
            },
            scoreBarOffset,
            0.25f
            ));
        } else
        {
            // update score bar ui
            int numerator = scoreGoal - score;
            int denominator = curLevelScoreGoal;
            float scoreBarOffset = 1000 * ((float)numerator / (float)denominator);

            
            //padding.w = scoreBarOffset;
            //scoreBarMask.padding = padding;

            DOTween.To(
            () => padding.w,
            x =>
            {
                padding.w = x;
                scoreBarMask.padding = padding;
            },
            scoreBarOffset,
            0.5f
            );
        }
    }

    // Fills the board, calls drop new orbs and drop existing orbs
    public IEnumerator FillBoard()
    {
        DropExistingOrbs();

        yield return StartCoroutine(DropNewOrbs());
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator DropNewOrbs()
    {
        // TODO: extract this part maybe!?
        int[] countNewOrbsNeeded = new int[gridSize];
        int maxCount = 0;
        for (int i = 0; i < gridSize; i++)
        {
            int count = 0;
            for (int j = 0; j < gridSize; j++)
            {
                if (matches[i, j] > 0)
                {
                    count++;
                }
            }
            countNewOrbsNeeded[i] = count;
            maxCount = Math.Max(maxCount, count);
        }

        for(int iter = 0; iter < maxCount; iter ++)
        {
            for(int i = 0; i < gridSize; i ++)
            {
                if (countNewOrbsNeeded[i] >= 1)
                {
                    GameObject orbGO = Instantiate(orbPrefab);
                    Orb orb = orbGO.GetComponent<Orb>();
                    orb.SetRects(gameAreaRect, dragLayerRect);
                    // assign orb to a grid
                    board[i, countNewOrbsNeeded[i] - 1].assignOrb(orb, "", countNewOrbsNeeded[i], true);
                    countNewOrbsNeeded[i]--;
                }
            }
            yield return new WaitForSeconds(dropTime);
        }
    }

    void DropExistingOrbs()
    {
        for(int i = 0; i < gridSize; i ++)
        {
            int dropCount = 0;
            for(int j = gridSize - 1; j >= 0; j --) // from bottom to top
            {
                if (board[i, j].orb == null)
                {
                    dropCount++;
                } else
                {
                    board[i, j + dropCount].assignOrb(board[i, j].orb, "", dropCount);
                }
            }
        }
    }



    // Detect Match, returns true if there is match
    public bool ExistsMatch()
    {
        UpdateMatches();
        foreach (int i in matches)
        {
            if (i != 0)
            {
                return true;
            }
        }
        return false;
    }
    
    // Detect Match, reads the current board and returns a 2d array mask of the matches
    public void UpdateMatches()
    {
        bool matchExist = false;
        int matchId = 1;
        matches = new int[gridSize, gridSize];
        bool[,] visited = new bool[gridSize, gridSize];
        Queue<(int x, int y)> queue = new Queue<(int, int)>();
        clearMatchesHint();
        // get 下＆右
        for (int i = 0; i < gridSize; i ++)
        {
            for(int j = 0; j < gridSize; j ++)
            {
                if (board[i, j] == null)
                {
                    continue;
                }
                Orb orb = board[i, j].orb;
                Orb.Suits curSuit = orb.suit;
                queue.Clear();
                if (curSuit == Orb.Suits.none || visited[i, j])
                {
                    continue;
                } else
                {
                    queue.Enqueue((i, j));
                    while(queue.Count != 0)
                    {
                        var pos = queue.Dequeue();
                        int x = pos.x;
                        int y = pos.y;

                        if (visited[x, y])
                        {
                            continue;
                        }
                        int up = findMatchUp(curSuit, x, y);
                        int down = findMatchDown(curSuit, x, y);
                        int left = findMatchLeft(curSuit, x, y);
                        int right = findMatchRight(curSuit, x, y);

                        visited[x, y] = true;
                        // draw matches
                        (matches, matchExist) = drawMatches(matches, ref queue, matchId, x, y, up, down, left, right);
                    }
                    if (matchExist)
                    {
                        this.numMatches = matchId;
                        matchId++;
                    }
                }

            }
        }
    }



    // Update match helpers

    // Draw the matches to the match board (coming from a single grid)
    (int[,], bool) drawMatches(int[,] matches, ref Queue<(int x, int y)> queue, int id, int x, int y, int up, int down, int left, int right)
    {
        bool match = false;
        if (up + down >= 2)
        {
            match = true;
            matches[x, y] = id;
            // up
            for (int k = 1; k <= up; k++)
            {
                matches[x, y - k] = id;
                queue.Enqueue((x, y - k));
                board[x, y - k].setMatch(id);
            }
            // down
            for (int k = 1; k <= down; k++)
            {
                matches[x, y + k] = id;
                queue.Enqueue((x, y + k));
                board[x, y + k].setMatch(id);
            }
        }
        if (left + right >= 2)
        {
            match = true;
            matches[x, y] = id;
            // left
            for (int k = 1; k <= left; k ++)
            {
                matches[x - k, y] = id;
                queue.Enqueue((x - k, y));
                board[x - k, y].setMatch(id);
            }
            // right
            for (int k = 1; k <= right; k++)
            {
                matches[x + k, y] = id;
                queue.Enqueue((x + k, y));
                board[x + k, y].setMatch(id);
            }
        }
        return (matches, match);
    }

    int findMatchUp(Orb.Suits suit, int i, int j)
    {
        int x = i;
        int y = j - 1;
        int rtn = 0;
        while(y >= 0)
        {
            if (board[x, y] != null && board[x, y].orb.suit == suit)
            {
                rtn++;
                y--;
            } else
            {
                break;
            }
        }
        return rtn;
    }

    int findMatchDown(Orb.Suits suit, int i, int j)
    {
        int x = i;
        int y = j + 1;
        int rtn = 0;
        while (y < gridSize)
        {
            if (board[x, y] != null && board[x, y].orb.suit == suit)
            {
                rtn++;
                y++;
            }
            else
            {
                break;
            }
        }
        return rtn;
    }

    int findMatchLeft(Orb.Suits suit, int i, int j)
    {
        int x = i - 1;
        int y = j;
        int rtn = 0;
        while (x >= 0)
        {
            if (board[x, y] != null && board[x, y].orb.suit == suit)
            {
                rtn++;
                x--;
            }
            else
            {
                break;
            }
        }
        return rtn;
    }

    int findMatchRight(Orb.Suits suit, int i, int j)
    {
        int x = i + 1;
        int y = j;
        int rtn = 0;
        while (x < gridSize)
        {
            if (board[x, y] != null && board[x, y].orb.suit == suit)
            {
                rtn++;
                x++;
            }
            else
            {
                break;
            }
        }
        return rtn;
    }


    

    //----------------------DEBUG--------------------------

    // DEBUG HELPERS, not used in game
    void printArray(int[] arr)
    {
        string line = "";
        for (int x = 0; x < arr.GetLength(0); x++)
        {
            line += arr[x].ToString().PadLeft(2) + " ";
        }
        Debug.Log(line);
    }

    void print2dIntArray(int[,] arr)
    {
        Debug.Log("print array: ");
        int width = arr.GetLength(0);
        int height = arr.GetLength(1);

        for (int y = 0; y < height; y++) // from top row to bottom row
        {
            string line = "";
            for (int x = 0; x < width; x++)
            {
                line += arr[x, y].ToString().PadLeft(2) + " ";
            }
            Debug.Log(line);
        }
    }

    void clearMatchesHint()
    {
        for(int i = 0; i < gridSize; i ++)
        {
            for (int j = 0; j < gridSize; j ++)
            {
                if (board[i, j] != null) { board[i, j].setMatch(0); }
            }
        }
    }

    void printOrbBoard()
    {
        for (int j = 0; j < gridSize; j++)
        {
            string line = "";
            for (int i = 0; i < gridSize; i++)
            {
                if (board[i, j].orb == null)
                {
                    line += "NULL ";
                } else
                {
                    line += board[i, j].orb.name.PadLeft(2) + " ";
                }
            }
            Debug.Log(line);
        }
    }
}
