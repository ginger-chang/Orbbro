using UnityEngine;

public class LevelParams
{
    public int   levelNumber;
    public float timeLimit;
    public int   scoreGoal;

    private static readonly float[] _timeLimits = {
        10f,  10f,                          // L1, L2  (testing)
        60f,  60f,  75f,                    // L3, L4, L5 (boss)
        55f,  55f,  55f,  55f,  70f,        // L6-L9, L10 (boss)
        50f,  50f,  50f,  50f,  65f,        // L11-L14, L15 (boss)
        47f,  47f,  47f,  47f,  62f,        // L16-L19, L20 (boss)
    };

    private static readonly int[] _scoreGoals = {
        3_000,   3_000,                              // L1, L2
        7_000,   10_000,  40_000,                    // L3, L4, L5 (boss)
        20_000,  26_000,  34_000,  45_000,  200_000, // L6-L9, L10 (boss)
        75_000,  100_000, 130_000, 170_000, 800_000, // L11-L14, L15 (boss)
        300_000, 390_000, 510_000, 660_000, 3_000_000, // L16-L19, L20 (boss)
    };

    public LevelParams(int level)
    {
        levelNumber = level;

        if (level <= _timeLimits.Length)
        {
            timeLimit = _timeLimits[level - 1];
            scoreGoal = _scoreGoals[level - 1];
            return;
        }

        // L21+: base goal scales 25% per level from the L19 anchor (660,000).
        // Boss levels (every 5th) get ×3.5 and a longer timer.
        bool isBoss = level % 5 == 0;
        float regularGoal = 660_000f * Mathf.Pow(1.25f, level - 19);

        timeLimit = isBoss ? 59f : 44f;
        scoreGoal = isBoss
            ? Mathf.RoundToInt(regularGoal * 3.5f)
            : Mathf.RoundToInt(regularGoal);
    }
}
