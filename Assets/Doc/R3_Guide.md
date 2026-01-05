# R3 for Unity: 极简实战指南

## 0. 前言：为什么要用 R3？

想象一下，你正在写一个“玩家受伤扣血”的功能。

**传统写法 (命令式)：**
你需要在 `TakeDamage` 函数里：
1. 扣减血量变量。
2. 查找 UI 组件。
3. 更新 UI 文本。
4. 检查是否死亡。
5. 如果死亡，播放音效，播放动画...

代码越写越长，逻辑分散在各个角落。

**R3 写法 (响应式)：**
你只需要定义“血量”这个**数据流**。
*   UI 监听血量：只要变了，我就更新文字。
*   音效系统监听血量：只要归零，我就放声音。

**核心思想**：不要去“命令”别人做什么，而是**发布数据变化**，让关心这件事的人自己去“响应”。

---

## 1. 核心概念：水管与水流

在 R3 中，一切皆可视为**水流 (Stream)**。

*   **Observable (可观察对象)** = **水管**。水从这里流出来。
*   **Subscribe (订阅)** = **接水管**。只有接上了水管，水才会流出来，你才能处理它。
*   **Operator (操作符)** = **滤网/阀门**。你可以过滤脏水（Where），或者把水变成蒸汽（Select）。

---

## 2. 最常用的功能：响应式变量 (ReactiveProperty)

这是 R3 最基础也最强大的功能。它把一个普通的变量变成了一个“会通知别人的变量”。

### 2.1 基础用法

```csharp
using R3;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    // 1. 定义一个响应式变量，初始值为 100
    // SerializableReactiveProperty 可以在 Inspector 面板里直接看到和修改！
    public SerializableReactiveProperty<int> CurrentHp = new(100);

    public Text hpText;

    void Start()
    {
        // 2. 订阅变化 (接水管)
        // 只要 CurrentHp 的值发生变化，这里的代码就会执行
        // 注意：Start 的时候会默认执行一次，初始化 UI
        CurrentHp.Subscribe(hp => 
        {
            hpText.text = $"HP: {hp}";
            
            if (hp <= 0)
            {
                Debug.Log("玩家死亡！");
            }
        })
        .AddTo(this); // 3. 【重要】绑定生命周期 (详见第5章)
    }

    public void TakeDamage(int amount)
    {
        // 4. 修改数值
        // 你只需要改值，上面的 Subscribe 逻辑会自动触发！
        CurrentHp.Value -= amount;
    }
}
```

### 2.2 为什么好用？
*   **解耦**：`TakeDamage` 不需要知道 UI 的存在。
*   **省事**：不需要在 `Start` 里手动写一遍 `hpText.text = ...`，订阅时会自动同步一次。

---

## 3. 替代 Update：事件流

以前我们需要在 `Update` 里每帧检查按键或状态，现在我们可以把 `Update` 看作一个每帧滴水的水龙头。

### 3.1 简单的每帧执行

```csharp
void Start()
{
    // 替代 void Update()
    Observable.EveryUpdate()
        .Subscribe(_ => 
        {
            transform.Rotate(0, 1, 0);
        })
        .AddTo(this);
}
```

### 3.2 强大的过滤 (Where)

比如：只有当按下空格键时，才旋转。

```csharp
Observable.EveryUpdate()
    .Where(_ => Input.GetKey(KeyCode.Space)) // 过滤器：只有条件为真，水流才能通过
    .Subscribe(_ => 
    {
        transform.Rotate(0, 5, 0);
    })
    .AddTo(this);
```

---

## 4. UI 交互与防抖 (Throttle)

处理按钮点击时，我们经常遇到“玩家狂点按钮导致发送多次请求”的问题。R3 解决这个问题只需要一行代码。

