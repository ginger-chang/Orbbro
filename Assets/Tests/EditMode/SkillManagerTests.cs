using NUnit.Framework;
using System.Collections.Generic;

public class SkillManagerTests
{
    private SkillManager _sm;

    [SetUp]
    public void SetUp() => _sm = new SkillManager();

    // -------------------------------------------------------
    // Apply — each skill type updates the correct field
    // -------------------------------------------------------

    [Test]
    public void Apply_ComboMultiplier_IncreasesBonus()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.ComboMultiplier, ColorIndex = -1 });
        Assert.AreEqual(0.1f, _sm.ComboMultiplierBonus, 0.0001f);
    }

    [Test]
    public void Apply_BaseMultiplier_IncreasesBonus()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.BaseMultiplier, ColorIndex = -1 });
        Assert.AreEqual(25, _sm.BaseMultiplierBonus);
    }

    [Test]
    public void Apply_ExtraLife_IncreasesCount()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.ExtraLife, ColorIndex = -1 });
        Assert.AreEqual(1, _sm.ExtraLives);
    }

    [Test]
    public void Apply_MatchFiveBonus_IncreasesMultiplier()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.MatchFiveBonus, ColorIndex = -1 });
        Assert.AreEqual(0.5f, _sm.MatchFiveBonusMultiplier, 0.0001f);
    }

    [Test]
    public void Apply_StartingCombo_IncreasesCount()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.StartingCombo, ColorIndex = -1 });
        Assert.AreEqual(1, _sm.StartingCombo);
    }

    [Test]
    public void Apply_OrbColorBias_IncrementsCorrectColorWeight()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.OrbColorBias, ColorIndex = 2 });
        Assert.AreEqual(2, _sm.OrbColorWeights[2]); // started at 1, now 2
        // other colors unchanged
        Assert.AreEqual(1, _sm.OrbColorWeights[0]);
        Assert.AreEqual(1, _sm.OrbColorWeights[5]);
    }

    [Test]
    public void Apply_ColorClearBonus_IncrementsCorrectColorMultiplier()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.ColorClearBonus, ColorIndex = 4 });
        Assert.AreEqual(0.5f, _sm.ColorClearMultipliers[4], 0.0001f);
        Assert.AreEqual(0f,   _sm.ColorClearMultipliers[0], 0.0001f);
    }

    // -------------------------------------------------------
    // Stacking — applying the same skill multiple times accumulates
    // -------------------------------------------------------

    [Test]
    public void Stack_ComboMultiplier_AccumulatesCorrectly()
    {
        var offer = new SkillOffer { Type = SkillType.ComboMultiplier, ColorIndex = -1 };
        _sm.Apply(offer);
        _sm.Apply(offer);
        _sm.Apply(offer);
        Assert.AreEqual(0.3f, _sm.ComboMultiplierBonus, 0.0001f);
    }

    [Test]
    public void Stack_BaseMultiplier_AccumulatesCorrectly()
    {
        var offer = new SkillOffer { Type = SkillType.BaseMultiplier, ColorIndex = -1 };
        _sm.Apply(offer);
        _sm.Apply(offer);
        Assert.AreEqual(50, _sm.BaseMultiplierBonus);
    }

    [Test]
    public void Stack_OrbColorBias_SameColor_AccumulatesWeight()
    {
        var offer = new SkillOffer { Type = SkillType.OrbColorBias, ColorIndex = 1 };
        _sm.Apply(offer);
        _sm.Apply(offer);
        Assert.AreEqual(3, _sm.OrbColorWeights[1]); // base 1 + 2 stacks
    }

    [Test]
    public void Stack_OrbColorBias_DifferentColors_EachColorIncrements()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.OrbColorBias, ColorIndex = 0 });
        _sm.Apply(new SkillOffer { Type = SkillType.OrbColorBias, ColorIndex = 3 });
        Assert.AreEqual(2, _sm.OrbColorWeights[0]);
        Assert.AreEqual(2, _sm.OrbColorWeights[3]);
        Assert.AreEqual(1, _sm.OrbColorWeights[1]); // untouched
    }

    [Test]
    public void Stack_MatchFiveBonus_AccumulatesCorrectly()
    {
        var offer = new SkillOffer { Type = SkillType.MatchFiveBonus, ColorIndex = -1 };
        _sm.Apply(offer);
        _sm.Apply(offer);
        Assert.AreEqual(1.0f, _sm.MatchFiveBonusMultiplier, 0.0001f);
    }

    // -------------------------------------------------------
    // Reset — all values return to defaults
    // -------------------------------------------------------

    [Test]
    public void Reset_ClearsAllBonuses()
    {
        _sm.Apply(new SkillOffer { Type = SkillType.ComboMultiplier,  ColorIndex = -1 });
        _sm.Apply(new SkillOffer { Type = SkillType.BaseMultiplier,   ColorIndex = -1 });
        _sm.Apply(new SkillOffer { Type = SkillType.ExtraLife,        ColorIndex = -1 });
        _sm.Apply(new SkillOffer { Type = SkillType.MatchFiveBonus,   ColorIndex = -1 });
        _sm.Apply(new SkillOffer { Type = SkillType.StartingCombo,    ColorIndex = -1 });
        _sm.Apply(new SkillOffer { Type = SkillType.OrbColorBias,     ColorIndex = 0  });
        _sm.Apply(new SkillOffer { Type = SkillType.ColorClearBonus,  ColorIndex = 0  });

        _sm.Reset();

        Assert.AreEqual(0f, _sm.ComboMultiplierBonus,     0.0001f);
        Assert.AreEqual(0,  _sm.BaseMultiplierBonus);
        Assert.AreEqual(0,  _sm.ExtraLives);
        Assert.AreEqual(0f, _sm.MatchFiveBonusMultiplier, 0.0001f);
        Assert.AreEqual(0,  _sm.StartingCombo);
        foreach (int w in _sm.OrbColorWeights)       Assert.AreEqual(1,  w);
        foreach (float m in _sm.ColorClearMultipliers) Assert.AreEqual(0f, m, 0.0001f);
    }

    // -------------------------------------------------------
    // GetRandomSkills — duplicate prevention within a round
    // -------------------------------------------------------

    [Test]
    public void GetRandomSkills_ReturnsCorrectCount()
    {
        var offers = _sm.GetRandomSkills(3);
        Assert.AreEqual(3, offers.Length);
    }

    [Test]
    public void GetRandomSkills_NonColorSkills_NoDuplicatesInRound()
    {
        // Run many times to catch random failures
        for (int run = 0; run < 200; run++)
        {
            var offers = _sm.GetRandomSkills(3);
            var seen = new HashSet<SkillType>();
            foreach (var offer in offers)
            {
                if (offer.Type == SkillType.OrbColorBias || offer.Type == SkillType.ColorClearBonus)
                    continue;
                Assert.IsFalse(seen.Contains(offer.Type),
                    $"Duplicate non-color skill {offer.Type} in run {run}");
                seen.Add(offer.Type);
            }
        }
    }

    [Test]
    public void GetRandomSkills_OrbColorBias_NoDuplicateColorsInRound()
    {
        for (int run = 0; run < 200; run++)
        {
            var offers = _sm.GetRandomSkills(3);
            var seenColors = new HashSet<int>();
            foreach (var offer in offers)
            {
                if (offer.Type != SkillType.OrbColorBias) continue;
                Assert.IsFalse(seenColors.Contains(offer.ColorIndex),
                    $"Duplicate OrbColorBias color {offer.ColorIndex} in run {run}");
                seenColors.Add(offer.ColorIndex);
            }
        }
    }

    [Test]
    public void GetRandomSkills_ColorClearBonus_NoDuplicateColorsInRound()
    {
        for (int run = 0; run < 200; run++)
        {
            var offers = _sm.GetRandomSkills(3);
            var seenColors = new HashSet<int>();
            foreach (var offer in offers)
            {
                if (offer.Type != SkillType.ColorClearBonus) continue;
                Assert.IsFalse(seenColors.Contains(offer.ColorIndex),
                    $"Duplicate ColorClearBonus color {offer.ColorIndex} in run {run}");
                seenColors.Add(offer.ColorIndex);
            }
        }
    }

    [Test]
    public void GetRandomSkills_OrbColorBiasAndColorClearBonus_SameColorAllowed()
    {
        // Verify cross-type same color is never incorrectly blocked.
        // Run enough iterations that if same-color were blocked, we'd statistically
        // never see it — but we should see it occasionally across 500 runs.
        bool sawSameColor = false;
        for (int run = 0; run < 500; run++)
        {
            var offers = _sm.GetRandomSkills(3);
            int biasColor  = -1;
            int clearColor = -1;
            foreach (var o in offers)
            {
                if (o.Type == SkillType.OrbColorBias)    biasColor  = o.ColorIndex;
                if (o.Type == SkillType.ColorClearBonus) clearColor = o.ColorIndex;
            }
            if (biasColor != -1 && clearColor != -1 && biasColor == clearColor)
            {
                sawSameColor = true;
                break;
            }
        }
        Assert.IsTrue(sawSameColor,
            "Expected OrbColorBias and ColorClearBonus to share a color at least once in 500 runs");
    }

    [Test]
    public void GetRandomSkills_DifferentRounds_CanRepeatSkills()
    {
        // Skills from a previous round must be offerable again next round.
        // Verify by checking that across many rounds, every skill type appears at least once.
        var seen = new HashSet<SkillType>();
        for (int run = 0; run < 200; run++)
        {
            foreach (var offer in _sm.GetRandomSkills(3))
                seen.Add(offer.Type);
        }
        foreach (SkillType t in System.Enum.GetValues(typeof(SkillType)))
            Assert.IsTrue(seen.Contains(t), $"Skill {t} never appeared across 200 rounds");
    }
}
