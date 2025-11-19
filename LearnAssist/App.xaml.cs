using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace LearnAssist;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
#if RELEASE
    // 设置工作目录 
  // 从注册表读取
    using (var key = Registry.CurrentUser.OpenSubKey(@$"Software\Nexus"))       
#else
        using (var key = Registry.CurrentUser.OpenSubKey(@$"Software\Nexus\Debug"))
#endif
        {
            if (key != null)
            {
                var workFolder = key.GetValue("WorkingFolder") as string;
                if (!string.IsNullOrWhiteSpace(workFolder))
                {
                    var di = new DirectoryInfo(workFolder);
                    if (di.Exists)
                        Directory.SetCurrentDirectory(di.FullName);
                }
            }
        }

    }
}
