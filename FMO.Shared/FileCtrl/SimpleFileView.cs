using System.Windows;
using System.Windows.Controls;

namespace FMO.Shared;


public class FileViewBase : HeaderedContentControl
{
    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FileViewBase), new PropertyMetadata(false));


}


public class SimpleFileView : FileViewBase
{
    static SimpleFileView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleFileView), new FrameworkPropertyMetadata(typeof(SimpleFileView)));
    }





    public SimpleFileViewModel Binding
    {
        get { return (SimpleFileViewModel)GetValue(BindingProperty); }
        set { SetValue(BindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register("Binding", typeof(SimpleFileViewModel), typeof(SimpleFileView), new PropertyMetadata(null));



}


public class MultiDualFileView : FileViewBase
{
    static MultiDualFileView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiDualFileView), new FrameworkPropertyMetadata(typeof(MultiDualFileView)));
    }





    public MultiDualFileViewModel Binding
    {
        get { return (MultiDualFileViewModel)GetValue(BindingProperty); }
        set { SetValue(BindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register("Binding", typeof(MultiDualFileViewModel), typeof(MultiDualFileView), new PropertyMetadata(null));

}