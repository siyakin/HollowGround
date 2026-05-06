using System;
using System.Collections.Generic;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.UI
{
    [Serializable]
    public class ResourceSlotEntry
    {
        public ResourceType Type;
        public Sprite Icon;
    }

    [CreateAssetMenu(fileName = "ResourceSlotConfig", menuName = "HollowGround/Resource Slot Config")]
    public class ResourceSlotConfig : ScriptableObject
    {
        public List<ResourceSlotEntry> Entries = new();

        public Sprite GetIcon(ResourceType type)
        {
            foreach (var entry in Entries)
                if (entry.Type == type)
                    return entry.Icon;
            return null;
        }
    }
}
