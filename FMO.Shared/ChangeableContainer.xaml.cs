using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FMO.Shared;

/// <summary>
/// EditableContainer.xaml 的交互逻辑
/// </summary>
public partial class ChangeableContainer : UserControl
{
    private ContentPresenter? _content;

    public IPropertyModifier Binding
    {
        get { return (IPropertyModifier)GetValue(BindingProperty); }
        set { SetValue(BindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register("Binding", typeof(IPropertyModifier), typeof(ChangeableContainer), new PropertyMetadata(null));




    //public Style ElementStyle
    //{
    //    get { return (Style)GetValue(ElementStyleProperty); }
    //    set { SetValue(ElementStyleProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for ElementStyle.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty ElementStyleProperty =
    //    DependencyProperty.Register("ElementStyle", typeof(Style), typeof(EditableContainer), new PropertyMetadata(null, OnStyleChanged));

    //private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //    if (d is EditableContainer obj && e.NewValue is Style s && obj.Content.GetType() == s.TargetType && obj.Content is FrameworkElement element)
    //    {
    //        element.Style = s;
    //    }
    //}

    public ChangeableContainer()
    {
        InitializeComponent();

      //  Unloaded += EditableContainer_Unloaded;

       // Loaded += EditableContainer_Loaded;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _content = Template.FindName("PART_Content", this) as ContentPresenter;
    }


    private void EditableContainer_Loaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("-------------------------------");

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_content); i++)
        {
            var obj  = VisualTreeHelper.GetChild(_content, i);

            switch (obj)
            {
                case TextBox tb:
                    tb.TextChanged += (a, b) => Binding.Refresh();
                        break;
                default:
                    break;
            }

            Debug.WriteLine(obj.GetType());
        }

        Debug.WriteLine("\n\n");
    }

    private void EditableContainer_Unloaded(object sender, RoutedEventArgs e)
    {
        
    }
}
