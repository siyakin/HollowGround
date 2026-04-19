using System.Collections.Generic;
using HollowGround.Heroes;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class HeroPanelUI : MonoBehaviour
    {
        [Header("Hero List")]
        [SerializeField] private Transform _heroListContainer;
        [SerializeField] private GameObject _heroCardPrefab;
        [SerializeField] private Button _summonButton;
        [SerializeField] private TMP_Text _summonCostText;

        [Header("Hero Detail")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailRarity;
        [SerializeField] private TMP_Text _detailRole;
        [SerializeField] private TMP_Text _detailLevel;
        [SerializeField] private TMP_Text _detailXP;
        [SerializeField] private Slider _detailXPSlider;
        [SerializeField] private TMP_Text _detailHP;
        [SerializeField] private TMP_Text _detailAttack;
        [SerializeField] private TMP_Text _detailDefense;
        [SerializeField] private TMP_Text _detailPassive;
        [SerializeField] private TMP_Text _detailActive;
        [SerializeField] private TMP_Text _detailWeapon;
        [SerializeField] private TMP_Text _detailArmor;
        [SerializeField] private TMP_Text _detailAccessory;
        [SerializeField] private TMP_Text _detailArmyBonus;
        [SerializeField] private Button _closeDetailButton;

        private Hero _selectedHero;

        private void OnEnable()
        {
            RefreshList();

            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged += RefreshList;

            if (_summonButton != null)
                _summonButton.onClick.AddListener(OnSummonClicked);

            if (_closeDetailButton != null)
                _closeDetailButton.onClick.AddListener(CloseDetail);
        }

        private void OnDisable()
        {
            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged -= RefreshList;

            if (_summonButton != null)
                _summonButton.onClick.RemoveListener(OnSummonClicked);

            if (_closeDetailButton != null)
                _closeDetailButton.onClick.RemoveListener(CloseDetail);
        }

        private void RefreshList()
        {
            if (HeroManager.Instance == null) return;

            if (_heroListContainer != null)
            {
                foreach (Transform child in _heroListContainer)
                    Destroy(child.gameObject);

                foreach (var hero in HeroManager.Instance.AllHeroes)
                {
                    GameObject card = Instantiate(_heroCardPrefab, _heroListContainer);
                    var nameText = card.GetComponentInChildren<TMP_Text>();
                    if (nameText != null)
                        nameText.text = $"{hero.Data.DisplayName} Lv{hero.Level}";

                    var btn = card.GetComponent<Button>();
                    if (btn != null)
                    {
                        var captured = hero;
                        btn.onClick.AddListener(() => ShowDetail(captured));
                    }
                }
            }

            if (_summonCostText != null)
                _summonCostText.text = "Summon: 100 TechPart";
        }

        public void ShowDetail(Hero hero)
        {
            _selectedHero = hero;
            if (_detailPanel != null) _detailPanel.SetActive(true);

            if (_detailName != null) _detailName.text = hero.Data.DisplayName;
            if (_detailRarity != null) _detailRarity.text = hero.Data.Rarity.ToString();
            if (_detailRole != null) _detailRole.text = hero.Data.Role.ToString();
            if (_detailLevel != null) _detailLevel.text = $"Level {hero.Level}";
            if (_detailXP != null) _detailXP.text = $"XP: {hero.CurrentXP}/{hero.Data.GetXPForLevel(hero.Level)}";
            if (_detailXPSlider != null) _detailXPSlider.value = hero.GetXPProgress();
            if (_detailHP != null) _detailHP.text = $"HP: {hero.CurrentHP}";
            if (_detailAttack != null) _detailAttack.text = $"ATK: {hero.CurrentAttack} (+{hero.GetTotalBonusAttack()})";
            if (_detailDefense != null) _detailDefense.text = $"DEF: {hero.CurrentDefense} (+{hero.GetTotalBonusDefense()})";
            if (_detailPassive != null) _detailPassive.text = $"{hero.Data.PassiveAbilityName}: {hero.Data.PassiveAbilityDesc}";
            if (_detailActive != null) _detailActive.text = $"{hero.Data.ActiveAbilityName}: {hero.Data.ActiveAbilityDesc}";
            if (_detailArmyBonus != null) _detailArmyBonus.text = $"Army Bonus: +{hero.TotalArmyBonus:P0}";
            if (_detailWeapon != null) _detailWeapon.text = hero.Weapon != null ? hero.Weapon.Name : "Empty";
            if (_detailArmor != null) _detailArmor.text = hero.Armor != null ? hero.Armor.Name : "Empty";
            if (_detailAccessory != null) _detailAccessory.text = hero.Accessory != null ? hero.Accessory.Name : "Empty";
        }

        private void CloseDetail()
        {
            if (_detailPanel != null) _detailPanel.SetActive(false);
            _selectedHero = null;
        }

        private void OnSummonClicked()
        {
            if (HeroManager.Instance == null) return;

            var data = HeroManager.Instance.GetRandomHeroByRarity();
            if (data == null)
            {
                ToastUI.Show("No hero data found!");
                return;
            }

            if (!HeroManager.Instance.CanSummon(data))
            {
                ToastUI.Show("Not enough Tech Parts!");
                return;
            }

            HeroManager.Instance.SummonHero(data);
            ToastUI.Show($"Summoned: {data.DisplayName} ({data.Rarity})!");
            RefreshList();
        }
    }
}
