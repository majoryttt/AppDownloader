using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;

namespace AppDownloader.Apps
{
    public class Chrome : IApp
    {
        // Constants for Chrome application
        private const string ProgramName = "Chrome";
        private const string ExecutableName = "chrome.exe";
        private const string DownloadUrl = "https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7B416DC6D3-8AD8-9476-8690-5A5C5C97A3FF%7D%26lang%3Den%26browser%3D4%26usagestats%3D1%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-statsdef_1%26brand%3DJJTC%26installdataindex%3Dempty/update2/installers/ChromeSetup.exe";
        private readonly string InstallerPath = Path.Combine(Path.GetTempPath(), "ChromeSetup.exe");

        // Check if Chrome is installed using file search or registry
        public bool IsInstalled()
        {
            return FindProgramExecutable(ExecutableName, null) || IsProgramInstalled(ProgramName);
        }

        // Download Chrome installer from official source
        public void Download()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(DownloadUrl, InstallerPath);
            }
        }

        // Install Chrome silently using downloaded installer
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

        // Check Windows Registry for Chrome installation
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

        // Search for Chrome executable with progress tracking
        public bool FindProgramExecutable(string executableName, Action<int> progressCallback)
        {
            // Common DeepL installation paths
            var commonPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Chrome"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chrome"),
                @"C:\Program Files\Chrome",
                @"C:\Program Files (x86)\Chrome"
            };

            // Quick check in common locations first
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

        // Efficient directory search implementation using queue
        private bool QuickSearchInDrive(string drivePath, string executableName)
        {
            try
            {
                // Initialize search queue with drive path
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

                    // Add subdirectories to search queue, skipping system folders
                    try
                    {
                        foreach (string dir in Directory.GetDirectories(currentPath))
                        {
                            // Skip Windows and Recycle Bin folders for efficiency
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
