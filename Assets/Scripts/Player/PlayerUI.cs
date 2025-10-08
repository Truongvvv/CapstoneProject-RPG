using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _manaSlider;

    [Header("Test Add Value")]
    [SerializeField] private float _healthPlus;
    [SerializeField] private float _manaPlus;

    private bool _isAnySkillIsActive = false;

    [System.Serializable]
    public class SkillSlot
    {
        public string slotName;
        public Image coverSkill;
        public float timeReload;
        [HideInInspector] public bool isOnCooldown = false;
    }

    public List<SkillSlot> skillSlots = new();

    private void Start()
    {
        foreach (var skill in skillSlots)
        {
            if (skill.coverSkill != null)
            {
                skill.coverSkill.fillAmount = 0f;
                skill.coverSkill.gameObject.SetActive(false);
            }
        }
    }

    public void SetUpHealth(float maxHealth, float maxMana)
    {
        _healthSlider.maxValue = maxHealth;
        _manaSlider.maxValue = maxMana;
    }

    public void UseSkill_01()
    {
        UseSkillByIndex(0);
    }

    public void UseSkill_02()
    {
        UseSkillByIndex(1);
    }

    private void UseSkillByIndex(int index)
    {
        if (index < 0 || index >= skillSlots.Count)
            return;

        SkillSlot skill = skillSlots[index];
        if (skill.isOnCooldown || skill.coverSkill == null)
            return;

        _isAnySkillIsActive = true;
        StartCoroutine(SkillCooldownRoutine(skill));
    }

    private IEnumerator SkillCooldownRoutine(SkillSlot skill)
    {
        skill.isOnCooldown = true;
        skill.coverSkill.gameObject.SetActive(true);
        skill.coverSkill.fillAmount = 1f;

        float elapsed = 0f;
        while (elapsed < skill.timeReload)
        {
            elapsed += Time.deltaTime;
            skill.coverSkill.fillAmount = 1f - (elapsed / skill.timeReload);
            yield return null;
        }

        skill.coverSkill.fillAmount = 0f;
        skill.coverSkill.gameObject.SetActive(false);
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
}
