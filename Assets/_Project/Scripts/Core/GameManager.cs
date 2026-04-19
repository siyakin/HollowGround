using UnityEngine;

namespace HollowGround.Core
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Building
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Menu;

        public event System.Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void StartGame()
        {
            SetState(GameState.Playing);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Paused)
                SetState(GameState.Playing);
            else if (CurrentState == GameState.Playing)
                SetState(GameState.Paused);
        }
    }
}
