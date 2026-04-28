using System.Collections.Generic;
using HollowGround.Heroes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class HeroPanelUI : MonoBehaviour
    {
        private TMP_Text _headerText;
        private TMP_Text _summonCostText;
        private Transform _listContainer;
        private bool _built;

        private void OnEnable()
        {
            if (!_built) BuildUI();
            RefreshList();

            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged += RefreshList;
        }

        private void OnDisable()
        {
            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged -= RefreshList;
        }

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            foreach (Transform child in root)
                Destroy(child.gameObject);

            var oldVlg = GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) DestroyImmediate(oldVlg);

            var oldImages = GetComponents<Image>();
            foreach (var img in oldImages) DestroyImmediate(img);

            UIPrimitiveFactory.SetupPanelBackground(gameObject, UIColors.Default);

            var vlg = UIPrimitiveFactory.AddStandardVLG(gameObject);

            _headerText = UIPrimitiveFactory.AddThemedText(transform, "HEROES", 20, UIColors.Default.Gold, TextAlignmentOptions.Center, UIStyleType.HeaderText);
            var headerLE = _headerText.gameObject.AddComponent<LayoutElement>();
            headerLE.preferredHeight = 35;

            var listObj = new GameObject("HeroList", typeof(RectTransform));
            listObj.transform.SetParent(root, false);
            var listLE = listObj.AddComponent<LayoutElement>();
            listLE.preferredHeight = 200;
            listLE.minHeight = 100;
            var listVLG = listObj.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _listContainer = listObj.transform;

            var summonRow = new GameObject("SummonRow", typeof(RectTransform));
            summonRow.transform.SetParent(root, false);
            var summonLE = summonRow.AddComponent<LayoutElement>();
            summonLE.preferredHeight = 45;
            var summonBg = summonRow.AddComponent<Image>();
            summonBg.color = UIColors.Default.RowBg;
            var summonHLG = UIPrimitiveFactory.AddRowHLG(summonRow, new RectOffset(15, 15, 5, 5), 15);
            summonHLG.childAlignment = TextAnchor.MiddleCenter;

            _summonCostText = UIPrimitiveFactory.AddThemedText(summonRow.transform, "Summon: 100 TechPart", 15, UIColors.Default.Muted);
            _summonCostText.alignment = TextAlignmentOptions.MidlineLeft;
            var costLE = _summonCostText.gameObject.AddComponent<LayoutElement>();
            costLE.preferredWidth = 180;

            var btn = UIPrimitiveFactory.CreateThemedButton(summonRow.transform, "SummonBtn", "SUMMON", OnSummonClicked, UIStyleType.ConfirmButton);
            var btnLE = btn.gameObject.AddComponent<LayoutElement>();
            btnLE.minWidth = 120;
            btnLE.preferredWidth = 140;
            btnLE.minHeight = 35;

            _built = true;
        }

        private void RefreshList()
        {
            if (!_built || _listContainer == null || HeroManager.Instance == null) return;

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
