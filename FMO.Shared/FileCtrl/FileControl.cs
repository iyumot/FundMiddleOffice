using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO.Shared;

/// <summary>
/// 
/// </summary>
public class FileControl : HeaderedContentControl
{
    static FileControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FileControl), new FrameworkPropertyMetadata(typeof(FileControl)));
    }

 

    protected override void OnDrop(DragEventArgs e)
    {
        if (DataContext is IFileSetter setter && Binding is not null && e.Data.GetData(DataFormats.FileDrop) is string[] ss)
        {
            // 如果有旧文件
            if (Binding.File is not null)
                try { Binding.File.Delete(); } catch { }

            setter.SetFile(Binding, ss[0]);
        }
    }

    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FileControl), new PropertyMetadata(false));



    public IFileViewModel Binding
    {
        get { return (IFileViewModel)GetValue(BindingProperty); }
        set { SetValue(BindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register("Binding", typeof(IFileViewModel), typeof(FileControl), new PropertyMetadata(null));





 

}







public class MultiFileControl : Control
{
    static MultiFileControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiFileControl), new FrameworkPropertyMetadata(typeof(MultiFileControl)));
    }





    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(MultiFileControl), new PropertyMetadata(false));



    public IFileSelector Binding
    {
        get { return (IFileSelector)GetValue(BindingProperty); }
        set { SetValue(BindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register("Binding", typeof(IFileSelector), typeof(MultiFileControl), new PropertyMetadata(null));

}






public class SingleFileView : HeaderedContentControl
{
    static SingleFileView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SingleFileView), new FrameworkPropertyMetadata(typeof(SingleFileView)));
    }



    //protected override void OnDrop(DragEventArgs e)
    //{
    //    if (DataContext is IFileSetter setter && Binding is not null && e.Data.GetData(DataFormats.FileDrop) is string[] ss)
    //    {
    //        // 如果有旧文件
    //        if (Binding.File is not null)
    //            try { Binding.File.Delete(); } catch { }

    //        setter.SetFile(Binding, ss[0]);
    //    }
    //}

    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SingleFileView), new PropertyMetadata(false));



    public SingleFileViewModel Binding
    {
        get { return (SingleFileViewModel)GetValue(BindingProperty); }
        set { SetValue(BindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register("Binding", typeof(SingleFileViewModel), typeof(SingleFileView), new PropertyMetadata(null));







}







public class MultipleFileView:Control
{
    static MultipleFileView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultipleFileView), new FrameworkPropertyMetadata(typeof(MultipleFileView)));
    }


    public IFileSelector Binding
    {
        get { return (IFileSelector)GetValue(BindingProperty); }
        set { SetValue(BindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BindingProperty =
        DependencyProperty.Register("Binding", typeof(IFileSelector), typeof(MultipleFileView), new PropertyMetadata(null));

    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(MultipleFileView), new PropertyMetadata(false));


}