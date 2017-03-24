using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alnitak.Utils
{
    public class MemoryTargetEventArg : EventArgs
    {
        public string NewLog { get; set; }
        public IList<string> WholeLog { get; set; }
    }
}
