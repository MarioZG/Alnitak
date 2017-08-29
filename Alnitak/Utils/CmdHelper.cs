using NLog;
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
        static public Task<CmdStreamsOutput> RunProcessAsync(string fileName, string arguments, string workingDir, ILogger logger = null)
        {
            // there is no non-generic TaskCompletionSource
            var tcs = new TaskCompletionSource<CmdStreamsOutput>();
           // StringBuilder output = new StringBuilder();
          //  StringBuilder error = new StringBuilder();

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

            process.OutputDataReceived += (sender, e) =>
            {
                logger?.Info(e.Data);
              //  output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                logger?.Info(e.Data);
               // error.AppendLine(e.Data);
            };

            process.Exited += (sender, args) =>
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                tcs.SetResult(new CmdStreamsOutput()
                {
                    Out = output.ToString(),
                    Error = error.ToString()
                });

                process.Dispose();
            };

            process.Start();
            //process.BeginOutputReadLine();
            return tcs.Task;
        }
    }
}
