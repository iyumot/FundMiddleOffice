using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO.Shared;

/// <summary>
/// 
/// </summary>
public class FileControl : Control
{
    static FileControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FileControl), new FrameworkPropertyMetadata(typeof(FileControl)));
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


