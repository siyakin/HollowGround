using HollowGround.Buildings;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.UI
{
    public enum PauseSubState
    {
        None,
        PauseMenu,
        SaveMenu,
        About
    }

    public class PauseController
    {
        public bool IsPaused { get; private set; }
        public PauseSubState SubState { get; private set; } = PauseSubState.None;

        public bool IsInputBlocked => IsPaused || SubState == PauseSubState.SaveMenu || SubState == PauseSubState.About;

        private readonly GameObject _pausePanel;
        private readonly GameObject _saveMenuPanel;
        private readonly GameObject _aboutPanel;
        private readonly PanelManager _panels;

        public PauseController(GameObject pausePanel, GameObject saveMenuPanel, GameObject aboutPanel, PanelManager panels)
        {
            _pausePanel = pausePanel;
            _saveMenuPanel = saveMenuPanel;
            _aboutPanel = aboutPanel;
            _panels = panels;

            if (_pausePanel != null) _pausePanel.SetActive(false);
        }

        public bool HandleEscape()
        {
            if (BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing)
                return false;

            if (SubState == PauseSubState.About)
            {
                CloseAbout();
                return true;
            }

            if (IsPaused)
            {
                if (SubState == PauseSubState.SaveMenu)
                {
                    ShowPausePanel();
                    return true;
                }
                TogglePause();
                return true;
            }

            if (_panels.IsPanelOpen)
            {
                _panels.CloseCurrent();
                return true;
            }

            TogglePause();
            return true;
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;

            if (IsPaused)
            {
                _panels.CloseAll();
                GameManager.Instance?.TogglePause();
                TimeManager.Instance?.TogglePause();
                ShowPausePanel();
            }
            else
            {
                ClosePauseAndSubPanels();
                if (TimeManager.Instance != null && TimeManager.Instance.IsPaused)
                    TimeManager.Instance.TogglePause();
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                    GameManager.Instance.TogglePause();
            }
        }

        public void Resume()
        {
            IsPaused = false;
            ClosePauseAndSubPanels();
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused)
                TimeManager.Instance.TogglePause();
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.TogglePause();
        }

        public void ResumeAfterLoad()
        {
            if (_saveMenuPanel != null) _saveMenuPanel.SetActive(false);
            ClosePauseAndSubPanels();
            IsPaused = false;

            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused)
                TimeManager.Instance.TogglePause();
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.TogglePause();
        }

        public void ToggleAbout()
        {
            if (_aboutPanel == null) return;

            if (SubState == PauseSubState.About)
            {
                CloseAbout();
            }
            else
            {
                var about = _aboutPanel.GetComponent<AboutPanelUI>();
                if (about != null) about.Show();
                else _aboutPanel.SetActive(true);
                TimeManager.Instance?.SetSpeed(0);
                SubState = PauseSubState.About;
            }
        }

        public void ToggleSaveMenu()
        {
            if (_saveMenuPanel == null) return;

            if (SubState == PauseSubState.SaveMenu)
            {
                ShowPausePanel();
            }
            else
            {
                _saveMenuPanel.SetActive(true);
                if (_pausePanel != null) _pausePanel.SetActive(false);
                SubState = PauseSubState.SaveMenu;
            }
        }

        public void OpenSaveMenuFromPause()
        {
            if (_saveMenuPanel == null) return;
            _saveMenuPanel.SetActive(true);
            if (_pausePanel != null) _pausePanel.SetActive(false);
            SubState = PauseSubState.SaveMenu;
        }

        public void ShowPauseFromSaveMenu()
        {
            if (IsPaused)
                ShowPausePanel();
        }

        public void ToggleDebugHUD(GameObject debugPanel)
        {
            if (debugPanel != null)
                debugPanel.SetActive(!debugPanel.activeSelf);
        }

        private void CloseAbout()
        {
            if (_aboutPanel != null) _aboutPanel.SetActive(false);
            SubState = PauseSubState.None;
            if (!IsPaused)
                TimeManager.Instance?.SetSpeed(1);
        }

        private void ShowPausePanel()
        {
            if (_pausePanel != null) _pausePanel.SetActive(true);
            if (_saveMenuPanel != null) _saveMenuPanel.SetActive(false);
            SubState = PauseSubState.PauseMenu;
        }

        private void ClosePauseAndSubPanels()
        {
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_saveMenuPanel != null) _saveMenuPanel.SetActive(false);
            SubState = PauseSubState.None;
        }
    }
}
