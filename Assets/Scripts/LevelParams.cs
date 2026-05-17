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
        3_000,   3_000,                               // L1, L2
        4_000,   6_000,   18_000,                     // L3, L4, L5 (boss)
        9_000,   12_000,  15_000,  20_000,  60_000,   // L6-L9, L10 (boss)
        28_000,  36_000,  47_000,  62_000,  180_000,  // L11-L14, L15 (boss)
        85_000,  110_000, 145_000, 190_000, 480_000,  // L16-L19, L20 (boss)
    };

    // L21-50: anchor at L19 goal (190k), grow 4% per level.
    // L51+:   anchor at L50 regular goal, grow 2.5% per level.
    // Boss levels (every 5th): ×2.5, timer +15s.
    private const float L19Anchor   = 190_000f;
    private const float EarlyRate   = 1.04f;    // L21-50
    private const float LateRate    = 1.025f;   // L51+
    private const float BossMulti   = 2.5f;
    private const float BossTimeBonus = 15f;

    // Pre-computed so the L50→L51 boundary is seamless.
    private static readonly float L50RegularGoal =
        L19Anchor * Mathf.Pow(EarlyRate, 50 - 19);

    // Classic has no skills so its score potential is much lower; scale goals down accordingly.
    private const float ClassicScaleFactor = 0.55f;

    public LevelParams(int level, GameMode mode = GameMode.Adventure)
    {
        levelNumber = level;

        if (level <= _timeLimits.Length)
        {
            timeLimit = _timeLimits[level - 1];
            scoreGoal = mode == GameMode.Classic
                ? Mathf.RoundToInt(_scoreGoals[level - 1] * ClassicScaleFactor)
                : _scoreGoals[level - 1];
            return;
        }

        bool isBoss = level % 5 == 0;

        float regularGoal;
        float baseTime;

        if (level <= 50)
        {
            regularGoal = L19Anchor * Mathf.Pow(EarlyRate, level - 19);
            baseTime    = 44f;
        }
        else
        {
            regularGoal = L50RegularGoal * Mathf.Pow(LateRate, level - 50);
            baseTime    = 40f;
        }

        float goal = isBoss ? regularGoal * BossMulti : regularGoal;
        if (mode == GameMode.Classic) goal *= ClassicScaleFactor;

        timeLimit = isBoss ? baseTime + BossTimeBonus : baseTime;
        scoreGoal = Mathf.RoundToInt(goal);
    }
}
