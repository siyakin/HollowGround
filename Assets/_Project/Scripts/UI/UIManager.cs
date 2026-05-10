using HollowGround.Core;
using HollowGround.Roads;
using UnityEngine;

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
        [SerializeField] private GameObject _aboutPanel;
        [SerializeField] private GameObject _settlerPanel;
        [SerializeField] private GameObject _debugPanel;
        [SerializeField] private GameObject _minimapPanel;

        private PanelManager _panels;
        private PauseController _pause;
        private ActionBarController _actionBar;

        public bool IsInputBlocked => _pause != null && _pause.IsInputBlocked;

        private void Start()
        {
            InitPanelManager();
            InitPauseController();
            InitActionBar();

            _panels.OnPanelOpened += OnPanelOpened;
            _panels.OnPanelClosed += OnPanelClosed;

            if (RoadManager.Instance != null)
                RoadManager.Instance.OnRoadMessage += OnRoadMessage;
        }

        protected override void OnDestroy()
        {
            if (RoadManager.Instance != null)
                RoadManager.Instance.OnRoadMessage -= OnRoadMessage;
            base.OnDestroy();
        }

        private void OnRoadMessage(string text, Color color)
        {
            ToastUI.Show(text, color);
        }

        private void OnPanelOpened(string id)
        {
            if (_panels.IsFullScreenPanel(id))
                _pause.OnFullScreenPanelOpened();
            UpdateMinimapVisibility();
        }

        private void OnPanelClosed(string id)
        {
            if (_panels.IsFullScreenPanel(id))
                _pause.OnFullScreenPanelClosed();
            UpdateMinimapVisibility();
        }

        private void UpdateMinimapVisibility()
        {
            if (_minimapPanel == null) return;
            _minimapPanel.SetActive(!_panels.IsFullScreenPanelOpen);
        }

        private void InitPanelManager()
        {
            _panels = new PanelManager();
            _panels.Register("BuildMenu", Resolve(ref _buildMenuPanel, "BuildMenu"));
            _panels.Register("Training", Resolve(ref _trainingPanel, "TrainingPanel"));
            _panels.Register("Army", Resolve(ref _armyPanel, "ArmyPanel"));
            _panels.Register("Hero", Resolve(ref _heroPanel, "HeroPanel"));
            _panels.Register("WorldMap", Resolve(ref _worldMapPanel, "WorldMapPanel"));
            _panels.Register("QuestLog", Resolve(ref _questLogPanel, "QuestLogPanel"));
            _panels.Register("TechTree", Resolve(ref _techTreePanel, "TechTreePanel"));
            _panels.Register("FactionTrade", Resolve(ref _factionTradePanel, "FactionTradePanel"));
            _panels.Register("Settler", Resolve(ref _settlerPanel, "SettlerPanel"));
            _panels.Register("BattleReport", Resolve(ref _battleReportPanel, "BattleReportPanel"));
            _panels.Register("BuildingInfo", Resolve(ref _buildingInfoPanel, "BuildingInfo"));
            _panels.Register("Toast", _toastPanel);
            _panels.Register("ResourceBar", _resourceBarPanel);
            _panels.Register("SaveMenu", Resolve(ref _saveMenuPanel, "SaveMenuPanel"));
            _panels.Register("Minimap", Resolve(ref _minimapPanel, "MinimapPanel"));

            _panels.OnPanelOpened += _ => _actionBar?.UpdateHighlights();
            _panels.OnPanelClosed += _ => _actionBar?.UpdateHighlights();
        }

        private void InitPauseController()
        {
            _pause = new PauseController(_pausePanel, _saveMenuPanel, _aboutPanel, _panels);
        }

        private void InitActionBar()
        {
            _actionBar = new ActionBarController(_panels);
            _actionBar.Initialize();
            _actionBar.UpdateHighlights();
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                _pause.HandleEscape();
                return;
            }

            if (_pause.IsPaused) return;

            if (kb.f5Key.wasPressedThisFrame) QuickSave();
            if (kb.f9Key.wasPressedThisFrame) QuickLoad();
            if (kb.f12Key.wasPressedThisFrame) _pause.ToggleDebugHUD(_debugPanel);
            if (kb.f1Key.wasPressedThisFrame) _pause.ToggleAbout();
        }

        #region Save/Load Shortcuts

        private void QuickSave()
        {
            if (SaveSystem.Instance == null) return;
            SaveSystem.Instance.QuickSave();
            ToastUI.Show("Quick saved!", UIColors.Default.Ok);
        }

        private void QuickLoad()
        {
            if (SaveSystem.Instance == null) return;
            if (SaveSystem.Instance.HasSave("quicksave"))
            {
                SaveSystem.Instance.Load("quicksave");
                ToastUI.Show("Quick loaded!", UIColors.Default.Ok);
            }
            else
            {
                ToastUI.Show("No quicksave found!", UIColors.Default.Warn);
            }
        }

        #endregion

        #region Panel Resolution

        private GameObject Resolve(ref GameObject field, string sceneName)
        {
            if (field != null) return field;
            var t = transform.Find(sceneName) ?? GameCanvas()?.transform.Find(sceneName);
            if (t != null) field = t.gameObject;
            return field;
        }

        private static GameObject GameCanvas()
        {
            var gc = GameObject.Find("GameCanvas");
            return gc != null ? gc : null;
        }

        #endregion

        #region Public Panel API

        public void ToggleBuildMenu() => _panels?.Toggle("BuildMenu");
        public void ToggleTrainingPanel() => _panels?.Toggle("Training");
        public void ToggleArmyPanel() => _panels.Toggle("Army");
        public void ToggleHeroPanel() => _panels.Toggle("Hero");
        public void ToggleWorldMap() => _panels.Toggle("WorldMap");
        public void ToggleQuestLog() => _panels.Toggle("QuestLog");
        public void ToggleTechTree() => _panels.Toggle("TechTree");
        public void ToggleFactionTrade() => _panels.Toggle("FactionTrade");
        public void ToggleSettlerPanel() => _panels.Toggle("Settler");
        public void ToggleBattleReport() => _panels.Toggle("BattleReport");

        public void TogglePause() => _pause.TogglePause();
        public void ToggleSaveMenu() => _pause.ToggleSaveMenu();
        public void ToggleAbout() => _pause.ToggleAbout();

        public void ShowBuildingInfo() => _panels.OpenOverlay("BuildingInfo");
        public void HideBuildingInfo() => _panels.CloseOverlay("BuildingInfo");

        public void OnResumeButton() => _pause.Resume();
        public void OnSaveGameButton() => _pause.OpenSaveMenuFromPause();
        public void OnQuitButton() => Application.Quit();
        public void ShowPauseFromSaveMenu() => _pause.ShowPauseFromSaveMenu();
        public void ResumeAfterLoad() => _pause.ResumeAfterLoad();

        public void TogglePanel(GameObject panel) { if (panel != null) panel.SetActive(!panel.activeSelf); }
        public void ShowPanel(GameObject panel) { if (panel != null) panel.SetActive(true); }
        public void HidePanel(GameObject panel) { if (panel != null) panel.SetActive(false); }

        public void QuitToMenu() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        public void QuitGame() => Application.Quit();

        #endregion
    }
}
