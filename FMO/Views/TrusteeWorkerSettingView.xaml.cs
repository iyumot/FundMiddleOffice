using FMO.Trustee;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// TrusteeWorkerSettingView.xaml 的交互逻辑
/// </summary>
public partial class TrusteeWorkerSettingView : UserControl
{
    public TrusteeWorkerSettingView()
    {
        InitializeComponent();

        Task.Run(() => this.Dispatcher.BeginInvoke(() => Init()));
    }

    private void Init()
    {
        int rows = TrusteeGallay.Trustees.Length;
        int cols = 5;

        for (int i = 0; i <= rows; i++)
            grid.RowDefinitions.Add(new RowDefinition());

        for (int i = 0; i < rows; i++)
        {

            // 行首
            var tb = new TextBlock
            {
                Text = TrusteeGallay.Trustees[i].Title,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            grid.Children.Add(tb);
            Grid.SetRow(tb, i + 1);
        }

        for (int i = 0; i <= cols; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition());




    }
}
