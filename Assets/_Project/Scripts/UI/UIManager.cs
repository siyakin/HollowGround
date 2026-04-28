using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Core;
using TMPro;
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

        private PanelManager _panels;
        private Dictionary<string, Button> _actionBarButtons;
        private Dictionary<string, ThemedButton> _actionBarThemed;
        private bool _isPaused;

        private void Start()
        {
            InitPanelManager();
            EnsurePausePanelContent();
            CacheActionBarButtons();
            UpdateActionBarHighlights();
        }

        private void InitPanelManager()
        {
            _panels = new PanelManager();
            _panels.Register("BuildMenu", _buildMenuPanel);
            _panels.Register("Training", _trainingPanel);
            _panels.Register("Army", _armyPanel);
            _panels.Register("Hero", _heroPanel);
            _panels.Register("WorldMap", _worldMapPanel);
            _panels.Register("QuestLog", _questLogPanel);
            _panels.Register("TechTree", _techTreePanel);
            _panels.Register("FactionTrade", _factionTradePanel);
            _panels.Register("BattleReport", _battleReportPanel);
            _panels.Register("BuildingInfo", _buildingInfoPanel);
            _panels.Register("Toast", _toastPanel);
            _panels.Register("ResourceBar", _resourceBarPanel);
            _panels.Register("SaveMenu", _saveMenuPanel);

            _panels.OnPanelOpened += _ => UpdateActionBarHighlights();
            _panels.OnPanelClosed += _ => UpdateActionBarHighlights();
        }

        private void CacheActionBarButtons()
        {
            _actionBarButtons = new Dictionary<string, Button>();
            _actionBarThemed = new Dictionary<string, ThemedButton>();
            var actionBar = FindActionBar();
            if (actionBar == null) return;

            var map = new (string id, string btnName)[]
            {
                ("BuildMenu", "BtnBuild"),
                ("TechTree", "BtnResearch"),
                ("Training", "BtnArmy"),
                ("Hero", "BtnHero"),
                ("QuestLog", "BtnQuest"),
                ("FactionTrade", "BtnTrade"),
                ("WorldMap", "BtnMap")
            };

            foreach (var (id, btnName) in map)
            {
                var t = actionBar.transform.Find(btnName);
                if (t != null)
                {
                    var btn = t.GetComponent<Button>();
                    if (btn != null)
                    {
                        _actionBarButtons[id] = btn;
                        var themed = t.GetComponent<ThemedButton>();
                        if (themed == null)
                            themed = t.gameObject.AddComponent<ThemedButton>();
                        themed.styleType = UIStyleType.ActionBarButton;
                        _actionBarThemed[id] = themed;
                    }
                }
            }
        }

        private Transform FindActionBar()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var t = canvas.transform.Find("ActionBar");
                if (t != null) return t;
            }

            var found = FindAnyObjectByType<Canvas>();
            if (found != null)
                return found.transform.Find("ActionBar");

            return null;
        }

        private void UpdateActionBarHighlights()
        {
            foreach (var kvp in _actionBarThemed)
            {
                if (kvp.Value == null) continue;
                kvp.Value.SetSelected(_panels.IsOpen(kvp.Key));
            }
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing)
                    return;

                if (_isPaused)
                {
                    if (_saveMenuPanel != null && _saveMenuPanel.activeSelf)
                    {
                        _saveMenuPanel.SetActive(false);
                        if (_pausePanel != null) _pausePanel.SetActive(true);
                    }
                    else
                        TogglePauseMenu();
                }
                else if (_panels.IsPanelOpen)
                    _panels.CloseCurrent();
                else
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
                _panels.CloseAll();
                if (GameManager.Instance != null) GameManager.Instance.TogglePause();
                if (HollowGround.Core.TimeManager.Instance != null) HollowGround.Core.TimeManager.Instance.TogglePause();
                ShowPausePanel();
            }
            else
            {
                ClosePauseAndSubPanels();
                if (HollowGround.Core.TimeManager.Instance != null && HollowGround.Core.TimeManager.Instance.IsPaused)
                    HollowGround.Core.TimeManager.Instance.TogglePause();
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                    GameManager.Instance.TogglePause();
            }
        }

        private void ShowPausePanel()
        {
            if (_pausePanel != null) _pausePanel.SetActive(true);
        }

        private void ClosePauseAndSubPanels()
        {
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_saveMenuPanel != null) _saveMenuPanel.SetActive(false);
        }

        public void OnResumeButton()
        {
            _isPaused = false;
            ClosePauseAndSubPanels();
            if (HollowGround.Core.TimeManager.Instance != null && HollowGround.Core.TimeManager.Instance.IsPaused)
                HollowGround.Core.TimeManager.Instance.TogglePause();
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.TogglePause();
        }

        public void OnSaveGameButton()
        {
            if (_saveMenuPanel == null) return;
            _saveMenuPanel.SetActive(true);
            if (_isPaused && _pausePanel != null) _pausePanel.SetActive(false);
        }

        public void OnQuitButton()
        {
            Application.Quit();
        }

        public void ShowPauseFromSaveMenu()
        {
            if (_isPaused && _pausePanel != null)
                _pausePanel.SetActive(true);
        }

        public void ToggleBuildMenu() => _panels?.Toggle("BuildMenu");
        public void ToggleTrainingPanel() => _panels?.Toggle("Training");
        public void ToggleArmyPanel() => _panels.Toggle("Army");
        public void ToggleHeroPanel() => _panels.Toggle("Hero");
        public void ToggleWorldMap() => _panels.Toggle("WorldMap");
        public void ToggleQuestLog() => _panels.Toggle("QuestLog");
        public void ToggleTechTree() => _panels.Toggle("TechTree");
        public void ToggleFactionTrade() => _panels.Toggle("FactionTrade");
        public void ToggleBattleReport() => _panels.Toggle("BattleReport");

        public void TogglePause() => TogglePauseMenu();

        public void ToggleSaveMenu()
        {
            if (_saveMenuPanel == null) return;
            bool opening = !_saveMenuPanel.activeSelf;
            _saveMenuPanel.SetActive(opening);
            if (opening && _isPaused && _pausePanel != null)
                _pausePanel.SetActive(false);
        }

        public void ShowBuildingInfo() => _panels.OpenOverlay("BuildingInfo");
        public void HideBuildingInfo() => _panels.CloseOverlay("BuildingInfo");

        public void TogglePanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(!panel.activeSelf);
        }

        public void ShowPanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(true);
        }

        public void HidePanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(false);
        }

        public void QuitToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void EnsurePausePanelContent()
        {
            if (_pausePanel == null) return;
            _pausePanel.SetActive(false);
        }
    }
}
