using System;
using System.Collections.Generic;
using R3;

namespace Architecture
{
    public class EventBus : IDisposable
    {
        private readonly Dictionary<Type, object> _subjects = new();
        
        /// <summary>
        /// 如果此事件类型无订阅者，会静默失败
        /// </summary>
        /// <param name="message">自定义record事件类型实例</param>
        public void Publish<T>(T message)
        {
            if (_subjects.TryGetValue(typeof(T), out var subjectObj))
            {
                var subject = (Subject<T>)subjectObj;
                subject.OnNext(message);
            }
        }
        
        /// <summary>
        /// 返回一个 Observable&lt;T&gt; 用于订阅事件。
        /// </summary>
        /// <typeparam name="T">事件类型（推荐使用record）</typeparam>
        /// <returns>事件流 Observable</returns>
        /// <example>
        /// <code>
        /// _eventBus.Receive&lt;PlayerDamagedEvent&gt;()
        ///     .Subscribe(evt => 
        ///     {
        ///         Debug.Log($"受到伤害: {evt.Damage}");
        ///     })
        ///     .AddTo(this); // ⚠️ 重要：绑定生命周期
        /// </code>
        /// </example>
        public Observable<T> Receive<T>()
        {
            if (!_subjects.TryGetValue(typeof(T), out var subjectObj))
            {
                var subject = new Subject<T>();
                _subjects[typeof(T)] = subject;
                return subject;
            }

            return (Subject<T>)subjectObj;
        }
        
        public void Dispose()
        {
            foreach (var subjectObj in _subjects.Values)
            {
                if (subjectObj is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _subjects.Clear();
        }
    }
}
//这个EventBus的字典实际上在运行时是只增不减的，如果取消订阅，Subject就是空的，但依然存在于字典中