using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tools
{
    /// <summary>
    /// 基于 DOTween 的常用 2D/UGUI 动画扩展方法集合。
    /// 约定：当扩展目标为 null 时返回 null（或不执行）。
    /// <br/>Ease.OutQuad: 最常用、最通用的缓出，效果自然。
    /// <br/>Ease.OutBack: 适合需要“弹出感”的UI元素，如窗口、按钮出现。
    /// <br/>Ease.InCubic / Ease.OutCubic: 比 Quad 更平滑一些。
    /// <br/>Ease.Linear: 适用于需要匀速滚动的背景或进度条。
    /// </summary>
    public static class DOTweenTool
    {
        // ---------- Common options ----------
        /// <summary>
        /// 为 Tween 统一设置常用选项：目标对象、缓动类型以及是否独立于 Time.timeScale 更新。
        /// </summary>
        /// <param name="t">要设置的 Tween 对象。如果为 null 将直接返回 null。</param>
        /// <param name="target">Tween 的目标对象（用于 DOTween.Kill/Complete 按目标操作）。</param>
        /// <param name="ease">缓动类型，默认 <see cref="Ease.OutQuad"/>。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新（暂停时仍播放）。</param>
        /// <returns>设置后的 Tween；如果传入为 null 则返回 null。</returns>
        private static Tween SetCommon(this Tween t, object target = null, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
        {
            if (t == null) return null;
            if (target != null) t.SetTarget(target);
            t.SetEase(ease);
            if (independentUpdate) t.SetUpdate(true);
            return t;
        }

        // ---------- Transform (2D) ----------
        /// <summary>
        /// 将 Transform 在世界坐标系中移动到指定 2D 位置（保持原有 z）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="worldPos">目标世界坐标（x、y）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="snapping">是否启用像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动的 Tweener。</returns>
        public static Tweener MoveTo(this Transform t, Vector2 worldPos, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null
                ? null
                : t.DOMove(new Vector3(worldPos.x, worldPos.y, t.position.z), duration, snapping)
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 将 Transform 在世界坐标系中按 2D 增量位移（相对移动）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="delta">相对位移（x、y）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="snapping">是否启用像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动的 Tweener（相对）。</returns>
        public static Tweener MoveBy(this Transform t, Vector2 delta, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null
                ? null
                : t.DOMove(new Vector3(delta.x, delta.y, 0f), duration, snapping)
                    .SetRelative()
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 沿世界坐标 X 轴移动到指定位置。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="x">目标世界 X 坐标。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="snapping">是否启用像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动的 Tweener。</returns>
        public static Tweener MoveX(this Transform t, float x, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null ? null : t.DOMoveX(x, duration, snapping).SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 沿世界坐标 Y 轴移动到指定位置。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="y">目标世界 Y 坐标。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="snapping">是否启用像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动的 Tweener。</returns>
        public static Tweener MoveY(this Transform t, float y, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null ? null : t.DOMoveY(y, duration, snapping).SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 在本地坐标系中移动到指定 2D 位置（保持原有 z）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="localPos">目标本地坐标（x、y）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="snapping">是否启用像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动的 Tweener。</returns>
        public static Tweener LocalMoveTo(this Transform t, Vector2 localPos, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null
                ? null
                : t.DOLocalMove(new Vector3(localPos.x, localPos.y, t.localPosition.z), duration, snapping)
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 在本地坐标系中按 2D 增量位移（相对移动）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="delta">相对位移（x、y）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="snapping">是否启用像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动的 Tweener（相对）。</returns>
        public static Tweener LocalMoveBy(this Transform t, Vector2 delta, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null
                ? null
                : t.DOLocalMove(new Vector3(delta.x, delta.y, 0f), duration, snapping)
                    .SetRelative()
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 沿 Z 轴旋转到指定角度（度）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="zDegrees">目标 Z 角（度）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="mode">旋转模式。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>旋转的 Tweener。</returns>
        public static Tweener RotateZ(this Transform t, float zDegrees, float duration,
            RotateMode mode = RotateMode.Fast, Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null
                ? null
                : t.DORotate(new Vector3(0, 0, zDegrees), duration, mode)
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 沿 Z 轴按增量旋转（相对旋转）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="deltaZDegrees">增量 Z 角（度）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>旋转的 Tweener（相对）。</returns>
        public static Tweener RotateByZ(this Transform t, float deltaZDegrees, float duration, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
            => t == null
                ? null
                : t.DORotate(new Vector3(0, 0, deltaZDegrees), duration)
                    .SetRelative()
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 按统一比例缩放到指定数值。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="uniformScale">目标统一缩放值。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="ease">缓动类型，默认 <see cref="Ease.OutBack"/>。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>缩放的 Tweener。</returns>
        public static Tweener ScaleTo(this Transform t, float uniformScale, float duration, Ease ease = Ease.OutBack,
            bool independentUpdate = false)
            => t == null ? null : t.DOScale(uniformScale, duration).SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 按 x、y 各自比例缩放到指定向量（保持原有 z 缩放）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="xy">目标缩放（x、y）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="ease">缓动类型，默认 <see cref="Ease.OutBack"/>。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>缩放的 Tweener。</returns>
        public static Tweener ScaleTo(this Transform t, Vector2 xy, float duration, Ease ease = Ease.OutBack,
            bool independentUpdate = false)
            => t == null
                ? null
                : t.DOScale(new Vector3(xy.x, xy.y, t.localScale.z), duration)
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 从指定缩放值过渡到当前缩放（From 动画）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="fromXY">起始缩放（x、y）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="ease">缓动类型，默认 <see cref="Ease.OutBack"/>。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>缩放的 Tweener。</returns>
        public static Tweener ScaleFrom(this Transform t, Vector2 fromXY, float duration, Ease ease = Ease.OutBack,
            bool independentUpdate = false)
            => t == null
                ? null
                : t.DOScale(new Vector3(t.localScale.x, t.localScale.y, t.localScale.z), duration)
                    .From(new Vector3(fromXY.x, fromXY.y, t.localScale.z))
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 以“拳击”方式弹跳缩放（短促的强度型缩放）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="punch">弹力向量（x、y）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="vibrato">振动次数。</param>
        /// <param name="elasticity">弹性强度。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>缩放弹跳 Tweener。</returns>
        public static Tweener ScalePunch(this Transform t, Vector2 punch, float duration = 0.2f, int vibrato = 8,
            float elasticity = 1f, Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => t == null
                ? null
                : t.DOPunchScale(new Vector3(punch.x, punch.y, 0f), duration, vibrato, elasticity)
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 2D 抖动位置（仅 x、y）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="strength">抖动强度（x、y）。为 null 时使用默认值 (0.5, 0.5)。</param>
        /// <param name="vibrato">振动次数。</param>
        /// <param name="randomness">随机度（角度）。</param>
        /// <param name="fadeOut">是否在尾部渐弱。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>位置抖动 Tweener。</returns>
        public static Tweener ShakePosition2D(this Transform t, float duration = 0.4f, Vector2? strength = null,
            int vibrato = 10, float randomness = 90f, bool fadeOut = true, Ease ease = Ease.Linear,
            bool independentUpdate = false)
        {
            if (t == null) return null;
            var s = strength ?? new Vector2(0.5f, 0.5f);
            return t.DOShakePosition(duration, new Vector3(s.x, s.y, 0f), vibrato, randomness, fadeOut)
                .SetCommon(t, ease, independentUpdate) as Tweener;
        }

        /// <summary>
        /// 沿 Z 轴抖动旋转。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="strength">Z 轴抖动强度（度）。</param>
        /// <param name="vibrato">振动次数。</param>
        /// <param name="randomness">随机度（角度）。</param>
        /// <param name="fadeOut">是否在尾部渐弱。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>旋转抖动 Tweener。</returns>
        public static Tweener ShakeRotationZ(this Transform t, float duration = 0.3f, float strength = 15f,
            int vibrato = 10, float randomness = 90f, bool fadeOut = true, Ease ease = Ease.Linear,
            bool independentUpdate = false)
            => t == null
                ? null
                : t.DOShakeRotation(duration, new Vector3(0, 0, strength), vibrato, randomness, fadeOut)
                    .SetCommon(t, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 抖动缩放。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="strength">抖动强度（x、y）。为 null 时使用默认值 (0.2, 0.2)。</param>
        /// <param name="vibrato">振动次数。</param>
        /// <param name="randomness">随机度（角度）。</param>
        /// <param name="fadeOut">是否在尾部渐弱。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>缩放抖动 Tweener。</returns>
        public static Tweener ShakeScale(this Transform t, float duration = 0.3f, Vector2? strength = null,
            int vibrato = 10, float randomness = 90f, bool fadeOut = true, Ease ease = Ease.Linear,
            bool independentUpdate = false)
        {
            if (t == null) return null;
            var s = strength ?? new Vector2(0.2f, 0.2f);
            return t.DOShakeScale(duration, new Vector3(s.x, s.y, 0f), vibrato, randomness, fadeOut)
                .SetCommon(t, ease, independentUpdate) as Tweener;
        }

        // Handy flips (by scaling sign)
        /// <summary>
        /// 沿 X 轴翻转（通过改变缩放符号）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <returns>缩放 Tweener。</returns>
        public static Tweener FlipX(this Transform t, float duration = 0.15f, Ease ease = Ease.OutQuad)
        {
            if (t == null) return null;
            float end = -t.localScale.x;
            return t.DOScaleX(end, duration).SetCommon(t, ease) as Tweener;
        }

        /// <summary>
        /// 沿 Y 轴翻转（通过改变缩放符号）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <returns>缩放 Tweener。</returns>
        public static Tweener FlipY(this Transform t, float duration = 0.15f, Ease ease = Ease.OutQuad)
        {
            if (t == null) return null;
            float end = -t.localScale.y;
            return t.DOScaleY(end, duration).SetCommon(t, ease) as Tweener;
        }

        // ---------- Rigidbody2D ----------
        /// <summary>
        /// 将 Rigidbody2D 移动到指定世界坐标（2D）。
        /// </summary>
        /// <param name="rb">目标刚体。</param>
        /// <param name="worldPos">目标世界坐标。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动 Tweener。</returns>
        public static Tweener MoveTo(this Rigidbody2D rb, Vector2 worldPos, float duration, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
            => rb == null
                ? null
                : rb.DOMove(worldPos, duration)
                    .SetCommon(rb, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 将 Rigidbody2D 按增量移动（相对）。
        /// </summary>
        /// <param name="rb">目标刚体。</param>
        /// <param name="delta">相对位移。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动 Tweener（相对）。</returns>
        public static Tweener MoveBy(this Rigidbody2D rb, Vector2 delta, float duration, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
            => rb == null
                ? null
                : rb.DOMove(delta, duration)
                    .SetRelative()
                    .SetCommon(rb, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 2D 跳跃（带可选多段）。
        /// </summary>
        /// <param name="rb">目标刚体。</param>
        /// <param name="end">落点坐标。</param>
        /// <param name="jumpPower">跳跃高度。</param>
        /// <param name="numJumps">跳跃段数。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="snapping">是否像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>跳跃 Tweener。</returns>
        public static Tweener Jump2D(this Rigidbody2D rb, Vector2 end, float jumpPower = 2f, int numJumps = 1,
            float duration = 0.5f, bool snapping = false, Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => rb == null
                ? null
                : rb.DOJump(end, jumpPower, numJumps, duration, snapping)
                    .SetCommon(rb, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 旋转 Rigidbody2D 到指定 Z 角（度）。
        /// </summary>
        /// <param name="rb">目标刚体。</param>
        /// <param name="zDegrees">目标角度（度）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>旋转 Tweener。</returns>
        public static Tweener RotateToZ(this Rigidbody2D rb, float zDegrees, float duration, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
            => rb == null ? null : rb.DORotate(zDegrees, duration).SetCommon(rb, ease, independentUpdate) as Tweener;

        // ---------- RectTransform (UI) ----------
        /// <summary>
        /// 将 UI 锚点位置移动到指定值。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="anchoredPos">目标锚点位置。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="snapping">是否像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动 Tweener。</returns>
        public static Tweener AnchorTo(this RectTransform rt, Vector2 anchoredPos, float duration,
            bool snapping = false, Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => rt == null
                ? null
                : rt.DOAnchorPos(anchoredPos, duration, snapping)
                    .SetCommon(rt, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 按增量改变 UI 锚点位置（相对）。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="delta">相对位移。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="snapping">是否像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动 Tweener（相对）。</returns>
        public static Tweener AnchorBy(this RectTransform rt, Vector2 delta, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => rt == null
                ? null
                : rt.DOAnchorPos(delta, duration, snapping)
                    .SetRelative()
                    .SetCommon(rt, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 将 UI 锚点 X 移动到指定值。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="x">目标锚点 X。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="snapping">是否像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动 Tweener。</returns>
        public static Tweener AnchorX(this RectTransform rt, float x, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => rt == null
                ? null
                : rt.DOAnchorPosX(x, duration, snapping).SetCommon(rt, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 将 UI 锚点 Y 移动到指定值。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="y">目标锚点 Y。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="snapping">是否像素对齐。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动 Tweener。</returns>
        public static Tweener AnchorY(this RectTransform rt, float y, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => rt == null
                ? null
                : rt.DOAnchorPosY(y, duration, snapping).SetCommon(rt, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 变更 UI 的 SizeDelta 到指定大小。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="sizeDelta">目标 SizeDelta。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>尺寸 Tweener。</returns>
        public static Tweener SizeTo(this RectTransform rt, Vector2 sizeDelta, float duration, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
            => rt == null
                ? null
                : rt.DOSizeDelta(sizeDelta, duration).SetCommon(rt, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 过渡 UI pivot（枢轴）到指定值。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="pivot">目标 pivot。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>pivot Tweener。</returns>
        public static Tweener PivotTo(this RectTransform rt, Vector2 pivot, float duration, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
            => rt == null
                ? null
                : DOTween.To(() => rt.pivot, v => rt.pivot = v, pivot, duration)
                    .SetCommon(rt, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 抖动 UI 的锚点位置。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="strength">抖动强度（x、y），为 null 使用默认 (20,20)。</param>
        /// <param name="vibrato">振动次数。</param>
        /// <param name="randomness">随机度（角度）。</param>
        /// <param name="fadeOut">是否在尾部渐弱。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>锚点抖动 Tweener。</returns>
        public static Tweener ShakeAnchored(this RectTransform rt, float duration = 0.3f, Vector2? strength = null,
            int vibrato = 10, float randomness = 90f, bool fadeOut = true, Ease ease = Ease.Linear,
            bool independentUpdate = false)
        {
            if (rt == null) return null;
            var s = strength ?? new Vector2(20f, 20f);
            return rt.DOShakeAnchorPos(duration, s, vibrato, randomness, fadeOut)
                .SetCommon(rt, ease, independentUpdate) as Tweener;
        }

        /// <summary>
        /// 变更 Image 的 fillAmount 到目标值（自动 0-1 截断）。
        /// </summary>
        /// <param name="img">目标 Image。</param>
        /// <param name="fillAmount">目标填充（0-1）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>填充 Tweener。</returns>
        public static Tweener FillTo(this Image img, float fillAmount, float duration, Ease ease = Ease.Linear,
            bool independentUpdate = false)
            => img == null
                ? null
                : img.DOFillAmount(Mathf.Clamp01(fillAmount), duration)
                    .SetCommon(img, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 变更 Slider 的 value。
        /// </summary>
        /// <param name="slider">目标 Slider。</param>
        /// <param name="value">目标值。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="snapping">是否像素对齐（仅对整数滑动有意义）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>数值 Tweener。</returns>
        public static Tweener ValueTo(this Slider slider, float value, float duration, bool snapping = false,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => slider == null
                ? null
                : slider.DOValue(value, duration, snapping).SetCommon(slider, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 横向滚动 ScrollRect 到指定归一化位置（0-1）。
        /// </summary>
        /// <param name="sr">目标 ScrollRect。</param>
        /// <param name="normalized">归一化位置（0-1）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>滚动 Tweener。</returns>
        public static Tweener HorizontalScrollTo(this ScrollRect sr, float normalized, float duration,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => sr == null
                ? null
                : sr.DOHorizontalNormalizedPos(Mathf.Clamp01(normalized), duration)
                    .SetCommon(sr, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 纵向滚动 ScrollRect 到指定归一化位置（0-1）。
        /// </summary>
        /// <param name="sr">目标 ScrollRect。</param>
        /// <param name="normalized">归一化位置（0-1）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>滚动 Tweener。</returns>
        public static Tweener VerticalScrollTo(this ScrollRect sr, float normalized, float duration,
            Ease ease = Ease.OutQuad, bool independentUpdate = false)
            => sr == null
                ? null
                : sr.DOVerticalNormalizedPos(Mathf.Clamp01(normalized), duration)
                    .SetCommon(sr, ease, independentUpdate) as Tweener;

        // ---------- SpriteRenderer (2D sprites) ----------
        /// <summary>
        /// 渐变 SpriteRenderer 的不透明度（alpha）。
        /// </summary>
        /// <param name="sr">目标 SpriteRenderer。</param>
        /// <param name="alpha">目标 alpha（0-1）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeTo(this SpriteRenderer sr, float alpha, float duration, Ease ease = Ease.Linear,
            bool independentUpdate = false)
            => sr == null ? null : sr.DOFade(alpha, duration).SetCommon(sr, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 渐变 SpriteRenderer 的颜色。
        /// </summary>
        /// <param name="sr">目标 SpriteRenderer。</param>
        /// <param name="color">目标颜色。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>颜色 Tweener。</returns>
        public static Tweener ColorTo(this SpriteRenderer sr, Color color, float duration, Ease ease = Ease.Linear,
            bool independentUpdate = false)
            => sr == null ? null : sr.DOColor(color, duration).SetCommon(sr, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 颜色闪烁：先变为指定颜色，再回到原色。
        /// </summary>
        /// <param name="sr">目标 SpriteRenderer。</param>
        /// <param name="flashColor">闪烁颜色。</param>
        /// <param name="halfDuration">往返中每一段的时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>闪烁 Sequence。</returns>
        public static Sequence Flash(this SpriteRenderer sr, Color flashColor, float halfDuration = 0.1f,
            Ease ease = Ease.Linear, bool independentUpdate = false)
        {
            if (sr == null) return null;
            var original = sr.color;
            return DOTween.Sequence()
                .Append(sr.DOColor(flashColor, halfDuration))
                .Append(sr.DOColor(original, halfDuration))
                .SetCommon(sr, ease, independentUpdate) as Sequence;
        }

        // ---------- UI Graphic / CanvasGroup ----------
        /// <summary>
        /// 渐变 UI Graphic 的不透明度（alpha）。
        /// </summary>
        /// <param name="g">目标 Graphic。</param>
        /// <param name="alpha">目标 alpha（0-1）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeTo(this Graphic g, float alpha, float duration, Ease ease = Ease.Linear,
            bool independentUpdate = false)
            => g == null ? null : g.DOFade(alpha, duration).SetCommon(g, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 渐变 UI Graphic 的颜色。
        /// </summary>
        /// <param name="g">目标 Graphic。</param>
        /// <param name="color">目标颜色。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>颜色 Tweener。</returns>
        public static Tweener ColorTo(this Graphic g, Color color, float duration, Ease ease = Ease.Linear,
            bool independentUpdate = false)
            => g == null ? null : g.DOColor(color, duration).SetCommon(g, ease, independentUpdate) as Tweener;

        /// <summary>
        /// 打字机效果地改变 <see cref="TMP_Text"/> 的内容。
        /// </summary>
        /// <param name="txt">目标 TMP_Text。</param>
        /// <param name="content">目标文本内容。</param>
        /// <param name="duration">打字时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>文本 Tweener。</returns>
        public static Tweener TypeText(this TMP_Text txt, string content, float duration, Ease ease = Ease.Linear,
            bool independentUpdate = false)
        {
            if (txt == null) return null;

            content ??= string.Empty;
            txt.text = content;
            txt.ForceMeshUpdate();
            int totalCharacters = Mathf.Max(0, txt.textInfo.characterCount);
            txt.maxVisibleCharacters = 0;

            var tween = DOTween.To(() => 0, v =>
                txt.maxVisibleCharacters = Mathf.Clamp(Mathf.RoundToInt(v), 0, totalCharacters),
                totalCharacters, Mathf.Max(0.0001f, duration));

            tween.SetEase(ease).SetTarget(txt);
            if (independentUpdate) tween.SetUpdate(true);
            return tween;
        }

        /// <summary>
        /// 渐变 CanvasGroup 的 alpha，并在完成后可选地修改交互与射线阻挡。
        /// </summary>
        /// <param name="group">目标 CanvasGroup。</param>
        /// <param name="alpha">目标 alpha（0-1）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="setInteractable">完成后是否根据 alpha 设置 interactable。</param>
        /// <param name="setBlocksRaycasts">完成后是否根据 alpha 设置blocksRaycasts。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeTo(this CanvasGroup group, float alpha, float duration, bool setInteractable = false,
            bool setBlocksRaycasts = false, Ease ease = Ease.Linear, bool independentUpdate = false)
        {
            if (group == null) return null;
            var tw = group.DOFade(alpha, duration).SetCommon(group, ease, independentUpdate) as Tweener;
            if (tw != null)
            {
                tw.onComplete += () =>
                {
                    if (setInteractable) group.interactable = alpha >= 0.99f;
                    if (setBlocksRaycasts) group.blocksRaycasts = alpha >= 0.01f;
                };
            }

            return tw;
        }

        /// <summary>
        /// 让 CanvasGroup 的 alpha 在 min/max 间循环，产生“呼吸”效果。
        /// 动画会立即将透明度设为 maxAlpha，然后开始在 minAlpha 和 maxAlpha 之间往返。
        /// </summary>
        /// <param name="group">目标 CanvasGroup。</param>
        /// <param name="minAlpha">最小透明度（0-1）。</param>
        /// <param name="maxAlpha">最大透明度（0-1）。</param>
        /// <param name="duration">一次从 max->min 的时长（秒）。整体往返为 2*duration。</param>
        /// <param name="loops">循环次数：-1 表示无限循环。</param>
        /// <param name="ease">缓动类型，默认平滑的 InOut Sine。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新（暂停时仍播放）。</param>
        /// <returns>创建的 Tween（可用于停止或修改）。</returns>
        public static Tween Breath(this CanvasGroup group, float minAlpha = 0.6f, float maxAlpha = 1f,
            float duration = 1f, int loops = -1, Ease ease = Ease.InOutSine, bool independentUpdate = false)
        {
            if (group == null) return null;
            // Clamp and ensure min <= max
            minAlpha = Mathf.Clamp01(minAlpha);
            maxAlpha = Mathf.Clamp01(maxAlpha);
            if (minAlpha > maxAlpha)
            {
                (minAlpha, maxAlpha) = (maxAlpha, minAlpha);
            }

            // Immediately set alpha to max value, then start the animation loop.
            group.alpha = maxAlpha;

            // Use DOTween.To to smoothly animate the alpha and loop with Yoyo
            var tw = DOTween.To(() => group.alpha, v => group.alpha = v, minAlpha, duration)
                .SetLoops(loops, LoopType.Yoyo)
                .SetEase(ease)
                .SetTarget(group);

            if (independentUpdate) tw.SetUpdate(true);

            return tw;
        }

        /// <summary>
        /// UI 组合：渐显 + 弹入。
        /// </summary>
        /// <param name="group">目标 CanvasGroup（透明度控制）。</param>
        /// <param name="targetScale">用于缩放的 Transform。</param>
        /// <param name="duration">总时长（秒）。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence ShowWithFadeAndPop(this CanvasGroup group, Transform targetScale, float duration = 0.25f)
        {
            if (group == null || targetScale == null) return null;
            var originalScale = targetScale.localScale;
            return DOTween.Sequence()
                .Append(targetScale.DOScale(originalScale, duration).From(originalScale * 0.8f).SetEase(Ease.OutBack))
                .Join(group.DOFade(1f, duration).From(0f))
                .SetTarget(group);
        }

        /// <summary>
        /// UI 组合：渐隐 + 收缩。
        /// </summary>
        /// <param name="group">目标 CanvasGroup（透明度控制）。</param>
        /// <param name="targetScale">用于缩放的 Transform。</param>
        /// <param name="duration">总时长（秒）。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence HideWithFadeAndShrink(this CanvasGroup group, Transform targetScale,
            float duration = 0.2f)
        {
            if (group == null || targetScale == null) return null;
            var originalScale = targetScale.localScale;
            return DOTween.Sequence()
                .Append(targetScale.DOScale(originalScale * 0.8f, duration).SetEase(Ease.InQuad))
                .Join(group.DOFade(0f, duration))
                .SetTarget(group);
        }

        // ---------- Camera (2D zoom) ----------
        /// <summary>
        /// 渐变正交相机的 orthographicSize（2D 镜头缩放）。
        /// </summary>
        /// <param name="cam">目标相机（应为正交）。</param>
        /// <param name="size">目标正交尺寸。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>镜头缩放 Tweener。</returns>
        public static Tweener OrthoZoom(this Camera cam, float size, float duration, Ease ease = Ease.OutQuad,
            bool independentUpdate = false)
            => cam == null
                ? null
                : DOTween.To(() => cam.orthographicSize, v => cam.orthographicSize = v, size, duration)
                    .SetCommon(cam, ease, independentUpdate) as Tweener;

        // ---------- Composite helpers ----------
        /// <summary>
        /// 以弹入效果显示（从指定缩放到当前缩放）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="fromScale">起始统一缩放。</param>
        /// <param name="ease">缓动类型（默认 OutBack）。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence PopIn(this Transform t, float duration = 0.2f, float fromScale = 0f,
            Ease ease = Ease.OutBack, bool independentUpdate = false)
        {
            if (t == null) return null;
            var target = t.localScale;
            return DOTween.Sequence()
                .Append(t.DOScale(target, duration).From(Vector3.one * fromScale).SetEase(ease))
                .SetCommon(t, ease, independentUpdate) as Sequence;
        }

        /// <summary>
        /// 以缩出效果隐藏（缩放至目标比例）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="toScale">目标统一缩放。</param>
        /// <param name="ease">缓动类型（默认 InBack）。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence PopOut(this Transform t, float duration = 0.15f, float toScale = 0f,
            Ease ease = Ease.InBack, bool independentUpdate = false)
        {
            if (t == null) return null;
            return DOTween.Sequence()
                .Append(t.DOScale(Vector3.one * toScale, duration).SetEase(ease))
                .SetCommon(t, ease, independentUpdate) as Sequence;
        }

        /// <summary>
        /// 从偏移位置滑入至当前锚点位置。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="offset">从该偏移相对当前位置开始。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence SlideInFrom(this RectTransform rt, Vector2 offset, float duration = 0.25f,
            Ease ease = Ease.OutCubic, bool independentUpdate = false)
        {
            if (rt == null) return null;
            var target = rt.anchoredPosition;
            return DOTween.Sequence()
                .Append(rt.DOAnchorPos(target, duration).From(target - offset).SetEase(ease))
                .SetCommon(rt, ease, independentUpdate) as Sequence;
        }

        /// <summary>
        /// 从当前位置滑出到指定偏移位置。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="offset">相对偏移量。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence SlideOutTo(this RectTransform rt, Vector2 offset, float duration = 0.2f,
            Ease ease = Ease.InCubic, bool independentUpdate = false)
        {
            if (rt == null) return null;
            var target = rt.anchoredPosition + offset;
            return DOTween.Sequence()
                .Append(rt.DOAnchorPos(target, duration).SetEase(ease))
                .SetCommon(rt, ease, independentUpdate) as Sequence;
        }

        /// <summary>
        /// 先将 UI 的锚点位置瞬时设置到当前锚点位置加上指定的增量，然后滑动回原来的初始位置。
        /// 等价于：rt.anchoredPosition = original + offset; 然后动画到 original。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="offset">相对偏移量（在 x,y 上叠加）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence SlideInFromDelta(this RectTransform rt, Vector2 offset, float duration = 0.25f,
            Ease ease = Ease.OutCubic, bool independentUpdate = false)
        {
            if (rt == null) return null;
            var original = rt.anchoredPosition;
            // 立即将位置置为 original + offset，再动画回 original
            rt.anchoredPosition = original + offset;
            return DOTween.Sequence()
                .Append(rt.DOAnchorPos(original, duration).SetEase(ease))
                .SetCommon(rt, ease, independentUpdate) as Sequence;
        }

        /// <summary>
        /// 从当前锚点位置滑动到当前锚点位置加上指定的增量（相对目标位置）。
        /// </summary>
        /// <param name="rt">目标 RectTransform。</param>
        /// <param name="delta">相对偏移量（在 x,y 上叠加）。</param>
        /// <param name="duration">动画时长（秒）。</param>
        /// <param name="ease">缓动类型。</param>
        /// <param name="independentUpdate">是否独立于 Time.timeScale 更新。</param>
        /// <returns>移动的 Tweener。</returns>
        public static Tweener SlideToDelta(this RectTransform rt, Vector2 delta, float duration = 0.2f,
            Ease ease = Ease.InCubic, bool independentUpdate = false)
        {
            if (rt == null) return null;
            var target = rt.anchoredPosition + delta;
            return rt.DOAnchorPos(target, duration).SetCommon(rt, ease, independentUpdate) as Tweener;
        }

        /// <summary>
        /// 轻量弹跳（PunchScale）。
        /// </summary>
        /// <param name="t">目标 Transform。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="strength">弹力强度（统一）。</param>
        /// <param name="vibrato">振动次数。</param>
        /// <param name="elasticity">弹性强度。</param>
        /// <returns>组合动画 Sequence。</returns>
        public static Sequence PunchBounce(this Transform t, float duration = 0.3f, float strength = 0.2f,
            int vibrato = 8, float elasticity = 1f)
        {
            if (t == null) return null;
            return DOTween.Sequence()
                .Append(t.DOPunchScale(Vector3.one * strength, duration, vibrato, elasticity))
                .SetTarget(t);
        }

        // ---------- Lifecycle helpers ----------
        /// <summary>
        /// 终止与该组件相关联的所有 Tweens。
        /// </summary>
        /// <param name="c">目标组件（作为 SetTarget 绑定）。</param>
        /// <param name="complete">若为 true，则在 Kill 前先 Complete。</param>
        public static void KillTweens(this Component c, bool complete = false)
        {
            if (c == null) return;
            DOTween.Kill(c, complete);
        }

        /// <summary>
        /// 完成与该组件相关联的所有 Tweens。
        /// </summary>
        /// <param name="c">目标组件（作为 SetTarget 绑定）。</param>
        public static void CompleteTweens(this Component c)
        {
            if (c == null) return;
            DOTween.Complete(c);
        }

        // ---------- Color alpha helpers ----------
        /// <summary>
        /// 渐变 SpriteRenderer 的 alpha（自动 Clamp 到 0-1 版本）。
        /// </summary>
        /// <param name="sr">目标 SpriteRenderer。</param>
        /// <param name="alpha01">目标 alpha（0-1）。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeTo01(this SpriteRenderer sr, float alpha01, float duration)
            => sr == null ? null : sr.DOFade(Mathf.Clamp01(alpha01), duration).SetTarget(sr) as Tweener;

        /// <summary>
        /// 使 SpriteRenderer 渐显。
        /// </summary>
        /// <param name="sr">目标 SpriteRenderer。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeIn(this SpriteRenderer sr, float duration) => sr.FadeTo(1f, duration);

        /// <summary>
        /// 使 SpriteRenderer 渐隐。
        /// </summary>
        /// <param name="sr">目标 SpriteRenderer。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeOut(this SpriteRenderer sr, float duration) => sr.FadeTo(0f, duration);

        /// <summary>
        /// 使 CanvasGroup 渐显（同时可设置交互与射线）。
        /// </summary>
        /// <param name="g">目标 CanvasGroup。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="setInteractable">完成后是否设置交互。</param>
        /// <param name="setBlocksRaycasts">完成后是否设置射线阻挡。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeIn(this CanvasGroup g, float duration, bool setInteractable = true,
            bool setBlocksRaycasts = true)
            => g.FadeTo(1f, duration, setInteractable, setBlocksRaycasts);

        /// <summary>
        /// 使 CanvasGroup 渐隐（同时可设置交互与射线）。
        /// </summary>
        /// <param name="g">目标 CanvasGroup。</param>
        /// <param name="duration">时长（秒）。</param>
        /// <param name="setInteractable">完成后是否设置交互。</param>
        /// <param name="setBlocksRaycasts">完成后是否设置射线阻挡。</param>
        /// <returns>渐变 Tweener。</returns>
        public static Tweener FadeOut(this CanvasGroup g, float duration, bool setInteractable = true,
            bool setBlocksRaycasts = true)
            => g.FadeTo(0f, duration, setInteractable, setBlocksRaycasts);
    }
}
