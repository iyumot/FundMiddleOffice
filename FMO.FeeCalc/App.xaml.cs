using System.IO;
using System.Windows;

namespace FMO.FeeCalc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            Directory.SetCurrentDirectory(@"E:\fmo");
#endif
        }
    }

}
