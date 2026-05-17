using System;
using System.Collections.Generic;
using UnityEngine;

public struct SkillOffer
{
    public SkillType  Type;
    public int        ColorIndex; // 0-5 for color-based skills, -1 otherwise
    public SkillManager.SkillRarity Rarity;
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
    public int     TimeSwellStacks          { get; private set; }
    public float   FirstStrikeBonus         { get; private set; }
    public float[] ColorRushBonuses         { get; private set; } // index 0-5 → Suits red-pink

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
        TimeSwellStacks          = 0;
        FirstStrikeBonus         = 0f;
        ColorRushBonuses         = new float[] { 0f, 0f, 0f, 0f, 0f, 0f };
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
                if (ExtraLives < 3) ExtraLives++;
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
            case SkillType.TimeSwell:
                TimeSwellStacks++;
                break;
            case SkillType.FirstStrike:
                FirstStrikeBonus += 0.2f;
                break;
            case SkillType.DiamondSurge:
                // immediate effect — handled by ScoreManager.OnSkillApplied
                break;
            case SkillType.ColorRush:
                ColorRushBonuses[offer.ColorIndex] += 0.2f;
                break;
        }
    }

    public enum SkillRarity { Common, Uncommon, Rare }

    private static readonly (SkillType type, int color, SkillRarity rarity)[] _offerPool = BuildOfferPool();

    private static (SkillType, int, SkillRarity)[] BuildOfferPool()
    {
        var pool = new List<(SkillType, int, SkillRarity)>();
        // Rare
        pool.Add((SkillType.ExtraLife, -1, SkillRarity.Rare));
        // Uncommon
        foreach (var t in new[] { SkillType.ComboMultiplier, SkillType.BaseMultiplier,
                                   SkillType.MatchFiveBonus, SkillType.StartingCombo,
                                   SkillType.TimeSwell, SkillType.FirstStrike })
            pool.Add((t, -1, SkillRarity.Uncommon));
        // Common (color-indexed ×6, then singletons)
        foreach (var t in new[] { SkillType.OrbColorBias, SkillType.ColorClearBonus, SkillType.ColorRush })
            for (int c = 0; c < 6; c++)
                pool.Add((t, c, SkillRarity.Common));
        pool.Add((SkillType.DiamondSurge, -1, SkillRarity.Common));
        return pool.ToArray();
    }

    public SkillOffer[] GetRandomSkills(int count)
    {
        var result = new SkillOffer[count];
        var used   = new HashSet<(SkillType, int)>();

        for (int i = 0; i < count; i++)
        {
            // Weighted rarity roll: Rare 5%, Uncommon 35%, Common 60%
            SkillRarity targetRarity;
            int roll = UnityEngine.Random.Range(0, 100);
            if      (roll < 5)            targetRarity = SkillRarity.Rare;
            else if (roll < 40)           targetRarity = SkillRarity.Uncommon;
            else                          targetRarity = SkillRarity.Common;

            SkillOffer offer = TryPickFromRarity(targetRarity, used)
                            ?? TryPickFromRarity(SkillRarity.Uncommon, used)
                            ?? TryPickFromRarity(SkillRarity.Common, used)
                            ?? default;

            result[i] = offer;
            used.Add((offer.Type, offer.ColorIndex));
        }
        return result;
    }

    private SkillOffer? TryPickFromRarity(SkillRarity rarity, HashSet<(SkillType, int)> used)
    {
        var candidates = new List<(SkillType type, int color)>();
        foreach (var (type, color, r) in _offerPool)
        {
            if (r != rarity) continue;
            if (used.Contains((type, color))) continue;
            if (type == SkillType.ExtraLife && ExtraLives >= 3) continue;
            candidates.Add((type, color));
        }
        if (candidates.Count == 0) return null;
        var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        return new SkillOffer { Type = pick.type, ColorIndex = pick.color, Rarity = rarity };
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
        SkillType.TimeSwell       => "Time Swell",
        SkillType.FirstStrike     => "First Strike",
        SkillType.DiamondSurge    => "Diamond Surge",
        SkillType.ColorRush       => $"{ColorNames[offer.ColorIndex]} Rush",
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
        SkillType.TimeSwell       => "Every 5 combos in a resolve adds +0.5s to the timer",
        SkillType.FirstStrike     => "+20% score on the first resolve of each level",
        SkillType.DiamondSurge    => "+2 bonus diamonds whenever a diamond orb is collected",
        SkillType.ColorRush       => $"Clearing {ColorNames[offer.ColorIndex]} in a resolve adds +0.2s to the timer",
        _                         => ""
    };
}
