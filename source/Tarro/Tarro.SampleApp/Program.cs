﻿using System;
using System.Configuration;
using System.Threading;

namespace Tarro.SampleApp
{
    class Program
    {

        private static Timer timer;
        static void Main(string[] args)
        {
            timer = new Timer(state => Log("Heartbeat"), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            Log("Starting");
            Log(Thread.CurrentThread.ManagedThreadId.ToString());
            Log("Conf: " + ConfigurationManager.AppSettings["setting"]);
            Log(AppDomain.CurrentDomain.BaseDirectory);
            Log($"Input redirected: {Console.IsInputRedirected}");
            if (Console.IsInputRedirected)
                Console.Read();
            Log("Exiting main method");
        }

        private static void Log(string message)
        {
            message = DateTime.Now.ToString("HH:mm:ss") + " " + message;
            Console.WriteLine(message);

        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {

            Log("Shutting down from Unload");
            timer.Dispose();
        }


    }
}
