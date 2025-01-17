using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// FundLifeTimeView.xaml 的交互逻辑
/// </summary>
public partial class FundLifeTimeView : UserControl
{
    public FundLifeTimeView()
    {
        InitializeComponent();
    }
}



public class FlowTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (container is not FrameworkElement element)
            return base.SelectTemplate(item, container);

        switch (item)
        {
            case InitiateFlowViewModel vm:
                return (DataTemplate)element.TryFindResource("DT.InitiateFlow");


            case ContractModifyFlowViewModel vm:
                return (DataTemplate)element.TryFindResource("DT.ContractModifyFlow");


            case ContractFinalizeFlowViewModel vm:
                return (DataTemplate)element.TryFindResource("DT.ContractFinalizeFlow");


            case SetupFlowViewModel vm:
                return (DataTemplate)element.TryFindResource("DT.SetupFlow");


            default:
                break;
        }




        return base.SelectTemplate(item, container);
    }
}


public partial class CustomFileInfo : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    public partial string? Name { get; set; }


    [ObservableProperty]
    public partial FileInfo? FileInfo { get; set; }


}
