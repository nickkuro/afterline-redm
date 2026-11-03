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

    // Picks one game from the running process names. When processes of both
    // games exist (a leftover launcher or helper, for example), the game with
    // a running game subprocess wins; if that doesn't settle it, the
    // previously detected game is kept so the status doesn't flip between
    // polls, and FiveM is the final tie-break.
    internal static CfxGame Choose(IEnumerable<string> processNames, CfxGame previous)
    {
        bool fiveM = false, redM = false, fiveMGame = false, redMGame = false;
        foreach (string name in processNames)
        {
            CfxGame game = Classify(name);
            bool isGameProcess = IsGameSubprocess(name);
            if (game == CfxGame.FiveM) { fiveM = true; fiveMGame |= isGameProcess; }
            else if (game == CfxGame.RedM) { redM = true; redMGame |= isGameProcess; }
        }

        if (fiveM != redM) return fiveM ? CfxGame.FiveM : CfxGame.RedM;
        if (!fiveM) return CfxGame.None;
        if (fiveMGame != redMGame) return fiveMGame ? CfxGame.FiveM : CfxGame.RedM;
        return previous == CfxGame.RedM ? CfxGame.RedM : CfxGame.FiveM;
    }

    private static bool IsGameSubprocess(string name)
        => name.Contains("GTAProcess", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("GameProcess", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("RDR", StringComparison.OrdinalIgnoreCase);

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
            var names = new List<string>();
            foreach (Process process in processes)
            {
                try
                {
                    names.Add(process.ProcessName);
                }
                catch
                {
                    // A process can exit between enumeration and inspection.
                }
            }
            return Choose(names, _cachedResult);
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
