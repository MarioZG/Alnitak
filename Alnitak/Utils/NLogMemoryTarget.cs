using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alnitak.Utils
{
    [Target("NLogMemoryTarget")]
    public class NLogMemoryTarget : NLog.Targets.TargetWithLayout
    {
        public event EventHandler  LogUpdated;

        public IList<string> Logs { get; private set; } = new List<string>();
        protected override void Write(LogEventInfo logEvent)
        {
            string msg = logEvent.FormattedMessage;// this.Layout.Render(logEvent);

            this.Logs.Add(msg);

            LogUpdated(this, new MemoryTargetEventArg() { NewLog = msg, WholeLog = Logs });
        }
    }
}
