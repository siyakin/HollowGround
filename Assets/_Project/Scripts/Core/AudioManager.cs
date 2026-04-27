using System.Collections.Generic;
using UnityEngine;

namespace HollowGround.Core
{
    public enum SoundType
    {
        UIClick,
        UIOpen,
        UIClose,
        Notification,
        BuildingPlaced,
        BuildingUpgrade,
        BuildingDemolish,
        TrainingComplete,
        BattleStart,
        BattleWin,
        BattleLose,
        HeroSummon,
        HeroLevelUp,
        QuestComplete,
        ResearchComplete,
        WaveWarning,
        WaveStart,
        WaveDefeated,
        ResourceGather,
        Error
    }

    [CreateAssetMenu(fileName = "AudioConfig", menuName = "HollowGround/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        public List<SoundEntry> Sounds = new();

        [System.Serializable]
        public class SoundEntry
        {
            public SoundType Type;
            public AudioClip Clip;
            [Range(0f, 1f)] public float Volume = 1f;
            public bool Loop;
            [Range(0f, 0.5f)] public float PitchVariation;
        }

        public SoundEntry GetEntry(SoundType type)
        {
            return Sounds.Find(s => s.Type == type);
        }
    }

    public class AudioManager : Singleton<AudioManager>
    {

        [SerializeField] private AudioConfig _config;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private int _sfxPoolSize = 10;

        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 0.5f;
        [Range(0f, 1f)] public float SFXVolume = 0.8f;

        private readonly List<AudioSource> _sfxPool = new();
        private int _poolIndex;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            InitializeSFXPool();
        }

        private void InitializeSFXPool()
        {
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxPool.Add(source);
            }
        }

        private void Update()
        {
            if (_musicSource != null)
                _musicSource.volume = MusicVolume * MasterVolume;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_musicSource == null || clip == null) return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = MusicVolume * MasterVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null)
                _musicSource.Stop();
        }

        public void PlaySFX(SoundType type)
        {
            if (_config == null) return;

            var entry = _config.GetEntry(type);
            if (entry == null || entry.Clip == null) return;

            PlaySFX(entry.Clip, entry.Volume, entry.PitchVariation);
        }

        public void PlaySFX(AudioClip clip, float volume = 1f, float pitchVariation = 0f)
        {
            if (clip == null) return;

            var source = GetNextPooledSource();
            if (source == null) return;

            source.clip = clip;
            source.volume = volume * SFXVolume * MasterVolume;
            source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            source.loop = false;
            source.Play();
        }

        private AudioSource GetNextPooledSource()
        {
            if (_sfxPool.Count == 0) return null;

            var source = _sfxPool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % _sfxPool.Count;
            return source;
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
        }
    }
}
