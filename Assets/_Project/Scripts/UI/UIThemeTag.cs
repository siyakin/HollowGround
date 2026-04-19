using UnityEngine;

namespace HollowGround.UI
{
    public enum UIStyleType
    {
        PrimaryButton,
        DangerButton,
        SecondaryButton,
        DarkPanel,
        ResourceBar,
        ActionBar,
        HeaderText,
        BodyText,
        LabelText,
        WarningText,
        DangerText,
    }

    /// <summary>
    /// Marks a UI element so UIThemeApplier knows which style to apply.
    /// Add this component to any Button, Image, or TMP text you want themed.
    /// </summary>
    public class UIThemeTag : MonoBehaviour
    {
        public UIStyleType styleType = UIStyleType.PrimaryButton;
    }
}
