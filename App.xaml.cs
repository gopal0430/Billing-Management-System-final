using System;
using System.IO;
using System.Reflection;
using System.Windows;
using log4net;
using log4net.Config;
/// <summary>
/// Implementation of log4net App.xaml
/// </summary>

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
         var repo = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));
            ILog log = LogManager.GetLogger(typeof(App));
            log.Info("Application started.");

        base.OnStartup(e);
    }
}