```csharp
public Button attackButton;

void Start()
{
    // 将按钮点击转换为水流
    attackButton.OnClickAsObservable()
        .ThrottleFirst(System.TimeSpan.FromSeconds(0.5f)) // 【防抖阀门】
        // 解释：一旦水流通过，阀门关闭 0.5秒，期间的水流全部丢弃。
        .Subscribe(_ => 
        {
            Debug.Log("发动攻击！(0.5秒内只能触发一次)");
        })
        .AddTo(this);
}
```

---

## 5. 黄金法则：生命周期管理 (.AddTo)

这是新手最容易踩坑的地方。

**问题**：当你销毁一个 GameObject 时，C# 的对象（引用）并不会立即消失。如果你订阅了事件但没有取消，当事件再次触发时，代码会试图去访问已经销毁的物体，导致报错 `MissingReferenceException` 或内存泄漏。

**解决**：R3 提供了 `.AddTo(this)`。

```csharp
// 错误写法 ❌
Observable.Interval(System.TimeSpan.FromSeconds(1))
    .Subscribe(_ => Debug.Log("Tick")); 
// 即使物体销毁了，这个 Log 还会一直打印，直到游戏关闭！

// 正确写法 ✅
Observable.Interval(System.TimeSpan.FromSeconds(1))
    .Subscribe(_ => Debug.Log("Tick"))
    .AddTo(this); 
// 告诉 R3：这个订阅是属于 "this" (当前脚本) 的。
// 如果 "this" 被销毁了，请自动切断这个订阅。
// 也可以绑定到 destroyCancellationToken
```

**口诀：写完 Subscribe，必须接 AddTo。**

---

## 6. 进阶：R3 + UniTask (异步处理)

当事件触发后，如果你需要做一些耗时操作（比如等待动画、网络请求），可以使用 `SubscribeAwait`。

```csharp
public Button loginButton;

void Start()
{
    loginButton.OnClickAsObservable()
        // AwaitOperation.Drop: 如果上一次登录还没结束，新的点击直接忽略
        .SubscribeAwait(async (_, ct) => 
        {
            Debug.Log("开始登录...");
            // 这里可以像平时一样写 await
            await UniTask.Delay(2000, cancellationToken: ct); 
            Debug.Log("登录成功！");
        }, AwaitOperation.Drop)
        .AddTo(this);
}
```

---

## 7. 实战：全局事件总线 (Event Bus)

用 R3 替换传统的 C# `event` 或 `UnityEvent`。

**1. 定义事件中心**
```csharp
public static class GameEvents
{
    // Subject 是一个既能"发消息"又能"收消息"的中转站
    public static readonly Subject<string> OnGameStart = new();
    public static readonly Subject<int> OnScoreChanged = new();
}
```

**2. 发送事件**
```csharp
// 在游戏管理器里
GameEvents.OnGameStart.OnNext("Level_1"); // 发送消息
GameEvents.OnScoreChanged.OnNext(100);
```

**3. 接收事件**
```csharp
// 在任何脚本里
void Start()
{
    GameEvents.OnScoreChanged
        .Where(score => score > 1000) // 甚至可以加过滤
        .Subscribe(score => 
        {
            Debug.Log("高分达成！");
        })
        .AddTo(this);
}
```

---

## 8. 常用操作符速查表

| 操作符 | 作用 | 场景 |
| :--- | :--- | :--- |
| **Where** | 过滤 | 只处理满足条件的数据（如：血量<0） |
| **Select** | 转换 | 把数据变个样（如：把 int 变成 string） |
| **ThrottleFirst** | 节流/防抖 | 防止按钮连点，限制触发频率 |
| **Debounce** | 防抖 | 停止输入一段时间后才触发（如：搜索框输入停止后才搜索） |
| **Skip** | 跳过 | 跳过前 N 次数据（如：跳过初始化的那次赋值） |
| **Take** | 只取 N 次 | 比如只处理第一次点击，之后自动断开 |
| **CombineLatest** | 组合 | 当 A 和 B 都有值时，把它们最新的值组合起来 |
