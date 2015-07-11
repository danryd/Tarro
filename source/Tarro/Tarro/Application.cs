﻿using System;
using System.Diagnostics;
using System.IO;
using Tarro.Logging;

namespace Tarro
{
    internal class Application : IDisposable
    {
        private readonly ILog log = LogFactory.GetLogger<Application>();
        private readonly string name;
        private readonly string pathToApp;
        private readonly string executable;
        private readonly AppWatcher watcher;

        private Process process;

        private readonly string cachePath = "appCache";
        public Application(string name, string pathToApp, string executable)
        {
            this.name = name;
            this.pathToApp = pathToApp;
            this.executable = executable;
            watcher = new AppWatcher(pathToApp);
            watcher.AppChanged += (o, e) =>
            {
                Stop();
            };
            watcher.AfterQuietPeriod += (o, e) =>
            {
                Start();
            };
        }

        public void Start()
        {
            try
            {
                log.Info("Starting application ({0})", name);
                ShadowCopy();
                var setup = CreateSetup();

                process = new Process();
                process.StartInfo = setup;
                process.EnableRaisingEvents = true;
                process.Exited += process_Exited;

                process.Start();


                if (Environment.UserInteractive)
                {
                    process.ErrorDataReceived += (sendingProcess, errorLine) => log.Error(string.Format("[{0}] {1}",process.ProcessName, errorLine.Data));
                    process.OutputDataReceived += (sendingProcess, dataLine) => log.Info(string.Format("[{0}] {1}",process.ProcessName,dataLine.Data));
                    
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    

                }
                log.Info("Application started ({0})", name);
            }
            catch (Exception ex)
            {
                log.Error("Unable to start application ({0})", ex, name);
            }
        }

        void process_Exited(object sender, EventArgs e)
        {
            log.Warn("Process exit");
        }

        private void ShadowCopy()
        {
            CleanShadowDirectory();
            DirectoryCopy(pathToApp, ShadowPath, true);
        }

        private void CleanShadowDirectory()
        {
            var directory = new DirectoryInfo(ShadowPath);
            if (directory.Exists)
                directory.Delete(true);
        }

        private string ShadowPath
        {
            get
            {
                var shadowPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cachePath, executable);
                return shadowPath;
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private ProcessStartInfo CreateSetup()
        {
            var setup = new ProcessStartInfo();
            setup.FileName = Path.Combine(ShadowPath, executable);
            setup.WorkingDirectory = ShadowPath;
            setup.UseShellExecute = false;
            setup.RedirectStandardOutput = true;
            setup.RedirectStandardError = true;
            //setup.RedirectStandardInput = true;

            return setup;
        }

        private readonly object unloadLock = new object();
        private void Stop()
        {
            lock (unloadLock)
                if (!process.HasExited)
                {
                    process.Kill();
                    process.Close();
                }
        }


        public void Dispose()
        {
            watcher.Dispose();
            if (process != null)
            {
                Stop();
            }
        }

        public string Name { get { return name; } }
    }

}