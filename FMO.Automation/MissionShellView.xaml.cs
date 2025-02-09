using System;
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

namespace FMO.Schedule
{
    /// <summary>
    /// MissionShellView.xaml 的交互逻辑
    /// </summary>
    public partial class MissionShellView : UserControl
    {
        public MissionShellView()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var type = Type.GetType(e.NewValue.GetType().ToString().Replace("ViewModel","View"));

            if (type is not null)
                Content = Activator.CreateInstance(type);
        }
    }
}
