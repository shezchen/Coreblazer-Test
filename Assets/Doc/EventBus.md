# Event Bus (基于 R3)

本项目使用基于 [R3 (Reactive Extensions for C# v3)](https://github.com/Cysharp/R3) 的事件总线系统。它提供了一种类型安全、高性能且解耦的方式来在不同系统之间传递消息。

## 核心特性

*   **类型安全**：使用 C# 类型系统作为事件的 Key，无需字符串或枚举。
*   **零分配 (Zero Allocation)**：配合 `readonly record struct` 事件定义，极大减少 GC 压力。
*   **响应式编程**：直接返回 `Observable<T>`，可无缝使用 R3 的强大操作符（如过滤、节流、防抖等）。

## 1. 定义事件

为了获得最佳性能和开发体验，推荐使用 `record` 定义事件。
Unity不支持C#10及以上版本的 `record struct`......

```csharp

## 2. 获取 EventBus 实例

`EventBus` 类本身不包含静态单例访问器。它设计为通过依赖注入（DI）容器（如 VContainer）进行管理。

```csharp
// 在 VContainer 的 LifetimeScope 中注册
builder.Register<EventBus>(Lifetime.Singleton);

// 在类中注入
public class PlayerController
{
    private readonly EventBus _eventBus;

    public PlayerController(EventBus eventBus)
    {
        _eventBus = eventBus;
    }
}
```

## 3. 发布事件 (Publish)

使用 `Publish<T>` 方法发送事件。

```csharp
public void TakeDamage(int amount)
{
    // 发送事件
    _eventBus.Publish(new PlayerDamagedEvent(amount, "Enemy"));
}
```

## 4. 订阅事件 (Receive)

使用 `Receive<T>` 方法获取事件流 (`Observable<T>`)。

### 基础订阅

```csharp
using R3;

public class UIManager : MonoBehaviour
{
    [Inject] private EventBus _eventBus;

    void Start()
    {
        _eventBus.Receive<PlayerDamagedEvent>()
            .Subscribe(evt => 
            {
                Debug.Log($"受到伤害: {evt.Damage}");
                UpdateHealthUI(evt.Damage);
            })
            .AddTo(this); // ⚠️ 重要：绑定生命周期，GameObject 销毁时自动取消订阅
    }
}
```

### 高级用法 (R3 操作符)

利用 R3 的操作符可以轻松处理复杂的事件逻辑。

```csharp
_eventBus.Receive<PlayerDamagedEvent>()
    .Where(evt => evt.Damage > 10)          // 过滤：只处理大于 10 的伤害
    .ThrottleFirst(TimeSpan.FromSeconds(0.5f)) // 节流：0.5秒内只处理一次
    .ObserveOn(SynchronizationContext.Current) // 线程切换：确保在主线程执行（如果事件来自后台线程）
    .Subscribe(evt => 
    {
        PlayHurtAnimation();
    })
    .AddTo(this);
```

## 5. 调试

由于使用了 `record struct`，你可以方便地打印事件内容进行调试。

```csharp
_eventBus.Receive<PlayerDamagedEvent>()
    .Do(evt => Debug.Log($"[EventBus] {evt}")) // 输出: PlayerDamagedEvent { Damage = 10, Source = "Enemy" }
    .Subscribe();
```

## 注意事项

1.  **生命周期管理**：务必在订阅链末尾使用 `.AddTo(this)` (对于 MonoBehaviour) 或 `.RegisterTo(disposable)`，否则会导致内存泄漏或空引用异常。
2.  **线程安全**：默认情况下，`EventBus` 不是线程安全的。如果从非主线程发布事件，请在订阅端使用 `.ObserveOn(SynchronizationContext.Current)` 切换回主线程。
