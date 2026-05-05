using System.Collections.Generic;
using HollowGround.Heroes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class HeroPanelUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private TMP_Text _summonCostText;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Button _summonBtn;

        private void OnEnable()
        {
            RefreshList();

            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged += RefreshList;
        }

        private void OnDisable()
        {
            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged -= RefreshList;
        }

        private void Awake()
        {
            if (_summonBtn != null)
                _summonBtn.onClick.AddListener(OnSummonClicked);
        }

        private void RefreshList()
        {
            if (_listContainer == null || HeroManager.Instance == null) return;

            foreach (Transform child in _listContainer)
                Destroy(child.gameObject);

            var heroes = HeroManager.Instance.AllHeroes;
            if (heroes.Count == 0)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_listContainer, "No heroes yet. Summon one!", 15, UIColors.Default.Muted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

            foreach (var hero in heroes)
            {
                var row = new GameObject($"Hero_{hero.Data.DisplayName}", typeof(RectTransform));
                row.transform.SetParent(_listContainer, false);
                var le = row.AddComponent<LayoutElement>();
                le.preferredHeight = 40;
                var rbg = row.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                var hlg = UIPrimitiveFactory.AddRowHLG(row);

                var nameT = UIPrimitiveFactory.AddThemedText(row.transform, $"{hero.Data.DisplayName} Lv{hero.Level}", 15, UIColors.Default.Text);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                var nle = nameT.gameObject.AddComponent<LayoutElement>();
                nle.preferredWidth = 150;

                var rarityColor = UIColors.GetRarityColor(hero.Data.Rarity);
                var rarT = UIPrimitiveFactory.AddThemedText(row.transform, hero.Data.Rarity.ToString(), 14, rarityColor);
                rarT.alignment = TextAlignmentOptions.MidlineLeft;
                var rle = rarT.gameObject.AddComponent<LayoutElement>();
                rle.preferredWidth = 80;

                var roleT = UIPrimitiveFactory.AddThemedText(row.transform, hero.Data.Role.ToString(), 14, UIColors.Default.Muted);
                roleT.alignment = TextAlignmentOptions.MidlineLeft;
                var roleLE = roleT.gameObject.AddComponent<LayoutElement>();
                roleLE.preferredWidth = 80;

                var statT = UIPrimitiveFactory.AddThemedText(row.transform, $"ATK:{hero.CurrentAttack} DEF:{hero.CurrentDefense}", 13, UIColors.Default.Muted);
                statT.alignment = TextAlignmentOptions.MidlineRight;
            }

            if (_headerText != null)
                _headerText.text = $"HEROES ({heroes.Count})";
        }

        private void OnSummonClicked()
        {
            if (HeroManager.Instance == null) return;

            var data = HeroManager.Instance.GetRandomHeroByRarity();
            if (data == null)
            {
                ToastUI.Show("No hero data found!", UIColors.Default.Danger);
                return;
            }

            if (!HeroManager.Instance.CanSummon(data))
            {
                ToastUI.Show($"Need {data.SummonCost} {data.SummonResource}!", UIColors.Default.Danger);
                return;
            }

            HeroManager.Instance.SummonHero(data);
            ToastUI.Show($"Summoned: {data.DisplayName} ({data.Rarity})!", UIColors.GetRarityColor(data.Rarity));
        }
    }
}
