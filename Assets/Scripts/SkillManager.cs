using System;
using System.Collections.Generic;
using UnityEngine;

public struct SkillOffer
{
    public SkillType Type;
    public int ColorIndex; // 0-5 for color-based skills, -1 otherwise
}

public class SkillManager
{
    public float   ComboMultiplierBonus     { get; private set; }
    public int     BaseMultiplierBonus      { get; private set; }
    public int[]   OrbColorWeights          { get; private set; } // index 0-5 → Suits red-pink
    public int     ExtraLives               { get; private set; }
    public float   MatchFiveBonusMultiplier { get; private set; }
    public float[] ColorClearMultipliers    { get; private set; } // index 0-5 → Suits red-pink
    public int     StartingCombo            { get; private set; }

    private static readonly string[] ColorNames = { "Red", "Blue", "Green", "Yellow", "Purple", "Pink" };

    public SkillManager() => Reset();

    public void Reset()
    {
        ComboMultiplierBonus     = 0f;
        BaseMultiplierBonus      = 0;
        OrbColorWeights          = new int[]   { 1, 1, 1, 1, 1, 1 };
        ExtraLives               = 0;
        MatchFiveBonusMultiplier = 0f;
        ColorClearMultipliers    = new float[] { 0f, 0f, 0f, 0f, 0f, 0f };
        StartingCombo            = 0;
    }

    public void Apply(SkillOffer offer)
    {
        switch (offer.Type)
        {
            case SkillType.ComboMultiplier:
                ComboMultiplierBonus += 0.1f;
                break;
            case SkillType.BaseMultiplier:
                BaseMultiplierBonus += 25;
                break;
            case SkillType.OrbColorBias:
                OrbColorWeights[offer.ColorIndex] += 1;
                break;
            case SkillType.ExtraLife:
                ExtraLives++;
                break;
            case SkillType.MatchFiveBonus:
                MatchFiveBonusMultiplier += 0.5f;
                break;
            case SkillType.ColorClearBonus:
                ColorClearMultipliers[offer.ColorIndex] += 0.5f;
                break;
            case SkillType.StartingCombo:
                StartingCombo++;
                break;
        }
    }

    public SkillOffer[] GetRandomSkills(int count)
    {
        var all = (SkillType[])Enum.GetValues(typeof(SkillType));
        var result          = new SkillOffer[count];
        var usedTypes       = new HashSet<SkillType>();
        var usedBiasColors  = new HashSet<int>();
        var usedClearColors = new HashSet<int>();

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                var type = all[UnityEngine.Random.Range(0, all.Length)];
                int colorIndex = -1;

                if (type == SkillType.OrbColorBias)
                {
                    colorIndex = UnityEngine.Random.Range(0, 6);
                    if (usedBiasColors.Contains(colorIndex)) continue;
                    usedBiasColors.Add(colorIndex);
                }
                else if (type == SkillType.ColorClearBonus)
                {
                    colorIndex = UnityEngine.Random.Range(0, 6);
                    if (usedClearColors.Contains(colorIndex)) continue;
                    usedClearColors.Add(colorIndex);
                }
                else
                {
                    if (usedTypes.Contains(type)) continue;
                    usedTypes.Add(type);
                }

                result[i] = new SkillOffer { Type = type, ColorIndex = colorIndex };
                break;
            }
        }
        return result;
    }

    public static string GetDisplayName(SkillOffer offer) => offer.Type switch
    {
        SkillType.ComboMultiplier => "Combo Multiplier",
        SkillType.BaseMultiplier  => "Base Multiplier",
        SkillType.OrbColorBias    => $"{ColorNames[offer.ColorIndex]} Orb Bias",
        SkillType.ExtraLife       => "Extra Life",
        SkillType.MatchFiveBonus  => "Match-5 Bonus",
        SkillType.ColorClearBonus => $"{ColorNames[offer.ColorIndex]} Clear Bonus",
        SkillType.StartingCombo   => "Starting Combo",
        _                         => offer.Type.ToString()
    };

    public static string GetDescription(SkillOffer offer) => offer.Type switch
    {
        SkillType.ComboMultiplier => "+0.1 to combo multiplier",
        SkillType.BaseMultiplier  => "+25 to base point multiplier",
        SkillType.OrbColorBias    => $"{ColorNames[offer.ColorIndex]} orbs spawn more often",
        SkillType.ExtraLife       => "+1 free revive this run",
        SkillType.MatchFiveBonus  => "+0.5× bonus when matching 5+ orbs",
        SkillType.ColorClearBonus => $"+0.5× bonus when clearing {ColorNames[offer.ColorIndex]} orbs",
        SkillType.StartingCombo   => "Every resolve starts with +1 combo",
        _                         => ""
    };
}
