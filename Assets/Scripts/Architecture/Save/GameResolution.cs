namespace Architecture
{
    /// <summary>
    /// 游戏支持的主流分辨率列表
    /// </summary>
    public enum GameResolution
    {
        // --- 16:9 (标准宽屏) ---
        /// <summary> HD Ready - 低配机型/掌机模式常用 </summary>
        Res_1280x720,
    
        /// <summary> 笔记本电脑常见分辨率 </summary>
        Res_1366x768,
    
        /// <summary> 900p - 性能与画质的折中 </summary>
        Res_1600x900,
    
        /// <summary> FHD - 目前最主流分辨率 (Steam占比约60%) </summary>
        Res_1920x1080,
    
        /// <summary> 2K/QHD - 进阶玩家首选，增长最快 </summary>
        Res_2560x1440,
    
        /// <summary> 4K/UHD - 高端显卡体验 </summary>
        Res_3840x2160,

        // --- 16:10 (生产力屏 / Steam Deck / Apple) ---
        /// <summary> Steam Deck 原生分辨率 </summary>
        Res_1280x800,
    
        /// <summary> 1200p - 常见于办公显示器 </summary>
        Res_1920x1200,
    
        /// <summary> MacBook Pro / 高端 16:10 笔记本 </summary>
        Res_2560x1600,
    }
}