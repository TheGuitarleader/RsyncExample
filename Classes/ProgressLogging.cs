using FastRsync.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RsyncExample.Classes
{
    internal class ProgressLogging : IProgress<ProgressReport>
    {
        private ProgressOperationType currentOperation;
        private int progressPercentage;

        public void Report(ProgressReport progress)
        {
            int num = (int)(progress.CurrentPosition / (double)progress.Total * 100.0 + 0.5);
            if (currentOperation != progress.Operation)
            {
                progressPercentage = -1;
                currentOperation = progress.Operation;
            }

            if (progressPercentage != num && num % 10 == 0)
            {
                progressPercentage = num;
                Console.WriteLine($"{progress.Operation}: {progress.CurrentPosition} out of {progress.Total} ({progressPercentage}%)");
            }
        }
    }
}
