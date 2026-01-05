using System;
using System.Collections.Generic;
using Architecture.GameSound.AudioProvider;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Architecture.GameSound
{
    /// <summary>
    /// Addressables-driven audio service. It loads clips on demand, caches handles, and keeps play logic centralized.
    /// Register it as a singleton in VContainer and inject the two AudioSources plus catalog asset.
    /// </summary>
    public sealed class AudioService : IAudioService, IDisposable
    {
        private readonly BgmAudioSourceProvider _bgmProvider;
        private readonly SfxAudioSourceProvider _sfxProvider;
        private readonly AudioCatalog _catalog;
        private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _loadedClipHandles = new();

        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private string _currentBgmId;
        private Tween _bgmFadeTween;

        public AudioService(AudioCatalog catalog, BgmAudioSourceProvider bgmProvider, SfxAudioSourceProvider sfxProvider)
        {
            _catalog = catalog;
            _bgmProvider = bgmProvider;
            _sfxProvider = sfxProvider;
            _bgmProvider.Source.loop = true;
        }

        public async UniTask PreloadAllClipsAsync()
        {
            await PreloadLookup(_catalog.BGM);
            await PreloadLookup(_catalog.SFX);
        }

        public async UniTask PlayBgmAsync(string clipId, float fadeDuration = 0.5f)
        {
            if (!_catalog.TryGetBGM(clipId, out var addressKey))
            {
                Debug.LogWarning($"Unknown BGM id: {clipId}");
                return;
            }

            var clip = await LoadClipAsync(clipId, addressKey);
            if (clip == null)
            {
                return;
            }

            _bgmFadeTween?.Kill();

            if (_bgmProvider.Source.isPlaying && _currentBgmId == clipId)
            {
                return;
            }

            _currentBgmId = clipId;

            if (_bgmProvider.Source.isPlaying && fadeDuration > 0f)
            {
                _bgmFadeTween = DOTween.Sequence()
                    .Append(_bgmProvider.Source.DOFade(0f, fadeDuration * 0.5f))
                    .AppendCallback(() =>
                    {
                        _bgmProvider.Source.clip = clip;
                        _bgmProvider.Source.volume = 0f;
                        _bgmProvider.Source.Play();
                    })
                    .Append(_bgmProvider.Source.DOFade(_bgmVolume, fadeDuration * 0.5f));
            }
            else
            {
                _bgmProvider.Source.clip = clip;
                _bgmProvider.Source.volume = fadeDuration > 0f ? 0f : _bgmVolume;
                _bgmProvider.Source.Play();
                if (fadeDuration > 0f)
                {
                    _bgmFadeTween = _bgmProvider.Source.DOFade(_bgmVolume, fadeDuration);
                }
            }
        }

        public async UniTask StopBgmAsync(float fadeDuration = 0.25f)
        {
            _currentBgmId = null;

            if (!_bgmProvider.Source.isPlaying)
            {
                return;
            }

            _bgmFadeTween?.Kill();

            if (fadeDuration > 0f)
            {
                _bgmFadeTween = _bgmProvider.Source.DOFade(0f, fadeDuration)
                    .OnComplete(() => _bgmProvider.Source.Stop());
                await _bgmFadeTween.AsyncWaitForCompletion();
            }
            else
            {
                _bgmProvider.Source.Stop();
            }
        }

        public async UniTask PlaySfxAsync(string clipId, float volumeScale = 1f)
        {
            if (!_catalog.TryGetSFX(clipId, out var addressKey))
            {
                Debug.LogWarning($"Unknown SFX id: {clipId}");
                return;
            }

            var clip = await LoadClipAsync(clipId, addressKey);
            if (clip == null)
            {
                return;
            }

            _sfxProvider.Source.PlayOneShot(clip, _sfxVolume * volumeScale);
        }

        public void SetBgmVolume(float normalizedVolume)
        {
            _bgmVolume = Mathf.Clamp01(normalizedVolume);
            Debug.Log("BGM volume set to " + _bgmVolume);
            if (_bgmProvider.Source.isPlaying)
            {
                _bgmProvider.Source.volume = _bgmVolume;
            }
        }

        public void SetSfxVolume(float normalizedVolume)
        {
            _sfxVolume = Mathf.Clamp01(normalizedVolume);
            Debug.Log("SFX volume set to " + _sfxVolume);
        }

        public void StopAllImmediately()
        {
            _bgmFadeTween?.Kill();
            _bgmProvider.Source.Stop();
            _sfxProvider.Source.Stop();
        }

        public void Dispose()
        {
            _bgmFadeTween?.Kill();

            foreach (var handle in _loadedClipHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _loadedClipHandles.Clear();
        }

        private async UniTask<AudioClip> LoadClipAsync(string clipId, string addressKey)
        {
            if (_loadedClipHandles.TryGetValue(clipId, out var cachedHandle))
            {
                return cachedHandle.Status == AsyncOperationStatus.Succeeded ? cachedHandle.Result : null;
            }

            var handle = Addressables.LoadAssetAsync<AudioClip>(addressKey);
            _loadedClipHandles[clipId] = handle;

            await handle.Task.AsUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load clip {clipId} from key {addressKey}");
                _loadedClipHandles.Remove(clipId);
                return null;
            }

            return handle.Result;
        }

        private UniTask PreloadLookup(IReadOnlyDictionary<string, string> lookup)
        {
            var taskList = new List<UniTask<AudioClip>>();
            foreach (var pair in lookup)
            {
                taskList.Add(LoadClipAsync(pair.Key, pair.Value));
            }
            return UniTask.WhenAll(taskList);
        }
    }
}
