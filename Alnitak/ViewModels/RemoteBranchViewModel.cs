using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alnitak.ViewModels
{
    public class RemoteBranchViewModel : BaseViewModel
    {
        public string Name { get; set; }

        private int ahead;
        public int Ahead
        {
            get { return ahead; }
            set { SetProperty(ref ahead, value); }
        }

        private int behind;
        public int Behind
        {
            get { return behind; }
            set { SetProperty(ref behind, value); }
        }

        public string Info { get; set; }

        private int remoteBranchesCount;
        public int RemoteBranchesCount
        {
            get { return remoteBranchesCount; }
            set { SetProperty(ref remoteBranchesCount, value); }
        }
    }
}
