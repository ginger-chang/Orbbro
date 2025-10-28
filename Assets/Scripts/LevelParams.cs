using UnityEngine;

public class LevelParams
{
    public int levelNumber;
    public float timeLimit;
    public int scoreGoal;

    private float[] timeList = new float[] {10f, 10f, 60f, 60f, 58f};
    private int[] scoreList = new int[] {3000, 3000, 10000, 12000, 13000}; // steps
    private int listSize = 5;

    public LevelParams(int level)
    {
        levelNumber = level;
        if (level <= listSize)
        {
            timeLimit = timeList[level - 1];
            scoreGoal = scoreList[level - 1];
        } else if (level <= 10)
        {
            timeLimit = 50f;
            scoreGoal = 8000;
        } else if (level <= 15)
        {
            timeLimit = 45f;
            scoreGoal = 10000;
        } else if (level <= 25)
        {
            timeLimit = 40f;
            scoreGoal = 10000;
            if (level % 5 == 0)
            {
                timeLimit = 100f;
                scoreGoal = 50000;
            }
        } else if (level <= 50)
        {
            timeLimit = 35f;
            scoreGoal = 12000;
        } else
        {
            timeLimit = 30f;
            scoreGoal = 12000;
            if (level % 5 == 0)
            {
                timeLimit = 120f;
                scoreGoal = 80000;
            }
        }
    }
}
