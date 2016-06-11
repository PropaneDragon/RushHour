using CimToolsWorkshop.v2;
using System.Reflection;

namespace RushHour.CimToolsHandler
{
    internal static class CimToolsHandler
    {
        internal static CimToolBase CimToolBase = new CimToolBase(new CimToolSettings("Rush Hour", "RushHour", Assembly.GetExecutingAssembly(), 605590542));
        internal static CimToolsWorkshop.Legacy.CimToolBase LegacyCimToolBase = new CimToolsWorkshop.Legacy.CimToolBase(new CimToolsWorkshop.Legacy.CimToolSettings("RushHour", Assembly.GetExecutingAssembly(), 605590542));
    }
}
