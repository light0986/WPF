using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace 重返雲之國_外傳
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Process process = Process.GetCurrentProcess();
            foreach (Process p in Process.GetProcessesByName(process.ProcessName))
            {
                if (p.Id != process.Id)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
        }
    }
}
