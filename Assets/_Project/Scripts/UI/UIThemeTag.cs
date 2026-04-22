using UnityEngine;

namespace HollowGround.UI
{
    public enum UIStyleType
    {
        // --- Butonlar ---
        BuildingCardButton,  // BuildMenu bina listesi kartları
        TabButton,           // Kaynak / Askeri / Sosyal / Özel sekmeleri
        ActionBarButton,     // Alt ActionBar: Build, Research, Army, Hero, Quest
        ConfirmButton,       // Pozitif eylemler: Upgrade, Train, Research, Send
        DangerButton,        // Yıkıcı eylemler: Demolish

        // --- Paneller ---
        DarkPanel,           // Genel koyu panel arka planı
        ResourceBar,         // Üst kaynak çubuğu
        ActionBar,           // Alt aksiyon çubuğu

        // --- Metinler ---
        HeaderText,          // Panel başlıkları
        BodyText,            // Açıklama / içerik metni
        LabelText,           // Alan etiketleri, ikincil bilgi
        CostText,            // Kaynak maliyeti değerleri
        WarningText,         // Uyarı mesajları
        DangerText,          // Hata / tehlike mesajları
    }

    /// <summary>
    /// UIThemeApplier'ın hangi stili uygulayacağını belirler.
    /// Button: root objeye ekle (Text child'a ekleme — otomatik bulunur).
    /// Image/TMP: doğrudan o objeye ekle.
    /// </summary>
    public class UIThemeTag : MonoBehaviour
    {
        public UIStyleType styleType = UIStyleType.BuildingCardButton;
    }
}
