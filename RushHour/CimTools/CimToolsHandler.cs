using CimTools;
using CimTools.V1;
using System.Reflection;

namespace RushHour.CimTools
{
    public static class CimToolsHandler
    {
        public static CimToolBase CimToolBase = new CimToolBase(new CimToolSettings("RushHour", Assembly.GetExecutingAssembly(), 605590542));
    }
}
