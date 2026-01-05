using System;

namespace Architecture
{
    [Serializable]
    public class GameSettings
    {
        //0~100
        public int bgmVolume = 100;
        public int sfxVolume = 100;

        public GameResolution gameResolution = GameResolution.Res_3840x2160;
        public GameWindow gameWindow = GameWindow.FullScreenWindow;
    }
}