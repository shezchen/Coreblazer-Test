## GameSound 系统说明

该预制系统通过 Addressables 按需加载音频资源，集中控制 BGM 与 SFX 的播放、淡入淡出、音量和缓存释放。核心逻辑在 `AudioService`，数据由 `AudioCatalog` 配置，播放通道由两个 AudioSource Provider 提供。

### 目录与角色
- `AudioCatalog`：ScriptableObject，维护 BGM/SFX 的 `id -> addressKey` 映射，运行时用此映射解析外部调用的 clipId。
- `AudioProvider/BgmAudioSourceProvider`、`SfxAudioSourceProvider`：对两个 `AudioSource` 的简单封装，便于通过依赖注入传入服务。
- `IAudioService`：播放服务接口，暴露预加载、播放/停止、音量控制、立即停止等能力。
- `AudioService`：接口实现。职责：
	- 按需加载或预加载音频（Addressables），并缓存 `AsyncOperationHandle<AudioClip>`。
	- BGM：支持淡入/淡出切换、循环播放、当前曲目记忆与重复播放短路。
	- SFX：`PlayOneShot` 支持额外 `volumeScale`。
	- 统一音量控制：`SetBgmVolume`、`SetSfxVolume`；`StopAllImmediately`；`Dispose` 时释放已加载句柄。

### 初始化步骤（编辑器）
1) 创建目录 `Assets/AudioCatalog`（或其他位置），右键 `Create/Alkene/Audio/Audio Catalog` 生成 `AudioCatalog.asset`。
2) 在 Inspector 中填写：
	 - `BgmEntries`：`id`（外部调用名），`addressKey`（Addressables 键）。
	 - `SfxEntries`：同上。确保 Addressables 中有对应键。
3) 场景中放置两个 `AudioSource`：
	 - BGM 通道：勾选 `Loop`，其余按需（输出组、空间设置等）。
	 - SFX 通道：用于 `PlayOneShot`。
4) 注册依赖（以 VContainer 为例）：
```csharp
builder.RegisterInstance(catalog); // AudioCatalog asset
builder.RegisterInstance(new BgmAudioSourceProvider(bgmSource));
builder.RegisterInstance(new SfxAudioSourceProvider(sfxSource));
builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
```

### 运行时用法（主要 API）
- 预加载全部：`await audioService.PreloadAllClipsAsync();`（可在场景加载阶段调用）。
- 播放/切换 BGM：`await audioService.PlayBgmAsync("MainTheme", fadeDuration: 0.5f);`
	- 相同曲目重复调用且已在播会被忽略。
	- `fadeDuration` 为 0 时立即切换且以当前音量播放。
- 停止 BGM：`await audioService.StopBgmAsync(fadeDuration: 0.25f);`
- 播放 SFX：`await audioService.PlaySfxAsync("Click", volumeScale: 1f);`
- 音量：`SetBgmVolume(0f~1f)`，`SetSfxVolume(0f~1f)`。
- 立即停掉所有通道：`StopAllImmediately();`

### 核心行为说明
- 地址解析：调用方使用 `clipId`，服务通过 `AudioCatalog` 查表得到 Addressables key；未找到会 `Debug.LogWarning`。
- 缓存策略：按 `clipId` 缓存 handle；加载失败会移除缓存并 `Debug.LogError`。
- 淡入淡出：使用 DOTween；切曲时先淡出当前 BGM，再淡入新曲；停止时可带淡出。
- 生命周期：`Dispose` 释放所有已加载的 Addressables 句柄，避免引用泄漏。

### 最小示例
```csharp
public sealed class UiAudioExample : MonoBehaviour
{
		[Inject] private IAudioService _audio;

		async void Start()
		{
				await _audio.PreloadAllClipsAsync();
				await _audio.PlayBgmAsync("MainTheme", 0.5f);
		}

		public async void OnButtonClick()
		{
				await _audio.PlaySfxAsync("Click", 1f);
		}
}
```

### 调试与常见问题
- 若听不到声音：确认 `AudioCatalog` 的 `id` 与调用字符串一致，Addressables 中存在对应 key，且音量未被调到 0。
- 若淡入淡出无效：检查 DOTween 是否在项目中初始化；确认 `fadeDuration` 非 0。
- 若资源未释放：确保退出时（或场景卸载时）调用 `Dispose`，或让容器销毁该单例以触发释放。
