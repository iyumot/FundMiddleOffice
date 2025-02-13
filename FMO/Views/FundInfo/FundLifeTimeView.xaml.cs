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

    private void ContentControl_Drop(object sender, DragEventArgs e)
    {
        if (sender is ContentControl c && c.Content is PredefinedFileViewModel vm && e.Data.GetData(DataFormats.FileDrop) is string[] s && s.Length > 0)
            vm.OnDrop(s[0]);
            
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

            case RegistrationFlowViewModel vm:
                return (DataTemplate)element.TryFindResource("DT.Registration");


            case LiquidationFlowViewModel vm:
                return (DataTemplate)element.TryFindResource("DT.Liquidation");
            default:
                break;
        }




        return base.SelectTemplate(item, container);
    }
}


public partial class CustomFileInfoViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    public partial string? Name { get; set; }


    [ObservableProperty]
    public partial FileInfo? FileInfo { get; set; }


}
