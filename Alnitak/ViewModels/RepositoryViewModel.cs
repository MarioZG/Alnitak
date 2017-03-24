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

        public string MasterCount { get; set; }
        public PropertiesObservableCollection<RemoteBranchViewModel> Remotes { get; set; } = new PropertiesObservableCollection<RemoteBranchViewModel>();

        public IAsyncCommand PullCommand
        {
            get
            {
                return new RealyAsyncCommand<object>(ExecutePullCommand, CanExecutePullCommand);
            }
        }

        private async Task<object> ExecutePullCommand(object arg)
        {
            logger.Info("Pulling {0}", repo.Info.WorkingDirectory);
            CmdStreamsOutput remotes = await CmdHelper.RunProcessAsync(
                "cmd",
                "/C git pull",
                repo.Info.WorkingDirectory
                );
            logger.Info(remotes.Out);
            logger.Info(remotes.Error);
            await Refresh(settings.RemoteBranchFilter);
            return null;
        }

        private bool CanExecutePullCommand(object arg)
        {
            return repo?.Head?.FriendlyName == "master";    
        }

        internal async Task Refresh(string branchFilter)
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
                        "/C git fetch",
                        repo.Info.WorkingDirectory
                        );
                    //await Task.Run(() => Commands.Fetch(repo, remote.Name, refSpecs, null, logMessage));
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
