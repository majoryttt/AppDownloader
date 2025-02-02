namespace AppDownloader.Apps;

public interface IApp
{
    void Download();
    void Install();
    bool IsInstalled();
    bool FindProgramExecutable(string executableName, Action<int> progressCallback);
}