using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Resources;

namespace HollowGround.Core
{
    public interface ISaveable
    {
        string SaveKey { get; }
        void CaptureSaveData(SaveData data);
        void ApplySaveData(SaveData data);
    }
}
