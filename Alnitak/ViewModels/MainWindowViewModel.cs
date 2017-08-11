using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alnitak.Services;
using LibGit2Sharp;
using NLog;
using Alnitak.Utils;

namespace Alnitak.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        IServiceFactory serviceFactory = ServiceFactory.DefaultServiceFactory;
        ISettings settings;

        public string Path { get; set; } = "c:\\dev";
        public int RefreshEvery { get; set; } = 15; //15 mins

        public PropertiesObservableCollection<RepositoryViewModel> Repositories { get; set; } = new PropertiesObservableCollection<RepositoryViewModel>();

        public event EventHandler CheckStarted;
        public event EventHandler CheckFinished;

        public IAsyncCommand RefreshInfoCommand
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecuteRefreshInfoCommand);
            }
        }

        
        public IAsyncCommand TbIconClickedCommand
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecuteTbIconClickedCommand);
            }
        }

        public MainWindowViewModel()
        {
            settings = serviceFactory.GetService<ISettings>();
        }

        private Task<object> ExecuteTbIconClickedCommand(object arg)
        {
            switch (((MainWindow)arg).WindowState)
            {
                case System.Windows.WindowState.Normal:
                    ((MainWindow)arg).Activate();
                    break;
                case System.Windows.WindowState.Minimized:
                    ((MainWindow)arg).WindowState = System.Windows.WindowState.Normal;
                    ((MainWindow)arg).Activate();
                    break;
                case System.Windows.WindowState.Maximized:
                    ((MainWindow)arg).Activate();
                    break;
                default:
                    break;
            }
            return null;            
        }

        internal async Task<object> ExecuteRefreshInfoCommand(object arg)
        {
            logger.Info("Updating {0}", this.Path);
            CheckStarted?.Invoke(this, EventArgs.Empty);
            foreach (string dir in System.IO.Directory.EnumerateDirectories(this.Path))
            {
                try
                {
                    using (Repository repo = new Repository(dir))
                    {
                        var repositoryViewModel = Repositories.Where(r => r.Name.TrimEnd(new char[] {'\\'}) == dir).FirstOrDefault();
                        if (repositoryViewModel == null)
                        {
                            repositoryViewModel = new RepositoryViewModel(repo);
                            Repositories.Add(repositoryViewModel);
                        }
                        await repositoryViewModel.Refresh(settings.RemoteBranchFilter, true);
                    }
                }
                catch (RepositoryNotFoundException)
                {

                }
                catch
                {
                    throw;
                }
            }
            CheckFinished?.Invoke(this, EventArgs.Empty);
            logger.Info("Finished updating {0}", this.Path);
            return null;
        }
    }
}
