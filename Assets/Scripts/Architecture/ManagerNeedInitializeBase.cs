using Cysharp.Threading.Tasks;

namespace Architecture
{
    /// <summary>
    /// 需要初始化的Manager基类
    /// </summary>
    public class ManagerNeedInitializeBase
    {
        public bool IsInitialized;

        public virtual UniTask Init()
        {
            IsInitialized = true;
            return UniTask.CompletedTask;
        }
    }
}