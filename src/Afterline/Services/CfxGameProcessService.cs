using System.Diagnostics;

namespace Afterline.Services;

public enum CfxGame
{
    None,
    FiveM,
    RedM
}

// Detects a running CitizenFX game. FiveM runs as FiveM.exe plus FiveM_*
// subprocesses; RedM runs as RedM.exe plus RedM_* subprocesses. Both share the
// same NUI chat stack, so capture works the same way for either.
public static class CfxGameProcessService
{
    private const long CacheDurationMilliseconds = 750;
    private static readonly object CacheGate = new();
    private static long _cachedAt = long.MinValue;
    private static CfxGame _cachedResult;

    public static bool IsRunning() => Detect() != CfxGame.None;

    public static bool Refresh()
    {
        lock (CacheGate)
        {
            _cachedAt = long.MinValue;
        }

        return IsRunning();
    }

    public static CfxGame Detect()
    {
        long now = Environment.TickCount64;
        lock (CacheGate)
        {
            if (_cachedAt != long.MinValue && now - _cachedAt < CacheDurationMilliseconds)
                return _cachedResult;

            _cachedResult = DetectRunning();
            _cachedAt = now;
            return _cachedResult;
        }
    }

    public static string DisplayName(CfxGame game) => game switch
    {
        CfxGame.FiveM => "FiveM",
        CfxGame.RedM => "RedM",
        _ => "FiveM or RedM"
    };

    public static CfxGame Classify(string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return CfxGame.None;
        if (processName.StartsWith("FiveM", StringComparison.OrdinalIgnoreCase)) return CfxGame.FiveM;
        if (processName.StartsWith("RedM", StringComparison.OrdinalIgnoreCase)) return CfxGame.RedM;
        return CfxGame.None;
    }

    private static CfxGame DetectRunning()
    {
        Process[] processes = Array.Empty<Process>();
        try
        {
            processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    CfxGame game = Classify(process.ProcessName);
                    if (game != CfxGame.None)
                        return game;
                }
                catch
                {
                    // A process can exit between enumeration and inspection.
                }
            }
            return CfxGame.None;
        }
        catch (Exception ex)
        {
            DiagnosticLogger.Error("Unable to enumerate processes.", ex);
            return CfxGame.None;
        }
        finally
        {
            foreach (Process process in processes)
                process.Dispose();
        }
    }
}
