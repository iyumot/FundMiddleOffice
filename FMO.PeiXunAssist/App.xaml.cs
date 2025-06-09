using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace FMO.LearnAssist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            Directory.SetCurrentDirectory(@"D:\fmo");
#endif


            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }
    }

}
