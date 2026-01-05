using UnityEngine;

namespace Architecture.GameSound.AudioProvider
{
    public interface IBgmAudioSourceProvider
    {
        AudioSource Source { get; }
    }

    public sealed class BgmAudioSourceProvider : IBgmAudioSourceProvider
    {
        public AudioSource Source { get; }

        public BgmAudioSourceProvider(AudioSource source)
        {
            Source = source;
        }
    }
}
