﻿using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;

namespace AppDownloader.Apps
{
    public class Steam
    {
        // Constants for Steam application
        private const string ProgramName = "Steam";
        private const string ExecutableName = "Steam.exe";
        private const string DownloadUrl = "https://cdn.akamai.steamstatic.com/client/installer/SteamSetup.exe";
        private readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "SteamSetup.exe");

        // Check if Steam is installed using file search or registry
        public bool IsInstalled()
        {
            return FindProgramExecutable(ExecutableName, null) || IsProgramInstalled(ProgramName);
        }

        // Download Steam installer from official source
        public void Download()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(DownloadUrl, InstallerPath);
            }
        }

        // Install Steam silently using downloaded installer
        public void Install()
        {
            Process process = new Process();
            process.StartInfo.FileName = InstallerPath;
            process.StartInfo.Arguments = "/S"; // Silent installation
            process.Start();
            process.WaitForExit();
        }

        // Check Windows Registry for Steam installation
        private bool IsProgramInstalled(string programName)
        {
            string[] registryPaths = {
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

        // Search for Steam executable with progress tracking
        public bool FindProgramExecutable(string executableName, Action<int> progressCallback)
        {
            // Common Steam installation paths
            var commonPaths = new List<string>
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"),
                @"D:\Steam",
                @"E:\Steam"
            };

            // Quick check in common locations first
            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path) && File.Exists(Path.Combine(path, executableName)))
                {
                    return true;
                }
            }

            // Deep search if not found in common locations
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

        // Efficient directory search implementation
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

                    // Check if executable exists in current directory
                    if (File.Exists(execPath))
                    {
                        return true;
                    }

                    // Add subdirectories to search queue
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