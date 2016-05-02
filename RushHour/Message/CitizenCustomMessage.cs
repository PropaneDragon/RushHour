using ColossalFramework;
using ColossalFramework.IO;

namespace RushHour.Message
{
    internal class CitizenCustomMessage : MessageBase
    {
        public string m_message;
        public uint m_senderID;

        public CitizenCustomMessage(uint senderID, string message)
        {
            m_message = message;
            m_senderID = senderID;
        }

        public override uint GetSenderID()
        {
            return m_senderID;
        }

        public override string GetSenderName()
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            return instance.GetCitizenName(m_senderID) ?? instance.GetDefaultCitizenName(m_senderID);
        }

        public override string GetText()
        {
            return m_message;
        }

        public override bool IsSimilarMessage(MessageBase other)
        {
            return false;
        }

        public override void Serialize(DataSerializer s)
        {
            s.WriteSharedString(m_message);
            s.WriteUInt32(m_senderID);
        }

        public override void Deserialize(DataSerializer s)
        {
            m_message = s.ReadSharedString();
            m_senderID = s.ReadUInt32();
        }

        public override void AfterDeserialize(DataSerializer s)
        {
        }
    }
}
