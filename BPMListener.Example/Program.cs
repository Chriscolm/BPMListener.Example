using BPMListener.Example.Execution;
using BPMListener.Example.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BPMListener.Example
{
    static class Program
    {
        static void Main()
        {            
            var logger = CreateLogger(nameof(Program));
            logger.LogInformation("BPM Listener starting");

            List<TaskConfig> taskMap = new();
            TimingConfig timingConfig = new();
            var config = Configure();
            config.GetSection("taskMap").Bind(taskMap);
            config.GetSection("durations").Bind(timingConfig);

            ILogger loggerFactory(string name)
            {
                var res = CreateLogger(name);
                return res;
            };

            
            var watchdog = new Watchdog(new Requests.ExtendLockRequest(), timingConfig.LockExtent, timingConfig.TimerInterval, loggerFactory);
                        
            using var listener = new Listener(taskMap, watchdog, timingConfig.DefaultLockDuration, loggerFactory);

            logger.LogInformation("Start listening");

            var f = new TaskFactory().StartNew(async () => await listener.ListenAsync(), TaskCreationOptions.LongRunning);            
            var line = Console.ReadLine();
            if(line == "quit")
            {
                listener.Stop();
            }
            Environment.Exit(0);
        }

        static ILogger CreateLogger(string name)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss LEVEL: ";
            }).SetMinimumLevel(LogLevel.Debug));
            var logger = loggerFactory.CreateLogger(name);
            return logger;
        }

        static IConfiguration Configure()
        {            
            var root = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();                
            return root;
        }
    }
}
