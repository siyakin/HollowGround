using System.Collections.Generic;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class ResourceCostDisplay : MonoBehaviour
    {
        [System.Serializable]
        public class CostSlot
        {
            public ResourceType Type;
            public GameObject Root;
            public Image Dot;
            public TMP_Text ValueText;
        }

        [SerializeField] private List<CostSlot> _slots = new();

        public void SetCosts(Dictionary<ResourceType, int> costs, Dictionary<ResourceType, int> have = null)
        {
            foreach (var slot in _slots)
            {
                if (slot.Root == null) continue;

                if (!costs.TryGetValue(slot.Type, out int amount) || amount <= 0)
                {
                    slot.Root.SetActive(false);
                    continue;
                }

                slot.Root.SetActive(true);

                if (slot.Dot != null)
                    slot.Dot.color = UIColors.GetResourceColor(slot.Type);

                if (slot.ValueText != null)
                {
                    if (have != null && have.TryGetValue(slot.Type, out int owned) && owned < amount)
                    {
                        string hex = ColorUtility.ToHtmlStringRGBA(UIColors.Default.Danger);
                        slot.ValueText.text = $"<color=#{hex}>{amount}</color>";
                    }
                    else
                    {
                        slot.ValueText.text = amount.ToString();
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var slot in _slots)
            {
                if (slot.Root != null)
                    slot.Root.SetActive(false);
            }
        }
    }
}
