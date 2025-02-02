using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;
using System.Collections.Generic;

namespace AppDownloader.Apps
{
    public class SteelSeriesGG: IApp
    {
        private const string ProgramName = "SteelSeries GG";
        private const string ExecutableName = "SteelSeriesGG.exe";
        private const string DownloadUrl = "https://steelseries.com/gg/downloads/gg/latest/windows";
        private readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "SteelSeriesGGSetup.exe");
        
        public bool IsInstalled()
        {
            return FindProgramExecutable(ExecutableName, null) || IsProgramInstalled(ProgramName);
        }
        
        public void Download()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(DownloadUrl, InstallerPath);
            }
        }

        public void Install()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = InstallerPath,
                Arguments = "/S",
                UseShellExecute = true,
                Verb = "runas"
            };
        
            using (var process = Process.Start(startInfo))
            {
                process?.WaitForExit();
            }
        }

        private bool IsProgramInstalled(string programName)
        {
            string[] registryPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var registryPath in registryPaths)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        foreach (string subkeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                            {
                                string displayName = subkey?.GetValue("DisplayName") as string;
                                if (!string.IsNullOrEmpty(displayName) &&
                                    displayName.Contains(programName, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
        public bool FindProgramExecutable(string executableName, Action<int> progressCallback)
        {
            string exactPath = @"C:\Program Files\SteelSeries\GG\SteelSeriesGG.exe";
            if (File.Exists(exactPath))
            {
                progressCallback?.Invoke(100);
                return true;
            }

            var commonPaths = new List<string>
            {
                @"C:\Program Files\SteelSeries\GG",
                @"C:\Program Files (x86)\SteelSeries\GG",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SteelSeries", "GG"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "SteelSeries", "GG")
            };

            foreach (var path in commonPaths)
            {
                string execPath = Path.Combine(path, executableName);
                Debug.WriteLine($"Checking path: {execPath}");
                if (File.Exists(execPath))
                {
                    progressCallback?.Invoke(100);
                    return true;
                }
            }

            progressCallback?.Invoke(100);
            return false;
        }
        private bool QuickSearchInDrive(string drivePath, string executableName)
        {
            try
            {
                var searchQueue = new Queue<string>();
                searchQueue.Enqueue(drivePath);

                while (searchQueue.Count > 0)
                {
                    string currentPath = searchQueue.Dequeue();
                    string execPath = Path.Combine(currentPath, executableName);

                    if (File.Exists(execPath))
                    {
                        return true;
                    }

                    try
                    {
                        foreach (string dir in Directory.GetDirectories(currentPath))
                        {
                            if (!dir.Contains("Windows") && !dir.Contains("$Recycle.Bin"))
                            {
                                searchQueue.Enqueue(dir);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}