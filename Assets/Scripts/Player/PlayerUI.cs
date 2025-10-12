using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    [Header("Runtime UI")] [SerializeField]
    private GameObject _gameplayerUI;

    [SerializeField] private GameObject _playerStats;
    [SerializeField] private GameObject _resultUI;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _manaSlider;
    //[SerializeField] private Button _questButton;
    [SerializeField] private GameObject _questPanel;

    [Header("Win UI")] [SerializeField] private GameObject _victoryUI;
    [SerializeField] private TextMeshProUGUI _winText;
    [SerializeField] private GameObject _playStats;
    [SerializeField] private TextMeshProUGUI _gameInfo;
    [SerializeField] private GameObject _buttons;
    [SerializeField] private ScrollRect _winScrollRect;
    [SerializeField] private float _scrollSpeed = 0.1f;
    [SerializeField] private float _scrollDelay = 3f;
    [SerializeField] private TextMeshProUGUI _playTimeText;
    [SerializeField] private TextMeshProUGUI _enemyDefeatedText;
    [SerializeField] private TextMeshProUGUI _expGainText;
    [SerializeField] private TextMeshProUGUI _goldGainText;
    [SerializeField] private Button _startANewGameButton;
    [SerializeField] private Button _returnHomeButton;

    [Header("Dead UI")] [SerializeField] private GameObject _deadUI;
    [SerializeField] private TextMeshProUGUI _tipText;

    [Header("Gameplay UI")] [SerializeField]
    private Image _bloodOverlay;

    [SerializeField, Range(0.5f, 3f)] private float _bloodFadeSpeed = 1.5f;
    [SerializeField, Range(0f, 1f)] private float _minBloodAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float _maxBloodAlpha = 0.7f;

    [SerializeField] private float _healthPlus;
    [SerializeField] private float _manaPlus;

    private int _enemyDefeated;
    private int _expGain;
    private int _goldGain;
    private float _playTime;

    [SerializeField] private List<string> _deathTips = new();

    private bool _isAnySkillIsActive = false;
    private bool _isDead = false;
    private bool _isShowQuest = false;

    private Tweener _tweenWinTextIn;
    private Tweener _tweenWinTextOut;
    private Tweener _tweenPlayStatsIn;
    private Tweener _tweenPlayStatsOut;
    private Tweener _tweenGameInfoIn;
    private Tweener _tweenButtonIn;
    private Tweener _tweenScroll;

    [System.Serializable]
    public class SkillSlot
    {
        public string slotName;
        public Image coverSkill;
        public float timeReload;
        [HideInInspector] public bool isOnCooldown = false;
        public TextMeshProUGUI countdownText;
    }

    public List<SkillSlot> skillSlots = new();

    private void Start()
    {
        foreach (var skill in skillSlots.Where(skill => skill.coverSkill != null))
        {
            skill.coverSkill.fillAmount = 0f;
            skill.coverSkill.gameObject.SetActive(false);
        }

        if (_bloodOverlay != null)
        {
            var c = _bloodOverlay.color;
            c.a = 0f;
            _bloodOverlay.color = c;
        }

        _resultUI.SetActive(false);
        _deadUI.SetActive(false);
        _victoryUI.SetActive(false);

        UpdateSkillUI();
    }

    private void Update()
    {
        if (!_isDead) _playTime += Time.deltaTime;
        UpdateBloodOverlay();
    }

    public void SetUpHealth(float maxHealth, float maxMana)
    {
        _healthSlider.maxValue = maxHealth;
        _manaSlider.maxValue = maxMana;
        _healthSlider.value = maxHealth;
        _manaSlider.value = maxMana;
    }

    private void UpdateSkillUI()
    {
        foreach (var skill in skillSlots)
        {
            if (skill.countdownText)
            {
                skill.countdownText.gameObject.SetActive(skill.isOnCooldown);
                skill.countdownText.text = skill.timeReload.ToString();
            }
        }
    }

    public void UseSkill_F() => UseSkillByIndex(1);
    public void UseSkill_V() => UseSkillByIndex(0);

    private void UseSkillByIndex(int index)
    {
        if (index < 0 || index >= skillSlots.Count) return;
        var skill = skillSlots[index];
        if (skill.isOnCooldown || skill.coverSkill == null) return;
        _isAnySkillIsActive = true;
        SkillCooldownRoutine(skill).Forget();
    }

    private void InitEventButton()
    {
        //_questButton.onClick.AddListener(OnQuestButtonPressed);
        _startANewGameButton.onClick.AddListener(OnStartANewGameButtonPressed);
        _returnHomeButton.onClick.AddListener(OnReturnHomeButtonPressed);
    }

    public void OnQuestButtonPressed()
    {
        _isShowQuest = !_isShowQuest;
        _questPanel.gameObject.SetActive(_isShowQuest);
    }

    private async UniTaskVoid SkillCooldownRoutine(SkillSlot skill)
    {
        skill.isOnCooldown = true;
        if (skill.countdownText != null) skill.countdownText.gameObject.SetActive(true);
        if (skill.coverSkill != null) skill.coverSkill.gameObject.SetActive(true);

        float total = Mathf.Max(0.0001f, skill.timeReload);
        float remaining = total;

        if (skill.coverSkill != null) skill.coverSkill.fillAmount = 1f;
        if (skill.countdownText != null) skill.countdownText.text = Mathf.CeilToInt(remaining).ToString();

        _isAnySkillIsActive = true;

        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            float t = Mathf.Clamp01(remaining / total);
            if (skill.coverSkill != null) skill.coverSkill.fillAmount = t;
            if (skill.countdownText != null)
                skill.countdownText.text = Mathf.Max(0, Mathf.CeilToInt(remaining)).ToString();
            await UniTask.Yield();
        }

        if (skill.coverSkill != null) skill.coverSkill.fillAmount = 0f;
        if (skill.coverSkill != null) skill.coverSkill.gameObject.SetActive(false);
        if (skill.countdownText != null) skill.countdownText.gameObject.SetActive(false);

        skill.isOnCooldown = false;
        _isAnySkillIsActive = false;
    }


    public void UseBottleOfHealth(float health)
    {
        _healthSlider.value = Mathf.Min(_healthSlider.maxValue, _healthSlider.value + health);
    }

    public void UseBottleOfMana(float mana)
    {
        _manaSlider.value = Mathf.Min(_manaSlider.maxValue, _manaSlider.value + mana);
    }

    private void UpdateBloodOverlay()
    {
        if (_bloodOverlay == null) return;
        var healthPercent = _healthSlider.value / _healthSlider.maxValue;
        var targetAlpha = Mathf.Lerp(_maxBloodAlpha, _minBloodAlpha, healthPercent);
        var c = _bloodOverlay.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * _bloodFadeSpeed);
        _bloodOverlay.color = c;
    }

    public void OnPlayerTakeDamage(float damage)
    {
        if (_isDead) return;
        _healthSlider.value = Mathf.Max(0f, _healthSlider.value - damage);
        if ((_healthSlider.value > 0f)) return;
        _isDead = true;
        ShowDeadUI();
    }

    public void OnEnemyDefeated(int exp, int gold)
    {
        _enemyDefeated++;
        _expGain += exp;
        _goldGain += gold;
    }

    private void ShowVictoryUI()
    {
        _gameplayerUI.SetActive(false);
        _resultUI.SetActive(true);
        _victoryUI.SetActive(true);
        _deadUI.SetActive(false);
        SetResultStats();
        StartAutoScrollCredits().Forget();
    }

    private void ShowDeadUI()
    {
        _gameplayerUI.SetActive(false);
        _resultUI.SetActive(true);
        _deadUI.SetActive(true);
        _victoryUI.SetActive(false);
        SetResultStats();
        ShowRandomTip();
    }

    private void SetResultStats()
    {
        _playTimeText.text = $"Play Time     : {_playTime:0.0}s";
        _enemyDefeatedText.text = $"Enemy Defeated: {_enemyDefeated}";
        _expGainText.text = $"Exp Gain      : {_expGain}";
        _goldGainText.text = $"Gold Gain     : {_goldGain}";
    }

    private void ShowRandomTip()
    {
        if (_tipText == null || _deathTips.Count == 0) return;
        var index = Random.Range(0, _deathTips.Count);
        _tipText.text = _deathTips[index];
    }

    public void SetAchievement(int enemyDefeated, int expGain, int goldGain)
    {
        _enemyDefeated = enemyDefeated;
        _expGain = expGain;
        _goldGain = goldGain;
    }

    [Button("Game Completed")]
    public void GameCompleted() => ShowVictoryUI();

    [Button("Game Defeated")]
    public void GameDefeated() => ShowDeadUI();

    private async UniTaskVoid StartAutoScrollCredits()
    {
        if (_winScrollRect == null || _winText == null) return;

        _winText.gameObject.SetActive(true);
        _winText.alpha = 0f;
        _playStats.SetActive(false);
        _gameInfo.gameObject.SetActive(false);
        _buttons.SetActive(false);
        _winScrollRect.verticalNormalizedPosition = 1f;

        _tweenWinTextIn = _winText.DOFade(1f, 1.2f).SetEase(Ease.OutQuad);
        await _tweenWinTextIn.AsyncWaitForCompletion();
        await UniTask.Delay(700);
        _tweenWinTextOut = _winText.DOFade(0f, 1f).SetEase(Ease.InQuad);
        await _tweenWinTextOut.AsyncWaitForCompletion();
        _winText.gameObject.SetActive(false);

        _playStats.SetActive(true);
        CanvasGroup playGroup = _playStats.GetComponent<CanvasGroup>() ?? _playStats.AddComponent<CanvasGroup>();
        playGroup.alpha = 0f;
        _tweenPlayStatsIn = playGroup.DOFade(1f, 1f);
        await _tweenPlayStatsIn.AsyncWaitForCompletion();
        await UniTask.Delay(700);
        _tweenPlayStatsOut = playGroup.DOFade(0f, 1f);
        await _tweenPlayStatsOut.AsyncWaitForCompletion();
        _playStats.SetActive(false);

        _gameInfo.gameObject.SetActive(true);
        //CanvasGroup infoGroup = _gameInfo.GetComponent<CanvasGroup>() ?? _gameInfo.AddComponent<CanvasGroup>();
        _gameInfo.alpha = 0f;
        _tweenGameInfoIn = _gameInfo.DOFade(1f, 1.2f);
        await _tweenGameInfoIn.AsyncWaitForCompletion();
        await UniTask.Delay(1000);

        _tweenScroll = DOTween.To(
            () => _winScrollRect.verticalNormalizedPosition,
            x => _winScrollRect.verticalNormalizedPosition = x,
            0f,
            10f
        ).SetEase(Ease.Linear);
        await _tweenScroll.AsyncWaitForCompletion();
        await UniTask.Delay(1000);

        _gameInfo.gameObject.SetActive(false);
        _winText.gameObject.SetActive(true);
        _tweenWinTextIn = _winText.DOFade(1f, 1.2f).SetEase(Ease.OutQuad);
        await _tweenWinTextIn.AsyncWaitForCompletion();
        await UniTask.Delay(1000);

        _buttons.SetActive(true);
        CanvasGroup btnGroup = _buttons.GetComponent<CanvasGroup>() ?? _buttons.AddComponent<CanvasGroup>();
        btnGroup.alpha = 0f;
        _tweenButtonIn = btnGroup.DOFade(1f, 1f);
        await _tweenButtonIn.AsyncWaitForCompletion();
    }

    private void OnDestroy()
    {
        _tweenWinTextIn?.Kill();
        _tweenWinTextOut?.Kill();
        _tweenPlayStatsIn?.Kill();
        _tweenPlayStatsOut?.Kill();
        _tweenGameInfoIn?.Kill();
        _tweenButtonIn?.Kill();
        _tweenScroll?.Kill();
    }

    private void OnStartANewGameButtonPressed()
    {
        SceneManager.LoadScene(1);
    }

    private void OnReturnHomeButtonPressed()
    {
        SceneManager.LoadScene(1);
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        _healthSlider.value = currentHealth;
    }
}