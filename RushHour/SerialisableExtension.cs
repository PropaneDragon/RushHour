using ICities;
using RushHour.CimTools;

namespace RushHour
{
    public class SerialisableExtension : SerializableDataExtensionBase
    {
        public override void OnSaveData()
        {
            CimToolsHandler.CimToolBase.SaveFileOptions.OnSaveData(serializableDataManager, CimToolsHandler.CimToolBase.ModSettings);
        }

        public override void OnLoadData()
        {
            CimToolsHandler.CimToolBase.SaveFileOptions.OnLoadData(serializableDataManager, CimToolsHandler.CimToolBase.ModSettings);
        }
    }
}
