using System;
using System.Threading.Tasks;

namespace BPM.Listener.Example.LongRunningProcess
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            Console.Out.WriteLine("Long Running Process started");
            var exitCode = -1;
            if (args.Length > 0 && int.TryParse(args[0], out var delay))
            {
                Console.Out.WriteLine($"Process running with argument [{delay}]");
                await Task.Delay(delay);
                exitCode = 0;
            }            
            Console.Out.WriteLine($"Long Running Process finished with exit code {exitCode}");
            Environment.Exit(exitCode);
        }
    }
}
