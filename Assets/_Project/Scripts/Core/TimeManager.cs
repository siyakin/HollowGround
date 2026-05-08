using UnityEngine;

namespace HollowGround.Core
{
    public class TimeManager : Singleton<TimeManager>
    {
        public float GameTime { get; private set; }
        public int GameSpeed { get; private set; } = 1;
        public bool IsPaused => GameSpeed == 0;

        public event System.Action<int> OnSpeedChanged;
        public event System.Action<float> OnGameTimeChanged;

        private void Update()
        {
            if (IsPaused) return;

            float dt = Time.deltaTime * GameSpeed;
            GameTime += dt;
            OnGameTimeChanged?.Invoke(GameTime);
        }

        public void SetTime(float time)
        {
            GameTime = time;
            OnGameTimeChanged?.Invoke(GameTime);
        }

        public void SetSpeed(int speed)
        {
            int prevSpeed = GameSpeed;
            GameSpeed = Mathf.Clamp(speed, 0, 3);
            if (GameSpeed == 0 && prevSpeed > 0)
                UnityEngine.Time.timeScale = 0f;
            else if (GameSpeed > 0 && prevSpeed == 0)
                UnityEngine.Time.timeScale = 1f;
            OnSpeedChanged?.Invoke(GameSpeed);
        }

        public void TogglePause()
        {
            if (IsPaused)
                SetSpeed(1);
            else
                SetSpeed(0);
        }
    }
}
