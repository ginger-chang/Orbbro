using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ScoreManager : MonoBehaviour
{
    [Header("---UI---")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private RectMask2D scoreBarMask;
    [SerializeField] public Slider timerBar;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("---Diamonds---")]
    [SerializeField] private TextMeshProUGUI diamondText;
    public int Diamonds { get; private set; }

    [Header("---Multipliers---")]
    public float comboMultiplier = 1.3f;
    public float iterMultiplier  = 1.2f;
    public int   baseMultiplier  = 100;

    public int   CurrentLevel     { get; private set; }
    public float CurrentTimeLimit { get; private set; }

    // Fired after a level advances so GameController can react (e.g. reset timer in PlayModeState)
    public event Action OnLevelGoalReached;

    private int _score;
    private int _curScore;
    private int _curCombo;
    private int _curIter;
    private int _highScore;
    private int _curLevelScoreGoal;
    private int _scoreGoal;
    private string _highScoreKey;

    private Tween _warningTween;
    private Color _timerBarNormalColor;

    private SkillManager _skillManager;

    private void Awake()
    {
        Diamonds = PlayerPrefs.GetInt("Diamonds", 0);
        if (diamondText != null) diamondText.text = $"Diamonds: {Diamonds}";
    }

    public static int GetHighScore(GameMode mode) =>
        PlayerPrefs.GetInt($"HighScore_{mode}", 0);

    public void CollectDiamonds(int count)
    {
        Diamonds += count;
        PlayerPrefs.SetInt("Diamonds", Diamonds);
        PlayerPrefs.Save();
        if (diamondText != null) diamondText.text = $"Diamonds: {Diamonds}";
    }

    public void ResetForNewGame(GameMode mode, SkillManager skillManager = null)
    {
        _skillManager   = skillManager;
        _highScoreKey   = $"HighScore_{mode}";

        comboMultiplier = 1.3f + (skillManager?.ComboMultiplierBonus ?? 0f);
        iterMultiplier  = 1.2f;
        baseMultiplier  = 100 + (skillManager?.BaseMultiplierBonus ?? 0);

        _score    = 0;
        _curScore = 0;
        _curCombo = 0;
        _curIter  = 0;

        _highScore = PlayerPrefs.GetInt(_highScoreKey, 0);
        highScoreText.text = $"High score: {_highScore}";
        scoreText.text     = $"Score: {_score}";
        comboText.gameObject.SetActive(false);

        _curLevelScoreGoal = 0;
        _scoreGoal         = 0;

        SetupLevel(1);
    }

    public void SetupLevel(int level)
    {
        CurrentLevel = level;
        LevelParams lp = new LevelParams(level);
        _curLevelScoreGoal = lp.scoreGoal;
        _scoreGoal        += lp.scoreGoal;
        CurrentTimeLimit   = lp.timeLimit;

        timerBar.value   = 1f;
        levelText.text   = $"Level {level}";

        if (level == 1)
        {
            var p = scoreBarMask.padding;
            p.w = 1000f;
            scoreBarMask.padding = p;
        }
    }

    // Called by GameController at the start of each resolve pass
    public void BeginResolve()
    {
        _curScore = 0;
        _curCombo = _skillManager?.StartingCombo ?? 0;
        _curIter  = 0;
    }

    // Called once per disappear iteration within a resolve pass
    public void BeginIteration() => _curIter++;

    // Called once per match group within an iteration
    public void AddMatchScore(int numOrb, Orb.Suits suit = Orb.Suits.none)
    {
        _curCombo++;
        float s = _curScore * comboMultiplier
                  + numOrb * baseMultiplier
                    * Mathf.Pow(comboMultiplier, _curCombo - 1)
                    * Mathf.Pow(iterMultiplier,  _curIter  - 1);

        if (_skillManager != null)
        {
            if (numOrb >= 5 && _skillManager.MatchFiveBonusMultiplier > 0f)
                s *= 1f + _skillManager.MatchFiveBonusMultiplier;

            int suitIndex = (int)suit - 1;
            if (suitIndex >= 0 && suitIndex < 6 && _skillManager.ColorClearMultipliers[suitIndex] > 0f)
                s *= 1f + _skillManager.ColorClearMultipliers[suitIndex];
        }

        int newCurScore = Mathf.FloorToInt(s);
        _score    += newCurScore - _curScore;
        _curScore  = newCurScore;

        scoreText.text = $"Score: {_score}";
        comboText.gameObject.SetActive(true);
        comboText.text = $"Combo x{_curCombo}";

        if (_score > _highScore)
        {
            _highScore = _score;
            PlayerPrefs.SetInt(_highScoreKey, _score);
            PlayerPrefs.Save();
            highScoreText.text = $"High score: {_highScore}";
        }

        AnimateScoreBar();
    }

    public void HideComboText() => comboText.gameObject.SetActive(false);

    public void ApplyNoMatchPenalty()
    {
        int penalty       = Mathf.FloorToInt(_curLevelScoreGoal * 0.2f);
        int levelProgress = _score - (_scoreGoal - _curLevelScoreGoal);

        if (levelProgress <= penalty)
            _score = _scoreGoal - _curLevelScoreGoal;
        else
            _score -= penalty;

        scoreText.text = $"Score: {_score}";
        AnimateScoreBar();
    }

    public void StartTimerWarning()
    {
        if (_warningTween != null && _warningTween.IsActive()) return;
        var fill = timerBar.fillRect.GetComponent<Image>();
        _timerBarNormalColor = fill.color;
        _warningTween = fill.DOColor(Color.red, 0.4f).SetLoops(-1, LoopType.Yoyo);
    }

    public void StopTimerWarning()
    {
        if (_warningTween == null) return;
        _warningTween.Kill();
        _warningTween = null;
        timerBar.fillRect.GetComponent<Image>().color = _timerBarNormalColor;
    }

    private void AnimateScoreBar()
    {
        var padding = scoreBarMask.padding;

        if (_score >= _scoreGoal)
        {
            AdvanceLevel();

            float offset = ScoreBarOffset();
            var seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => padding.w, x => { padding.w = x; scoreBarMask.padding = padding; }, 0f, 0.25f));
            seq.AppendCallback(() => { padding.w = 1000f; scoreBarMask.padding = padding; });
            seq.Append(DOTween.To(() => padding.w, x => { padding.w = x; scoreBarMask.padding = padding; }, offset, 0.25f));
        }
        else
        {
            float offset = ScoreBarOffset();
            DOTween.To(() => padding.w, x => { padding.w = x; scoreBarMask.padding = padding; }, offset, 0.5f);
        }
    }

    private void AdvanceLevel()
    {
        SetupLevel(CurrentLevel + 1);
        OnLevelGoalReached?.Invoke();
    }

    private float ScoreBarOffset()
    {
        int numerator   = _scoreGoal - _score;
        int denominator = _curLevelScoreGoal;
        return 1000f * ((float)numerator / denominator);
    }
}
