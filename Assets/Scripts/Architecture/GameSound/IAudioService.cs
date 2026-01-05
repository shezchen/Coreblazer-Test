using Cysharp.Threading.Tasks;

namespace Architecture.GameSound
{
    public interface IAudioService
    {
        UniTask PreloadAllClipsAsync();

        UniTask PlayBgmAsync(string clipId, float fadeDuration = 0.5f);
        UniTask StopBgmAsync(float fadeDuration = 0.25f);

        UniTask PlaySfxAsync(string clipId, float volumeScale = 1f);

        void SetBgmVolume(float normalizedVolume);
        void SetSfxVolume(float normalizedVolume);

        void StopAllImmediately();
    }
}
