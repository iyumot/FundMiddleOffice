using Microsoft.Win32;
using Serilog;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FMO"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FMO;assembly=FMO"
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
    ///     <MyNamespace:FileDisplay/>
    ///
    /// </summary>
    public class FileDisplay : Control
    {


        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(FileDisplay), new PropertyMetadata("文件名"));




        public string? FilePath
        {
            get { return (string?)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(FileDisplay), new PropertyMetadata(null));




        static FileDisplay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileDisplay), new FrameworkPropertyMetadata(typeof(FileDisplay)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var btn = Template.FindName("PART_View", this) as Button;
            if (btn is not null)
                btn.Click += ViewFile_Click;

            btn = Template.FindName("PART_SaveAs", this) as Button;
            if (btn is not null)
                btn.Click += SaveAs_Click;

            btn = Template.FindName("PART_Modify", this) as Button;
            if (btn is not null)
                btn.Click += Modify_Click;

            btn = Template.FindName("PART_Modify2", this) as Button;
            if (btn is not null)
                btn.Click += Modify_Click;
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        { 
            if (!File.Exists(FilePath)) return;

            try
            {
                var d = new SaveFileDialog();
                d.FileName = Path.GetFileName(FilePath) + Path.GetExtension(FilePath);
                if (d.ShowDialog() == true)
                    File.Copy(FilePath, d.FileName);
            }
            catch (Exception ex)
            {
                Log.Error($"文件另存为失败: {ex.Message}");
            }
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() == true)
                FilePath = fd.FileName;
        }

        private void ViewFile_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(FilePath)) return;

            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(FilePath) {  UseShellExecute = true }); } catch { }
        }
    }
}
