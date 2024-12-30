using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;

namespace AppDownloader.Apps
{
    // Discord class
    public class PowerToys
    {
        private const string ProgramName = "PowerToys"; // Name of the program
        private const string ExecutableName = "PowerToys.exe"; // Name of the executable file
        private const string DownloadUrl = "https://github.com/microsoft/PowerToys/releases/download/v0.87.1/PowerToysSetup-0.87.1-x64.exe"; // URL to download the installer
        private readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "PowerToysSetup-0.87.1-x64.exe"); // Path to the installer file

        // Method to check if the program is installed
        public bool IsInstalled()
        {
            return FindProgramExecutable(ExecutableName, null) || IsProgramInstalled(ProgramName);
        }

        // Method to download the program
        public void Download()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(DownloadUrl, InstallerPath);
            }
        }

        // Method to install the program
        public void Install()
        {
            Process process = new Process();
            process.StartInfo.FileName = InstallerPath;
            process.StartInfo.Arguments = "/silent";
            process.Start();
            process.WaitForExit();
        }

        // Method to check if the program is installed
        private bool IsProgramInstalled(string programName)
        {
            string[] registryPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            // Loop through the registry paths
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

        // Method to find the executable file
        public bool FindProgramExecutable(string executableName, Action<int> progressCallback)
        {
            // Define common paths to search for the executable file
            var commonPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PowerToys"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerToys"),
                @"C:\Program Files\PowerToys",
                @"C:\Program Files (x86)\PowerToys"
            };

            // Quick check in common locations
            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path))
                {
                    string execPath = Path.Combine(path, executableName);
                    if (File.Exists(execPath))
                    {
                        progressCallback?.Invoke(100);
                        return true;
                    }
                }
            }

            // Deep search in system drives if not found in common locations
            try
            {
                string[] drives = Directory.GetLogicalDrives();
                int totalDrives = drives.Length;

                for (int i = 0; i < totalDrives; i++)
                {
                    string drive = drives[i];
                    if (!Directory.Exists(drive)) continue;

                    progressCallback?.Invoke((i * 100) / totalDrives);

                    if (QuickSearchInDrive(drive, executableName))
                    {
                        progressCallback?.Invoke(100);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                progressCallback?.Invoke(100);
                return false;
            }

            progressCallback?.Invoke(100);
            return false;
        }

        // Method to perform a quick search in a drive
        private bool QuickSearchInDrive(string drivePath, string executableName)
        {
            // Perform a quick search in the drive
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