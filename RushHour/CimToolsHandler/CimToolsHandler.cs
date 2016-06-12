using CimToolsRushHour.v2;
using System.Reflection;

namespace RushHour.CimToolsHandler
{
    internal static class CimToolsHandler
    {
        internal static CimToolBase CimToolBase = new CimToolBase(new CimToolSettings("Rush Hour", "RushHour", Assembly.GetExecutingAssembly(), 605590542));
        internal static CimToolsRushHour.Legacy.CimToolBase LegacyCimToolBase = new CimToolsRushHour.Legacy.CimToolBase(new CimToolsRushHour.Legacy.CimToolSettings("RushHour", Assembly.GetExecutingAssembly(), 605590542));
    }
}
