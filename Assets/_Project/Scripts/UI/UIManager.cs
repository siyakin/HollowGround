using HollowGround.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class UIManager : Singleton<UIManager>
    {

        [SerializeField] private GameObject _resourceBarPanel;
        [SerializeField] private GameObject _buildMenuPanel;
        [SerializeField] private GameObject _buildingInfoPanel;
        [SerializeField] private GameObject _toastPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _trainingPanel;
        [SerializeField] private GameObject _armyPanel;
        [SerializeField] private GameObject _battleReportPanel;
        [SerializeField] private GameObject _heroPanel;
        [SerializeField] private GameObject _worldMapPanel;
        [SerializeField] private GameObject _questLogPanel;
        [SerializeField] private GameObject _techTreePanel;
        [SerializeField] private GameObject _factionTradePanel;
        [SerializeField] private GameObject _saveMenuPanel;

        private bool _saveBtnCreated;
        private bool _isPaused;

        private void Start()
        {
            EnsurePausePanelContent();
        }

        private void EnsurePausePanelContent()
        {
            if (_pausePanel == null || _pausePanel.transform.childCount > 0) return;

            var bg = _pausePanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);
            bg.raycastTarget = true;
            var cg = _pausePanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = _pausePanel.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;

            var center = UIPrimitiveFactory.CreateUIObject("PauseCenter", _pausePanel.transform);
            UIPrimitiveFactory.SetAnchors(center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            center.sizeDelta = new UnityEngine.Vector2(300f, 280f);

            var panelBg = UIPrimitiveFactory.AddImage(center, UIColors.Default.PanelBg);
            UIPrimitiveFactory.StretchFull(panelBg.rectTransform);

            var vlg = UIPrimitiveFactory.AddStandardVLG(center.gameObject,
                new RectOffset(30, 30, 30, 30), 15);

            var title = UIPrimitiveFactory.AddThemedText(center, "PAUSED", 30,
                UIColors.Default.Text, TextAlignmentOptions.Center);
            UIPrimitiveFactory.StretchFull(title.rectTransform);
            UIPrimitiveFactory.AddLayoutElement(title.gameObject, minHeight: 45);

            UIPrimitiveFactory.AddLayoutElement(
                UIPrimitiveFactory.CreateButton(center, "ResumeBtn", "Resume", OnResumeButton,
                    UIColors.Default.Ok).gameObject,
                minHeight: 45, preferredHeight: 45);

            UIPrimitiveFactory.AddLayoutElement(
                UIPrimitiveFactory.CreateButton(center, "SaveBtn", "Save / Load", OnSaveGameButton,
                    UIColors.Default.Gold).gameObject,
                minHeight: 45, preferredHeight: 45);

            UIPrimitiveFactory.AddLayoutElement(
                UIPrimitiveFactory.CreateButton(center, "QuitBtn", "Quit Game", OnQuitButton,
                    UIColors.Default.Danger).gameObject,
                minHeight: 45, preferredHeight: 45);

            _pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null) return;

            var kb = UnityEngine.InputSystem.Keyboard.current;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                TogglePauseMenu();
                return;
            }

            if (_isPaused) return;

            if (kb.f5Key.wasPressedThisFrame)
                QuickSave();
            if (kb.f9Key.wasPressedThisFrame)
                QuickLoad();
        }

        private void QuickSave()
        {
            if (HollowGround.Core.SaveSystem.Instance == null) return;
            HollowGround.Core.SaveSystem.Instance.QuickSave();
            ToastUI.Show("Quick saved!", UIColors.Default.Ok);
        }

        private void QuickLoad()
        {
            if (HollowGround.Core.SaveSystem.Instance == null) return;
            if (HollowGround.Core.SaveSystem.Instance.HasSave("quicksave"))
            {
                HollowGround.Core.SaveSystem.Instance.Load("quicksave");
                ToastUI.Show("Quick loaded!", UIColors.Default.Ok);
            }
            else
            {
                ToastUI.Show("No quicksave found!", UIColors.Default.Warn);
            }
        }

        public void TogglePauseMenu()
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                if (GameManager.Instance != null) GameManager.Instance.TogglePause();
                if (HollowGround.Core.TimeManager.Instance != null) HollowGround.Core.TimeManager.Instance.TogglePause();
                ShowPausePanel();
            }
            else
            {
                CloseAllSubPanels();
                if (HollowGround.Core.TimeManager.Instance != null && HollowGround.Core.TimeManager.Instance.IsPaused)
                    HollowGround.Core.TimeManager.Instance.TogglePause();
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                    GameManager.Instance.TogglePause();
            }
        }

        private void ShowPausePanel()
        {
            if (_pausePanel == null) return;
            _pausePanel.SetActive(true);
        }

        private void CloseAllSubPanels()
        {
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_saveMenuPanel != null) _saveMenuPanel.SetActive(false);
        }

        public void OnResumeButton()
        {
            _isPaused = false;
            CloseAllSubPanels();
            if (HollowGround.Core.TimeManager.Instance != null && HollowGround.Core.TimeManager.Instance.IsPaused)
                HollowGround.Core.TimeManager.Instance.TogglePause();
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.TogglePause();
        }

        public void OnSaveGameButton()
        {
            if (_saveMenuPanel != null)
                _saveMenuPanel.SetActive(true);
        }

        public void OnQuitButton()
        {
            Application.Quit();
        }

        public void TogglePanel(GameObject panel)
        {
            if (panel == null) return;
            panel.SetActive(!panel.activeSelf);
        }

        public void ShowPanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(true);
        }

        public void HidePanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(false);
        }

        public void ToggleBuildMenu()
        {
            TogglePanel(_buildMenuPanel);
        }

        public void ShowBuildingInfo()
        {
            ShowPanel(_buildingInfoPanel);
        }

        public void HideBuildingInfo()
        {
            HidePanel(_buildingInfoPanel);
        }

        public void TogglePause()
        {
            TogglePanel(_pausePanel);
        }

        public void ToggleTrainingPanel()
        {
            TogglePanel(_trainingPanel);
        }

        public void ToggleArmyPanel()
        {
            TogglePanel(_armyPanel);
        }

        public void ToggleBattleReport()
        {
            TogglePanel(_battleReportPanel);
        }

        public void ToggleHeroPanel()
        {
            TogglePanel(_heroPanel);
        }

        public void ToggleWorldMap()
        {
            TogglePanel(_worldMapPanel);
        }

        public void ToggleQuestLog()
        {
            TogglePanel(_questLogPanel);
        }

        public void ToggleTechTree()
        {
            TogglePanel(_techTreePanel);
        }

        public void ToggleFactionTrade()
        {
            TogglePanel(_factionTradePanel);
        }

        public void ToggleSaveMenu()
        {
            TogglePanel(_saveMenuPanel);
        }

        public void QuitToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
