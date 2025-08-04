using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace FMO.Shared;

public static class MaskService
{
    // 注册附加属性 IsMask
    public static readonly DependencyProperty IsMaskProperty =
        DependencyProperty.RegisterAttached(
            "IsMask",
            typeof(bool),
            typeof(MaskService),
            new PropertyMetadata(false, OnIsMaskChanged));

    public static bool GetIsMask(DependencyObject obj) =>
        (bool)obj.GetValue(IsMaskProperty);

    public static void SetIsMask(DependencyObject obj, bool value) =>
        obj.SetValue(IsMaskProperty, value);

    // 注册附加属性 BlurRadius
    public static readonly DependencyProperty BlurRadiusProperty =
        DependencyProperty.RegisterAttached(
            "BlurRadius",
            typeof(double),
            typeof(MaskService),
            new PropertyMetadata(15.0, OnBlurRadiusChanged));

    public static double GetBlurRadius(DependencyObject obj) =>
        (double)obj.GetValue(BlurRadiusProperty);

    public static void SetBlurRadius(DependencyObject obj, double value) =>
        obj.SetValue(BlurRadiusProperty, value);

    // 内部属性存储原始效果
    private static readonly DependencyProperty OriginalEffectProperty =
        DependencyProperty.RegisterAttached(
            "OriginalEffect",
            typeof(Effect),
            typeof(MaskService));

    private static Effect GetOriginalEffect(DependencyObject obj) =>
        (Effect)obj.GetValue(OriginalEffectProperty);

    private static void SetOriginalEffect(DependencyObject obj, Effect value) =>
        obj.SetValue(OriginalEffectProperty, value);

    // 全局模糊状态
    private static bool _isGlobalMaskEnabled = false;

    static MaskService()
    {
        // 注册全局键盘事件
        EventManager.RegisterClassHandler(
            typeof(Window),
            Window.KeyDownEvent,
            new KeyEventHandler(HandleKeyDown)
        );
    }

    // 属性变更回调
    private static void OnIsMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        if (_isGlobalMaskEnabled && (bool)e.NewValue)
        {
            ApplyBlurEffect(element);
        }
        else if (!(bool)e.NewValue && GetOriginalEffect(element) != null)
        {
            RestoreOriginalEffect(element);
        }
    }

    private static void OnBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element && _isGlobalMaskEnabled && GetIsMask(element))
        {
            ApplyBlurEffect(element);
        }
    }

    // 全局按键处理
    private static void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F9 && Keyboard.Modifiers == ModifierKeys.None)
        {
            _isGlobalMaskEnabled = !_isGlobalMaskEnabled;
            ToggleMaskEffects();
            UpdateStatusIndicator(sender as Window);
            e.Handled = true;
        }
    }

    // 切换所有元素的模糊效果
    private static void ToggleMaskEffects()
    {
        foreach (Window window in Application.Current.Windows)
        {
            ProcessVisualTree(window, _isGlobalMaskEnabled);
        }
    }

    // 递归处理视觉树
    private static void ProcessVisualTree(DependencyObject parent, bool applyBlur)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is UIElement element)
            {
                if (GetIsMask(element))
                {
                    if (applyBlur) ApplyBlurEffect(element);
                    else RestoreOriginalEffect(element);
                }
            }
            ProcessVisualTree(child, applyBlur);
        }
    }

    // 应用模糊效果
    private static void ApplyBlurEffect(UIElement element)
    {
        if (GetOriginalEffect(element) == null)
        {
            // 保存原始效果
            SetOriginalEffect(element, element.Effect);

            // 应用模糊效果
            double radius = GetBlurRadius(element);
            element.Effect = new BlurEffect { Radius = radius };
        }
        else
        {
            // 更新现有模糊效果
            if (element.Effect is BlurEffect blurEffect)
            {
                blurEffect.Radius = GetBlurRadius(element);
            }
        }
    }

    // 恢复原始效果
    private static void RestoreOriginalEffect(UIElement element)
    {
        var originalEffect = GetOriginalEffect(element);
        if (originalEffect != null)
        {
            element.Effect = originalEffect;
            SetOriginalEffect(element, null);
        }
    }

    // 更新状态指示器
    private static void UpdateStatusIndicator(Window window)
    {
        if (window == null) return;

        if (window.FindName("statusIndicator") is Border indicator &&
            window.FindName("statusText") is TextBlock statusText)
        {
            if (_isGlobalMaskEnabled)
            {
                indicator.Background = new SolidColorBrush(Colors.Red);
                statusText.Text = "隐私保护已启用 (按 F9 关闭)";
            }
            else
            {
                indicator.Background = new SolidColorBrush(Colors.Green);
                statusText.Text = "隐私保护已禁用 (按 F9 启用)";
            }
        }
    }
}