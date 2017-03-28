using Alnitak.ViewModels;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alnitak.Utils
{
    public class PeriodicalChecker
    {
        public bool Running { get; set; } = true;
        MainWindowViewModel mwvm;

        private bool SessionOnHold = false;
        Logger logger = LogManager.GetCurrentClassLogger();

        public PeriodicalChecker(MainWindowViewModel mwvm)
        {
            this.mwvm = mwvm;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            SessionOnHold = e.Reason == SessionSwitchReason.RemoteDisconnect;
            logger.Info("Session stae changed to {0}", e.Reason);
            switch (e.Reason)
            {
                case SessionSwitchReason.ConsoleConnect:
                    break;
                case SessionSwitchReason.ConsoleDisconnect:
                    break;
                case SessionSwitchReason.RemoteConnect:
                    Task.Run(() => mwvm.ExecuteRefreshInfoCommand(null));
                    break;
                case SessionSwitchReason.RemoteDisconnect:
                    break;
                case SessionSwitchReason.SessionLogon:
                    break;
                case SessionSwitchReason.SessionLogoff:
                    break;
                case SessionSwitchReason.SessionLock:
                    break;
                case SessionSwitchReason.SessionUnlock:
                    break;
                case SessionSwitchReason.SessionRemoteControl:
                    break;
                default:
                    break;
            }
        }

        public async Task Worker()
        {
            while(Running)
            {
                if (!SessionOnHold)
                {
                    await mwvm.ExecuteRefreshInfoCommand(null);
                }
                await Task.Delay(new TimeSpan(0, mwvm.RefreshEvery, 0));
            }
        }
    }
}
