using UnityEngine;

namespace HollowGround.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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
