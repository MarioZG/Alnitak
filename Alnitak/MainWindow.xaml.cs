using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Alnitak.ViewModels;
using LibGit2Sharp;
using Alnitak.Utils;
using NLog;

namespace Alnitak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel mwvm = new MainWindowViewModel();
            mwvm.CheckStarted += Mwvm_CheckStarted;
            mwvm.CheckFinished += Pc_CheckFinished;

            PeriodicalChecker pc = new PeriodicalChecker(mwvm);
            this.Dispatcher.InvokeAsync(() => pc.Worker());
            //new Task(() => pc.Worker()).Start();

            foreach (var target in NLog.LogManager.Configuration.AllTargets)
            {
                if (target.Name.Equals("memory"))
                {
                    NLogMemoryTarget mt = target as NLogMemoryTarget;
                    mt.LogUpdated += Mt_LogUpdated;
                }
            }

            this.DataContext = mwvm;
        }

        private void Mwvm_CheckStarted(object sender, EventArgs e)
        {
            this.tbIcon.Dispatcher.InvokeAsync(() => this.tbIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/gitSynchronise.ico", UriKind.RelativeOrAbsolute)));
        }

        private void Pc_CheckFinished(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                bool anyBehind = ((MainWindowViewModel)this.DataContext).Repositories.Any(r => r.Remotes.Any(r2 => r2.Behind > 0));
                if (anyBehind)
                {
                    this.tbIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/gitWarning.ico", UriKind.RelativeOrAbsolute));
                    this.tbIcon.ShowBalloonTip("Repos refreshed", "You are behind on some of repos!", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                }
                else
                {
                    this.tbIcon.IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/gitOK.ico", UriKind.RelativeOrAbsolute));
                }
            });
        }

        private void Mt_LogUpdated(object sender, EventArgs e)
        {
            string msg = ((MemoryTargetEventArg)e).NewLog;
            if (!msg.EndsWith(Environment.NewLine))
            {
                msg += Environment.NewLine;
            }
            this.txtLog.Dispatcher.InvokeAsync(() => { this.txtLog.Text += msg; this.svLog.ScrollToEnd(); });
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
