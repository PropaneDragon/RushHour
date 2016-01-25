using System;
using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class ComicExpo : CityEvent
    {
        public ComicExpo()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "YES! FINALLY! An actually exciting #event going on at the Expo Center. #comicbooks"
            };

            m_eventStartedMessages = new List<string>()
            {
                "The doors have opened! #comicbooks #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "Shame it closes so early. I still had tons of places to visit #comicbooks #event"
            };
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);
            Citizen.Gender _citizenGender = Citizen.GetGender(citizenID);

            int percentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100);

            return (_citizenGender == Citizen.Gender.Male || (_citizenGender == Citizen.Gender.Female && percentage < 20)) &&
                    _citizenAge == Citizen.AgeGroup.Young || _citizenAge == Citizen.AgeGroup.Teen;
        }

        public override int GetCapacity()
        {
            return 4000;
        }

        public override double GetEventLength()
        {
            return 3D;
        }
    }
}
