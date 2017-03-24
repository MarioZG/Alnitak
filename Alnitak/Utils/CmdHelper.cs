using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alnitak.Utils
{
    class CmdHelper
    {
        static public Task<CmdStreamsOutput> RunProcessAsync(string fileName, string arguments, string workingDir)
        {
            // there is no non-generic TaskCompletionSource
            var tcs = new TaskCompletionSource<CmdStreamsOutput>();


            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo = new ProcessStartInfo()
            {
                WorkingDirectory = workingDir,
                Arguments = arguments,
                FileName = fileName,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            process.Exited += (sender, args) =>
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                tcs.SetResult(new CmdStreamsOutput()
                {
                    Out = output,
                    Error = error
                });
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
