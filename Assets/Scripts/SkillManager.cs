using System;
using UnityEngine;

public class SkillManager
{
    public float ComboMultiplierBonus  { get; private set; }
    public int   BaseMultiplierBonus   { get; private set; }
    public int[] OrbColorWeights       { get; private set; } // index 0-5 maps to Suits red-pink
    public int   ExtraLives            { get; private set; }
    public float MatchFiveBonusMultiplier { get; private set; }
    public float[] ColorClearMultipliers  { get; private set; } // index 0-5 maps to Suits red-pink
    public int   StartingCombo         { get; private set; }

    public SkillManager() => Reset();

    public void Reset()
    {
        ComboMultiplierBonus      = 0f;
        BaseMultiplierBonus       = 0;
        OrbColorWeights           = new int[] { 1, 1, 1, 1, 1, 1 };
        ExtraLives                = 0;
        MatchFiveBonusMultiplier  = 0f;
        ColorClearMultipliers     = new float[] { 0f, 0f, 0f, 0f, 0f, 0f };
        StartingCombo             = 0;
    }

    public void Apply(SkillType skill)
    {
        switch (skill)
        {
            case SkillType.ComboMultiplier:
                ComboMultiplierBonus += 0.1f;
                break;
            case SkillType.BaseMultiplier:
                BaseMultiplierBonus += 25;
                break;
            case SkillType.OrbColorBias:
                OrbColorWeights[UnityEngine.Random.Range(0, 6)] += 1;
                break;
            case SkillType.ExtraLife:
                ExtraLives++;
                break;
            case SkillType.MatchFiveBonus:
                MatchFiveBonusMultiplier += 0.5f;
                break;
            case SkillType.ColorClearBonus:
                ColorClearMultipliers[UnityEngine.Random.Range(0, 6)] += 0.5f;
                break;
            case SkillType.StartingCombo:
                StartingCombo++;
                break;
        }
    }

    public SkillType[] GetRandomSkills(int count)
    {
        var all = (SkillType[])Enum.GetValues(typeof(SkillType));
        var result = new SkillType[count];
        for (int i = 0; i < count; i++)
            result[i] = all[UnityEngine.Random.Range(0, all.Length)];
        return result;
    }

    public static string GetDisplayName(SkillType skill) => skill switch
    {
        SkillType.ComboMultiplier => "Combo Multiplier",
        SkillType.BaseMultiplier  => "Base Multiplier",
        SkillType.OrbColorBias    => "Orb Color Bias",
        SkillType.ExtraLife       => "Extra Life",
        SkillType.MatchFiveBonus  => "Match-5 Bonus",
        SkillType.ColorClearBonus => "Color Clear Bonus",
        SkillType.StartingCombo   => "Starting Combo",
        _                         => skill.ToString()
    };

    public static string GetDescription(SkillType skill) => skill switch
    {
        SkillType.ComboMultiplier => "+0.1 to combo multiplier",
        SkillType.BaseMultiplier  => "+25 to base point multiplier",
        SkillType.OrbColorBias    => "A random color spawns more often",
        SkillType.ExtraLife       => "+1 free revive this run",
        SkillType.MatchFiveBonus  => "+0.5× bonus when matching 5+ orbs",
        SkillType.ColorClearBonus => "A random color gives +0.5× bonus on clear",
        SkillType.StartingCombo   => "Every resolve starts with +1 combo",
        _                         => ""
    };
}
