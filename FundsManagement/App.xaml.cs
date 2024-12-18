using Serilog;
using System.Configuration;
using System.Data;
using System.Windows;

namespace FMO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

            Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();
        }
    }

}
