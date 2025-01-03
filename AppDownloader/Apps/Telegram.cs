using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;

namespace AppDownloader.Apps
{
    public class Telegram
    {
        private const string ProgramName = "Telegram"; // Name of the program
        private const string ExecutableName = "Telegram.exe"; // Name of the executable file
        private const string DownloadUrl = "https://github.com/telegramdesktop/tdesktop/releases/download/v5.9.0/tsetup-x64.5.9.0.exe"; // URL to download the installer
        private readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "tsetup-x64.5.9.0.exe"); // Path to the installer file

        // Check if the program is installed
        public bool IsInstalled()
        {
            return FindProgramExecutable(ExecutableName, null) || IsProgramInstalled(ProgramName);
        }

        // Download the program
        public void Download()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(DownloadUrl, InstallerPath);
            }
        }

        // Install the program
        public void Install()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = InstallerPath,
                Arguments = "/S", // Silent install
                UseShellExecute = true,
                Verb = "runas" // Run as administrator
            };
        
            using (var process = Process.Start(startInfo))
            {
                process?.WaitForExit();
            }
        }

        // Method to check if the program is installed
        private bool IsProgramInstalled(string programName)
        {
            string[] registryPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            // Iterate through the registry paths
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

        // Method to find the program executable
        public bool FindProgramExecutable(string executableName, Action<int> progressCallback)
        {
            // Define common paths to search for the executable
            var commonPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Telegram"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Telegram"),
                @"C:\Program Files\Telegram",
                @"C:\Program Files (x86)\Telegram"
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
            // Use a breadth-first search to traverse the file system
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