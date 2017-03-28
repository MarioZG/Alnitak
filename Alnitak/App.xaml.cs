using Alnitak.Utils;
using Microsoft.Win32;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Alnitak
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Logger logger;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Target.Register<Utils.NLogMemoryTarget>("NLogMemoryTarget");
            logger = LogManager.GetCurrentClassLogger();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error(e.Exception, "Cought at Dispatcher.UnhandledException");
            throw e.Exception;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error(e.ExceptionObject as Exception, "Cought at AppDomain.CurrentDomain.UnhandledException");
            throw e.ExceptionObject as Exception;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error(e.Exception, "Cought at DispatcherUnhandledException");
            throw e.Exception;
        }
    }
}
