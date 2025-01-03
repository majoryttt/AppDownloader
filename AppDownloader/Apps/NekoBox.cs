using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;

namespace AppDownloader.Apps
{
    public class NekoBox
    {
        private const string ProgramName = "NekoBox";
        private const string ExecutableName = "nekobox.exe";
        private const string DownloadUrl = "https://github.com/MatsuriDayo/nekoray/releases/download/4.0.1/nekoray-4.0.1-2024-12-12-windows64.zip";
        private readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "nekoray-4.0.1-2024-12-12-windows64.zip");

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
                Arguments = "/S", // Silent install
                UseShellExecute = true,
                Verb = "runas" // Run as administrator
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
            var commonPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NekoBox"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NekoBox"),
                @"C:\Program Files\NekoBox",
                @"C:\Program Files (x86)\NekoBox"
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