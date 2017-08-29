using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alnitak.Services;
using Alnitak.Utils;
using LibGit2Sharp;
using NLog;

namespace Alnitak.ViewModels
{
    public class RepositoryViewModel : BaseViewModel
    {
        private string name;
        private Repository repo;

        Logger logger = LogManager.GetCurrentClassLogger();
        IServiceFactory serviceFactory = ServiceFactory.DefaultServiceFactory;
        ISettings settings;

        public RepositoryViewModel(Repository repo)
        {
            settings = serviceFactory.GetService<ISettings>();

            this.repo = repo;
            this.Name = repo.Info.WorkingDirectory;
        }

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public string FolderName
        {
            get { return System.IO.Path.GetFileName(name.TrimEnd(new char[] { '\\' })); }
        }

        public string MasterCount { get; set; }
        public PropertiesObservableCollection<RemoteBranchViewModel> Remotes { get; set; } = new PropertiesObservableCollection<RemoteBranchViewModel>();

        public string NewBranchName { get; set; }
        public System.Windows.Visibility NewBranchNameVisible { get; set; } = System.Windows.Visibility.Collapsed;

        public IAsyncCommand PullCommand
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecutePullCommand, CanExecutePullCommand);
            }
        }

        public IAsyncCommand StartShCommand
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecuteStartShCommand, CanExecuteStartShCommand);
            }
        }

        public IAsyncCommand StartCreateBranchFromMain
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecuteStartCreateBranchFromMainCommand, CanStartExecuteCreateBranchFromMainCommand);
            }
        }

        public IAsyncCommand CreateBranchFromMain
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecuteCreateBranchFromMainCommand, CanExecuteCreateBranchFromMainCommand);
            }
        }

        private bool CanExecuteStartShCommand(object arg)
        {
            return true;
        }
        public IAsyncCommand CheckoutMasterCommand
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecuteCheckoutMasterCommand, CanExecuteCheckoutMasterCommand);
            }
        }

        private Task<object> ExecuteStartShCommand(object arg)
        {
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo()
            {
                WorkingDirectory = repo.Info.WorkingDirectory,
                FileName = "C:\\Program Files\\Git\\bin\\sh.exe",
                //WindowStyle = ProcessWindowStyle.Hidden,
                //UseShellExecute = false,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                //RedirectStandardInput = true,
                //CreateNoWindow = true
            };

            process.Start();

            return null;
        }

        private async Task<object> ExecutePullCommand(object arg)
        {
            try
            {
                logger.Info("Pulling {0}", repo.Info.WorkingDirectory);
                CmdStreamsOutput remotes = await CmdHelper.RunProcessAsync(
                    "cmd",
                    "/C git pull",
                    repo.Info.WorkingDirectory
                    );

                logger.Info(remotes.Error);
                logger.Info(remotes.Out);

                await Refresh(settings.RemoteBranchFilter, arg == null ? false: (bool)arg);

                logger.Info("Pull finished {0}", repo.Info.WorkingDirectory);
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Pull error");
                throw;
            }
        }

        private Task<object> ExecuteStartCreateBranchFromMainCommand(object arg)
        {
            this.NewBranchName = "/mk/";
            this.NewBranchNameVisible = System.Windows.Visibility.Visible;
            RaisePropertyChangedEvent(nameof(NewBranchName));
            RaisePropertyChangedEvent(nameof(NewBranchNameVisible));
            return null;
        }

        private bool CanStartExecuteCreateBranchFromMainCommand(object arg)
        {
            return IsMaster();
        }

        private bool CanExecuteCreateBranchFromMainCommand(object arg)
        {
            return IsMaster();
        }

        private async Task<object> ExecuteCreateBranchFromMainCommand(object arg)
        {
            CmdStreamsOutput remotes = await CmdHelper.RunProcessAsync(
                "cmd",
                $"/C git checkout --no-track origin/master -b \"{NewBranchName}\"",
                repo.Info.WorkingDirectory
                );

            logger.Info(remotes.Error);
            logger.Info(remotes.Out);

            this.NewBranchNameVisible = System.Windows.Visibility.Collapsed;
            RaisePropertyChangedEvent(nameof(NewBranchNameVisible));
            return null;
        }



        private bool CanExecutePullCommand(object arg)
        {
            return IsMaster();
        }

        private bool CanExecuteCheckoutMasterCommand(object arg)
        {
            return ! IsMaster();
        }

        private async Task<object> ExecuteCheckoutMasterCommand(object arg)
        {
           var remotes =  await CmdHelper.RunProcessAsync(
                    "cmd",
                    "/C git checkout master",
                    repo.Info.WorkingDirectory
                    );


            logger.Info(remotes.Error);
            logger.Info(remotes.Out);

            return remotes;
        }


        private bool IsMaster()
        {
            return repo?.Head?.FriendlyName == "master";
        }

        internal async Task Refresh(string branchFilter, bool executePullOnmastersbehind)
        {
            repo = new Repository(name); //refrsesh repo, otherwise memory exceptions are thrown
            //RepositoryStatus status;
            CmdStreamsOutput remotes = await CmdHelper.RunProcessAsync(
                    "cmd",
                    "/C git remote",
                    repo.Info.WorkingDirectory
                    );
            //await Task.Run(() => status = repo.RetrieveStatus());

            string logMessage = String.Empty;
            bool fetchedOk = false;
            foreach (Remote remote in repo.Network.Remotes)
            {
                try
                {
                    IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    CmdStreamsOutput output = await CmdHelper.RunProcessAsync(
                        "cmd",
                        "/C git fetch --prune",
                        repo.Info.WorkingDirectory
                        );
                    fetchedOk = true;

                    output = await CmdHelper.RunProcessAsync(
                         "cmd",
                         "/C git rev-list --count --left-right master..." + remote.Name + "/master",
                         repo.Info.WorkingDirectory
                         );

                    string[] infoSplit = output.Out.Split(new char[] { '\t' });

                    RemoteBranchViewModel rbvm = Remotes.Where(r => r.Name == remote.Name).FirstOrDefault();
                    if (rbvm == null)
                    {
                        rbvm = new RemoteBranchViewModel
                        {
                            Name = remote.Name
                        };
                        Remotes.Add(rbvm);

                    }
                    rbvm.Ahead = Int32.Parse(infoSplit[0]);
                    rbvm.Behind = Int32.Parse(infoSplit[1]);
                    rbvm.Info = output.Out + output.Error;
                    rbvm.RemoteBranchesCount = GetRemoteBranchesCount(remote, branchFilter);


                    if (IsMaster() && rbvm.Behind > 0 && executePullOnmastersbehind)
                    {
                        await ExecutePullCommand(false);
                    }
                }
                catch (LibGit2Sharp.LibGit2SharpException ex)
                {
                    throw;
                }
            }


            //var remoteBranches = repo.Branches.Where(b => b.IsRemote);
            //if (! String.IsNullOrEmpty(branchFilter))
            //{
            //    remoteBranches = remoteBranches.Where(b => b.FriendlyName.Contains(branchFilter));
            //}

            //foreach (var remoteBranch in remoteBranches.ToList())
            //{
            //    //git rev-list--count--left - right master...github/master

            //    CmdStreamsOutput output = await CmdHelper.RunProcessAsync(
            //        "cmd",
            //        "/C git rev-list --count --left-right master..." + remoteBranch.FriendlyName,
            //        repo.Info.WorkingDirectory
            //        );

            //    RemoteBranches.Add(new RemoteBranchViewModel
            //    {
            //        Name = remoteBranch.FriendlyName,
            //        Info = output.Out + output.Error
            //    });

            //}
        }

        private int GetRemoteBranchesCount(Remote remote, string branchFilter)
        {
            var query = repo.Branches.Where(b => b.IsRemote && (b.RemoteName == remote.Name));
            if (! String.IsNullOrWhiteSpace(branchFilter))
            {
                query = query.Where(b => b.FriendlyName.Contains(branchFilter));
            }            
            return query.Count();
        }
    }
}
