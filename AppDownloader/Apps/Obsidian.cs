using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;

namespace AppDownloader.Apps
{
    public class Obsidian
    {
        private const string ProgramName = "Obsidian";
        private const string ExecutableName = "Obsidian.exe";
        private const string DownloadUrl = "https://github.com/obsidianmd/obsidian-releases/releases/download/v1.5.3/Obsidian.1.5.3.exe";
        private readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "Obsidian.1.5.3.exe");
        
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
            var commonPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Obsidian"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Obsidian"),
                @"C:\Program Files\Obsidian",
                @"C:\Program Files (x86)\Obsidian"
            };

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
