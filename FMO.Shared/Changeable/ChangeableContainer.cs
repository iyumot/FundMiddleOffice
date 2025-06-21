﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMO.Shared
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FMO.Shared"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FMO.Shared;assembly=FMO.Shared"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:ChangeableContainer/>
    ///
    /// </summary>
    public class ChangeableContainer : ContentControl
    {
        static ChangeableContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChangeableContainer), new FrameworkPropertyMetadata(typeof(ChangeableContainer)));
        }

        public IPropertyModifier Binding
        {
            get { return (IPropertyModifier)GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.Register("Binding", typeof(IPropertyModifier), typeof(ChangeableContainer), new PropertyMetadata(null));




        public IChangeableEntityViewModel Entity
        {
            get { return (IChangeableEntityViewModel)GetValue(EntityProperty); }
            set { SetValue(EntityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Entity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EntityProperty =
            DependencyProperty.Register("Entity", typeof(IChangeableEntityViewModel), typeof(ChangeableContainer), new PropertyMetadata(null));

 


        public ControlTemplate EditTemplate
        {
            get { return (ControlTemplate)GetValue(EditTemplateProperty); }
            set { SetValue(EditTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditTemplateProperty =
            DependencyProperty.Register("EditTemplate", typeof(ControlTemplate), typeof(ChangeableContainer), new PropertyMetadata(null));




        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ChangeableContainer), new PropertyMetadata(false));


    }
}
