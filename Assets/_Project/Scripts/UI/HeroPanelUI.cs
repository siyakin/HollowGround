using System.Collections.Generic;
using HollowGround.Heroes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class HeroPanelUI : MonoBehaviour
    {
        private static readonly Color PanelBg = new(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color RowBg = new(0.14f, 0.15f, 0.17f, 1f);
        private static readonly Color ColorSummon = new(0.3f, 0.6f, 0.95f, 1f);
        private static readonly Color ColorText = new(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorMuted = new(0.65f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorGold = new(1f, 0.85f, 0.3f, 1f);

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

            var bg = gameObject.AddComponent<Image>();
            bg.color = PanelBg;
            bg.raycastTarget = true;

            var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            _headerText = AddText(transform, "HEROES", 20, ColorGold);
            _headerText.alignment = TextAlignmentOptions.Center;
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
            summonBg.color = RowBg;
            var summonHLG = summonRow.AddComponent<HorizontalLayoutGroup>();
            summonHLG.padding = new RectOffset(15, 15, 5, 5);
            summonHLG.spacing = 15;
            summonHLG.childAlignment = TextAnchor.MiddleCenter;
            summonHLG.childControlWidth = true;
            summonHLG.childControlHeight = true;
            summonHLG.childForceExpandWidth = true;
            summonHLG.childForceExpandHeight = false;

            _summonCostText = AddText(summonRow.transform, "Summon: 100 TechPart", 15, ColorMuted);
            _summonCostText.alignment = TextAlignmentOptions.MidlineLeft;
            var costLE = _summonCostText.gameObject.AddComponent<LayoutElement>();
            costLE.preferredWidth = 180;

            var btnObj = new GameObject("SummonBtn", typeof(RectTransform));
            btnObj.transform.SetParent(summonRow.transform, false);
            var btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.minWidth = 120;
            btnLE.preferredWidth = 140;
            btnLE.minHeight = 35;
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = ColorSummon;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = AddText(btnObj.transform, "SUMMON", 15, Color.white);
            btnLabel.alignment = TextAlignmentOptions.Center;
            StretchFull(btnLabel.rectTransform);

            btn.onClick.AddListener(OnSummonClicked);

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
                var empty = AddText(_listContainer, "No heroes yet. Summon one!", 15, ColorMuted);
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
                rbg.color = RowBg;
                var hlg = row.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(12, 12, 4, 4);
                hlg.spacing = 10;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;

                var nameT = AddText(row.transform, $"{hero.Data.DisplayName} Lv{hero.Level}", 15, ColorText);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                var nle = nameT.gameObject.AddComponent<LayoutElement>();
                nle.preferredWidth = 150;

                var rarityColor = GetRarityColor(hero.Data.Rarity);
                var rarT = AddText(row.transform, hero.Data.Rarity.ToString(), 14, rarityColor);
                rarT.alignment = TextAlignmentOptions.MidlineLeft;
                var rle = rarT.gameObject.AddComponent<LayoutElement>();
                rle.preferredWidth = 80;

                var roleT = AddText(row.transform, hero.Data.Role.ToString(), 14, ColorMuted);
                roleT.alignment = TextAlignmentOptions.MidlineLeft;
                var roleLE = roleT.gameObject.AddComponent<LayoutElement>();
                roleLE.preferredWidth = 80;

                var statT = AddText(row.transform, $"ATK:{hero.CurrentAttack} DEF:{hero.CurrentDefense}", 13, ColorMuted);
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
                ToastUI.Show("No hero data found!", Color.red);
                return;
            }

            if (!HeroManager.Instance.CanSummon(data))
            {
                ToastUI.Show($"Need {data.SummonCost} {data.SummonResource}!", Color.red);
                return;
            }

            HeroManager.Instance.SummonHero(data);
            ToastUI.Show($"Summoned: {data.DisplayName} ({data.Rarity})!", GetRarityColor(data.Rarity));
        }

        private static Color GetRarityColor(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => ColorMuted,
                HeroRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f, 1f),
                HeroRarity.Rare => new Color(0.3f, 0.6f, 1f, 1f),
                HeroRarity.Epic => new Color(0.7f, 0.3f, 0.9f, 1f),
                HeroRarity.Legendary => ColorGold,
                _ => ColorMuted
            };
        }

        private static TMP_Text AddText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("T", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
