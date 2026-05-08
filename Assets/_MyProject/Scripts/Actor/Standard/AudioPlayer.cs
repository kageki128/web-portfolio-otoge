using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Audio;

namespace MyProject.Actor
{
    public class AudioPlayer : MonoBehaviour
    {
        const float MinVolume = 0f;
        const float DefaultVolume = 0.5f;
        const float MaxVolume = 1f;
        const float MuteDecibel = -80f;

        public static AudioPlayer Instance { get; private set; }

        public ReadOnlyReactiveProperty<float> BgmVolume => bgmVolume;
        readonly ReactiveProperty<float> bgmVolume = new(DefaultVolume);

        public ReadOnlyReactiveProperty<float> SeVolume => seVolume;
        readonly ReactiveProperty<float> seVolume = new(DefaultVolume);

        [Header("Audio Sources")]
        [SerializeField] AudioSource seAudioSource;
        [SerializeField] AudioSource bgmAudioSource;

        [Header("Audio Mixer")]
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] string bgmVolumeParameter = "BgmVolume";
        [SerializeField] string seVolumeParameter = "SeVolume";

        [Header("Playback Cooldown")]
        [SerializeField, Min(0f)] float clipCooldownSeconds = 0.033f;

        readonly Dictionary<AudioClip, float> clipToPlaybackTime = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                throw new InvalidOperationException($"{nameof(AudioPlayer)} already exists.");
            }

            Instance = this;
            SetBgmVolume(DefaultVolume);
            SetSeVolume(DefaultVolume);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            bgmVolume.Dispose();
            seVolume.Dispose();
        }

        public void PlayBgm(AudioClip clip, bool loop = true)
        {
            if (!CanPlayClip(clip))
            {
                return;
            }

            bgmAudioSource.clip = clip;
            bgmAudioSource.loop = loop;
            bgmAudioSource.Play();
        }

        public void PlaySe(AudioClip clip)
        {
            if (!CanPlayClip(clip))
            {
                return;
            }

            seAudioSource.PlayOneShot(clip);
        }

        public void StopBgm()
        {
            bgmAudioSource.Stop();
        }

        public void SetBgmVolume(float volume)
        {
            var clamped = Mathf.Clamp(volume, MinVolume, MaxVolume);
            bgmVolume.Value = clamped;
            audioMixer.SetFloat(bgmVolumeParameter, ToDecibel(clamped));
        }

        public void SetSeVolume(float volume)
        {
            var clamped = Mathf.Clamp(volume, MinVolume, MaxVolume);
            seVolume.Value = clamped;
            audioMixer.SetFloat(seVolumeParameter, ToDecibel(clamped));
        }

        public void ResetBgmVolume()
        {
            SetBgmVolume(DefaultVolume);
        }

        public void ResetSeVolume()
        {
            SetSeVolume(DefaultVolume);
        }

        static float ToDecibel(float volume)
        {
            if (volume <= MinVolume)
            {
                return MuteDecibel;
            }

            return 20f * Mathf.Log10(volume / DefaultVolume);
        }

        bool CanPlayClip(AudioClip clip)
        {
            var now = Time.unscaledTime;

            if (clipToPlaybackTime.TryGetValue(clip, out var nextPlayableTime) && now < nextPlayableTime)
            {
                return false;
            }

            clipToPlaybackTime[clip] = now + clipCooldownSeconds;
            return true;
        }
    }
}
