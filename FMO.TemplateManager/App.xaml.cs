using System.Text;
using System.Windows;

namespace FMO.TemplateManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {

        Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }
}
