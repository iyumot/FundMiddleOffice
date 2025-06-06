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
    }

}
