using UnityEngine;
using UnityEngine.Events;

namespace HollowGround.Core
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "HollowGround/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private readonly UnityEvent _event = new();

        public void Raise()
        {
            _event.Invoke();
        }

        public void AddListener(UnityAction listener)
        {
            _event.AddListener(listener);
        }

        public void RemoveListener(UnityAction listener)
        {
            _event.RemoveListener(listener);
        }
    }

    [CreateAssetMenu(fileName = "GameEvent{T}", menuName = "HollowGround/GameEvent (Generic)")]
    public class GameEvent<T> : ScriptableObject
    {
        private readonly UnityEvent<T> _event = new();

        public void Raise(T value)
        {
            _event.Invoke(value);
        }

        public void AddListener(UnityAction<T> listener)
        {
            _event.AddListener(listener);
        }

        public void RemoveListener(UnityAction<T> listener)
        {
            _event.RemoveListener(listener);
        }
    }
}
