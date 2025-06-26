using System.Runtime.InteropServices;

namespace Perform.Web;

public static class Caffeine
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint EsContinuous = 0x80000000;
    private const uint EsSystemRequired = 0x00000001;
    private const uint EsAwayModeRequired = 0x00000040;

    public static void PreventSleep()
    {
        SetThreadExecutionState(EsContinuous | EsSystemRequired | EsAwayModeRequired);
    }

    public static void AllowSleep()
    {
        SetThreadExecutionState(EsContinuous);
    }
}