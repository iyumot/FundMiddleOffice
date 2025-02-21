using CommunityToolkit.Mvvm.ComponentModel;
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

namespace FMO;

/// <summary>
/// QualificationView.xaml 的交互逻辑
/// </summary>
public partial class QualificationView : UserControl
{
    public QualificationView()
    {
        InitializeComponent();

        DataContext = new QualificationViewModel();
    }
}


public partial class QualificationViewModel:ObservableObject
{
    [ObservableProperty]
    public partial bool IsProfessional { get; set; }

    [ObservableProperty]
    public partial bool NeedExperience { get; set; }


}


