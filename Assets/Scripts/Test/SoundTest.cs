using Architecture.GameSound;
using Sirenix.OdinInspector;
using VContainer;

namespace Test
{
    public class SoundTest : SerializedMonoBehaviour
    {
        [Inject] private IAudioService _audioService;

        [Button("播放测试BGM")]
        public void PlayTestBGM()
        {
            _audioService.PlayBgmAsync("TestBGM");
        }
        
        [Button("播放测试SFX")]
        public void PlayTestSFX()
        {
            _audioService.PlaySfxAsync("TestSFX");
        }
    }
}
