using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Steamworks;
using ColossalFramework.Threading;
using RushHour.CitizenHandlers;
using System;
using UnityEngine;

namespace RushHour.ResidentHandlers
{
    public static class NewResidentAI
    {
        private static void UpdateLocation(ResidentAI resident, uint citizenID, ref Citizen data)
        {
            try
            {
                CitizenManager _citizenManager = Singleton<CitizenManager>.instance;

                if (data.m_homeBuilding == 00 && data.m_workBuilding == 0 && data.m_visitBuilding == 0 && data.m_instance == 0 && data.m_vehicle == 0)
                {
                    _citizenManager.ReleaseCitizen(citizenID);
                }
                else
                {
                    switch (data.CurrentLocation)
                    {
                        case Citizen.Location.Home:
                            if (!CitizenLocationHandler.ProcessHome(citizenID, ref data))
                            {
                                return;
                            }
                            break;
                        case Citizen.Location.Work:
                            if (!CitizenLocationHandler.ProcessWork(citizenID, ref data))
                            {
                                return;
                            }
                            break;
                        case Citizen.Location.Visit:
                            if (!CitizenLocationHandler.ProcessVisit(citizenID, ref data))
                            {
                                return;
                            }
                            break;
                        case Citizen.Location.Moving:
                            if (!CitizenLocationHandler.ProcessMoving(citizenID, ref data))
                            {
                                return;
                            }
                            break;
                    }

                    data.m_flags &= ~Citizen.Flags.NeedGoods;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error on " + citizenID);
                Debug.LogException(ex);
            }
        }

        public static void SimulationStep(uint citizenID, ref Citizen data)
        {
            if (!data.Dead && UpdateAge(citizenID, ref data))
                return;
            if (!data.Dead)
                UpdateHome(citizenID, ref data);
            if (!data.Sick && !data.Dead)
            {
                if (UpdateHealth(citizenID, ref data))
                    return;
                UpdateWellbeing(citizenID, ref data);
                UpdateWorkplace(citizenID, ref data);
            }
            UpdateLocation(citizenID, ref data);
        }

        private static bool UpdateAge(uint citizenID, ref Citizen data)
        {
            int num = data.Age + 1;
            if (num <= 45)
            {
                if (num == 15 || num == 45)
                    FinishSchoolOrWork(citizenID, ref data);
            }
            else if (num == 90 || num == 180)
                FinishSchoolOrWork(citizenID, ref data);
            else if ((data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None && num % 15 == 0)
                FinishSchoolOrWork(citizenID, ref data);
            if ((data.m_flags & Citizen.Flags.Original) != Citizen.Flags.None)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                if (instance.m_tempOldestOriginalResident < num)
                    instance.m_tempOldestOriginalResident = num;
                if (num == 240)
                    Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.FullLifespans).Add(1);
            }
            data.Age = num;
            if (num >= 240 && data.CurrentLocation != Citizen.Location.Moving && ((int)data.m_vehicle == 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(240, (int)byte.MaxValue) <= num))
            {
                Die(citizenID, ref data);
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                {
                    Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    return true;
                }
            }
            return false;
        }

        private static void UpdateWorkplace(uint citizenID, ref Citizen data)
        {
            if ((int)data.m_workBuilding != 0 || (int)data.m_homeBuilding == 0)
                return;
            Vector3 worldPos = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_homeBuilding].m_position;
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(worldPos);
            DistrictPolicies.Services services = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
            int age = data.Age;
            TransferManager.TransferReason material = TransferManager.TransferReason.None;
            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                    if (!data.Education1)
                    {
                        material = TransferManager.TransferReason.Student1;
                        break;
                    }
                    break;
                case Citizen.AgeGroup.Teen:
                    if (!data.Education2)
                    {
                        material = TransferManager.TransferReason.Student2;
                        break;
                    }
                    break;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (!data.Education3)
                    {
                        material = TransferManager.TransferReason.Student3;
                        break;
                    }
                    break;
            }
            if (data.Unemployed != 0 && ((services & DistrictPolicies.Services.EducationBoost) == DistrictPolicies.Services.None || material != TransferManager.TransferReason.Student3 || age % 5 > 2))
            {
                TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
                offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8U);
                offer.Citizen = citizenID;
                offer.Position = worldPos;
                offer.Amount = 1;
                offer.Active = true;
                switch (data.EducationLevel)
                {
                    case Citizen.Education.Uneducated:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker0, offer);
                        break;
                    case Citizen.Education.OneSchool:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker1, offer);
                        break;
                    case Citizen.Education.TwoSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker2, offer);
                        break;
                    case Citizen.Education.ThreeSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker3, offer);
                        break;
                }
            }
            if (material == TransferManager.TransferReason.None || material == TransferManager.TransferReason.Student3 && (services & DistrictPolicies.Services.SchoolsOut) != DistrictPolicies.Services.None && age % 5 <= 1)
                return;
            Singleton<TransferManager>.instance.AddOutgoingOffer(material, new TransferManager.TransferOffer()
            {
                Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8U),
                Citizen = citizenID,
                Position = worldPos,
                Amount = 1,
                Active = true
            });
        }

        private static void UpdateWellbeing(uint citizenID, ref Citizen data)
        {
            if ((int)data.m_homeBuilding == 0)
                return;
            int num1 = 0;
            BuildingManager instance1 = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance1.m_buildings.m_buffer[(int)data.m_homeBuilding].Info;
            Vector3 vector3 = instance1.m_buildings.m_buffer[(int)data.m_homeBuilding].m_position;
            ItemClass itemClass = info.m_class;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(vector3);
            DistrictPolicies.Services services = instance2.m_districts.m_buffer[(int)district].m_servicePolicies;
            DistrictPolicies.Taxation taxationPolicies = instance2.m_districts.m_buffer[(int)district].m_taxationPolicies;
            int num2 = (int)data.m_health;
            if (num2 > 80)
                num1 += 10;
            else if (num2 > 60)
                num1 += 5;
            int num3 = num1 - Mathf.Clamp(50 - num2, 0, 30);
            if ((services & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None)
                num3 -= 5;
            if ((services & DistrictPolicies.Services.SmokingBan) != DistrictPolicies.Services.None)
                num3 -= 15;
            if ((int)instance1.m_buildings.m_buffer[(int)data.m_homeBuilding].GetLastFrameData().m_fireDamage != 0)
                num3 -= 15;
            Citizen.Wealth wealthLevel = data.WealthLevel;
            Citizen.AgePhase agePhase = Citizen.GetAgePhase(data.EducationLevel, data.Age);
            int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(itemClass, taxationPolicies);
            int num4 = (int)(8 - wealthLevel);
            int num5 = (int)(11 - wealthLevel);
            if (itemClass.m_subService == ItemClass.SubService.ResidentialHigh)
            {
                ++num4;
                ++num5;
            }
            if (taxRate < num4)
                num3 += num4 - taxRate;
            if (taxRate > num5)
                num3 -= taxRate - num5;
            int departmentRequirement1 = Citizen.GetPoliceDepartmentRequirement(agePhase);
            if (departmentRequirement1 != 0)
            {
                int local;
                int total;
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.PoliceDepartment, vector3, out local, out total);
                if (local != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(local, departmentRequirement1, 500, 20, 40);
                if (total != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(total, departmentRequirement1 >> 1, 250, 5, 20);
            }
            int departmentRequirement2 = Citizen.GetFireDepartmentRequirement(agePhase);
            if (departmentRequirement2 != 0)
            {
                int local;
                int total;
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.FireDepartment, vector3, out local, out total);
                if (local != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(local, departmentRequirement2, 500, 20, 40);
                if (total != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(total, departmentRequirement2 >> 1, 250, 5, 20);
            }
            int educationRequirement = Citizen.GetEducationRequirement(agePhase);
            if (educationRequirement != 0)
            {
                int local;
                int total;
                if (agePhase < Citizen.AgePhase.Teen0)
                {
                    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationElementary, vector3, out local, out total);
                    if (local > 1000 && !data.Education1 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local - 1000)
                        data.Education1 = true;
                }
                else if (agePhase < Citizen.AgePhase.Young0)
                {
                    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationHighSchool, vector3, out local, out total);
                    if (local > 1000 && !data.Education2 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local - 1000)
                        data.Education2 = true;
                }
                else
                {
                    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationUniversity, vector3, out local, out total);
                    if (local > 1000 && !data.Education3 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000U) < local - 1000)
                        data.Education3 = true;
                }
                if (local != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(local, educationRequirement, 500, 20, 40);
                if (total != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(total, educationRequirement >> 1, 250, 5, 20);
            }
            int entertainmentRequirement = Citizen.GetEntertainmentRequirement(agePhase);
            if (entertainmentRequirement != 0)
            {
                int local;
                int total;
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.Entertainment, vector3, out local, out total);
                if (local != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(local, entertainmentRequirement, 500, 30, 60);
                if (total != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(total, entertainmentRequirement >> 1, 250, 10, 40);
            }
            int transportRequirement = Citizen.GetTransportRequirement(agePhase);
            if (transportRequirement != 0)
            {
                int local;
                int total;
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.PublicTransport, vector3, out local, out total);
                if (local != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(local, transportRequirement, 500, 20, 40);
                if (total != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(total, transportRequirement >> 1, 250, 5, 20);
            }
            int deathCareRequirement = Citizen.GetDeathCareRequirement(agePhase);
            int local1;
            int total1;
            Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.DeathCare, vector3, out local1, out total1);
            if (deathCareRequirement != 0)
            {
                if (local1 != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(local1, deathCareRequirement, 500, 10, 20);
                if (total1 != 0)
                    num3 += ImmaterialResourceManager.CalculateResourceEffect(total1, deathCareRequirement >> 1, 250, 3, 10);
            }
            bool electricity;
            Singleton<ElectricityManager>.instance.CheckElectricity(vector3, out electricity);
            if (electricity)
            {
                num3 += 12;
                data.NoElectricity = 0;
            }
            else
            {
                int noElectricity = data.NoElectricity;
                if (noElectricity < 2)
                    data.NoElectricity = noElectricity + 1;
                else
                    num3 -= 5;
            }
            bool flag = Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment);
            int workRequirement = Citizen.GetWorkRequirement(agePhase);
            if (workRequirement != 0)
            {
                if ((int)data.m_workBuilding == 0)
                {
                    int unemployed = data.Unemployed;
                    num3 -= unemployed * workRequirement / 100;
                    data.Unemployed = !flag ? Mathf.Min(1, unemployed + 1) : unemployed + 1;
                }
                else
                    data.Unemployed = 0;
            }
            else
                data.Unemployed = 0;
            int wellbeing = Mathf.Clamp(num3, 0, 100);
            data.m_wellbeing = (byte)wellbeing;
            if (flag)
            {
                Randomizer randomizer = new Randomizer((uint)((int)citizenID * 7931 + 123));
                int num6 = Mathf.Min(Citizen.GetMaxCrimeRate(Citizen.GetWellbeingLevel(data.EducationLevel, wellbeing)), Citizen.GetCrimeRate(data.Unemployed));
                data.Criminal = randomizer.Int32(500U) < num6;
            }
            else
                data.Criminal = false;
        }

        private static bool UpdateHealth(uint citizenID, ref Citizen data)
        {
            if ((int)data.m_homeBuilding == 0)
                return false;
            int num1 = 20;
            BuildingManager instance1 = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance1.m_buildings.m_buffer[(int)data.m_homeBuilding].Info;
            Vector3 vector3 = instance1.m_buildings.m_buffer[(int)data.m_homeBuilding].m_position;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(vector3);
            if ((instance2.m_districts.m_buffer[(int)district].m_servicePolicies & DistrictPolicies.Services.SmokingBan) != DistrictPolicies.Services.None)
                num1 += 10;
            int amount;
            int max;
            info.m_buildingAI.GetMaterialAmount(data.m_homeBuilding, ref instance1.m_buildings.m_buffer[(int)data.m_homeBuilding], TransferManager.TransferReason.Garbage, out amount, out max);
            int num2 = amount / 1000;
            if (num2 <= 2)
                num1 += 12;
            else if (num2 >= 4)
                num1 -= num2 - 3;
            int healthCareRequirement = Citizen.GetHealthCareRequirement(Citizen.GetAgePhase(data.EducationLevel, data.Age));
            int local1;
            int total;
            Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.HealthCare, vector3, out local1, out total);
            if (healthCareRequirement != 0)
            {
                if (local1 != 0)
                    num1 += ImmaterialResourceManager.CalculateResourceEffect(local1, healthCareRequirement, 500, 20, 40);
                if (total != 0)
                    num1 += ImmaterialResourceManager.CalculateResourceEffect(total, healthCareRequirement >> 1, 250, 5, 20);
            }
            int local2;
            Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.NoisePollution, vector3, out local2);
            if (local2 != 0)
                num1 -= local2 * 100 / (int)byte.MaxValue;
            int local3;
            Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.CrimeRate, vector3, out local3);
            if (local3 > 3)
            {
                if (local3 <= 30)
                    num1 -= 2;
                else if (local3 <= 70)
                    num1 -= 5;
                else
                    num1 -= 15;
            }
            bool water;
            bool sewage;
            byte waterPollution;
            Singleton<WaterManager>.instance.CheckWater(vector3, out water, out sewage, out waterPollution);
            if (water)
            {
                num1 += 12;
                data.NoWater = 0;
            }
            else
            {
                int noWater = data.NoWater;
                if (noWater < 2)
                    data.NoWater = noWater + 1;
                else
                    num1 -= 5;
            }
            if (sewage)
            {
                num1 += 12;
                data.NoSewage = 0;
            }
            else
            {
                int noSewage = data.NoSewage;
                if (noSewage < 2)
                    data.NoSewage = noSewage + 1;
                else
                    num1 -= 5;
            }
            int num3 = (int)waterPollution >= 35 ? num1 - ((int)waterPollution * 2 - 35) : num1 - (int)waterPollution;
            byte groundPollution;
            Singleton<NaturalResourceManager>.instance.CheckPollution(vector3, out groundPollution);
            int num4 = Mathf.Clamp(num3 - (int)groundPollution * 100 / (int)byte.MaxValue, 0, 100);
            data.m_health = (byte)num4;
            int num5 = 0;
            if (num4 <= 10)
            {
                int badHealth = data.BadHealth;
                if (badHealth < 3)
                {
                    num5 = 15;
                    data.BadHealth = badHealth + 1;
                }
                else
                    num5 = total != 0 ? 50 : 75;
            }
            else if (num4 <= 25)
            {
                data.BadHealth = 0;
                num5 += 10;
            }
            else if (num4 <= 50)
            {
                data.BadHealth = 0;
                num5 += 3;
            }
            else
                data.BadHealth = 0;
            if (data.CurrentLocation != Citizen.Location.Moving && (int)data.m_vehicle == 0 && (num5 != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) < num5))
            {
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
                {
                    Die(citizenID, ref data);
                    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        return true;
                    }
                }
                else
                    data.Sick = true;
            }
            return false;
        }

        private static void UpdateHome(uint citizenID, ref Citizen data)
        {
            if ((int)data.m_homeBuilding != 0 || (data.m_flags & Citizen.Flags.DummyTraffic) != Citizen.Flags.None)
                return;
            TransferManager.TransferOffer offer = new TransferManager.TransferOffer();
            offer.Priority = 7;
            offer.Citizen = citizenID;
            offer.Amount = 1;
            offer.Active = true;
            if ((int)data.m_workBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                offer.Position = instance.m_buildings.m_buffer[(int)data.m_workBuilding].m_position;
            }
            else
            {
                offer.PositionX = Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);
                offer.PositionZ = Singleton<SimulationManager>.instance.m_randomizer.Int32(256U);
            }
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
            {
                switch (data.EducationLevel)
                {
                    case Citizen.Education.Uneducated:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0, offer);
                        break;
                    case Citizen.Education.OneSchool:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1, offer);
                        break;
                    case Citizen.Education.TwoSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2, offer);
                        break;
                    case Citizen.Education.ThreeSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3, offer);
                        break;
                }
            }
            else
            {
                switch (data.EducationLevel)
                {
                    case Citizen.Education.Uneducated:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0B, offer);
                        break;
                    case Citizen.Education.OneSchool:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1B, offer);
                        break;
                    case Citizen.Education.TwoSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2B, offer);
                        break;
                    case Citizen.Education.ThreeSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3B, offer);
                        break;
                }
            }
        }

        private static void FinishSchoolOrWork(uint citizenID, ref Citizen data)
        {
            if ((int)data.m_workBuilding == 0)
                return;
            if (data.CurrentLocation == Citizen.Location.Work && (int)data.m_homeBuilding != 0)
                StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
            BuildingManager instance1 = Singleton<BuildingManager>.instance;
            CitizenManager instance2 = Singleton<CitizenManager>.instance;
            uint num1 = instance1.m_buildings.m_buffer[(int)data.m_workBuilding].m_citizenUnits;
            int num2 = 0;
            while ((int)num1 != 0)
            {
                uint num3 = instance2.m_units.m_buffer[num1].m_nextUnit;
                CitizenUnit.Flags flags = instance2.m_units.m_buffer[num1].m_flags;
                if ((flags & (CitizenUnit.Flags.Work | CitizenUnit.Flags.Student)) != CitizenUnit.Flags.None)
                {
                    if ((flags & CitizenUnit.Flags.Student) != CitizenUnit.Flags.None)
                    {
                        if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[num1]))
                        {
                            BuildingInfo info = instance1.m_buildings.m_buffer[(int)data.m_workBuilding].Info;
                            if (info.m_buildingAI.GetEducationLevel1())
                                data.Education1 = true;
                            if (info.m_buildingAI.GetEducationLevel2())
                                data.Education2 = true;
                            if (info.m_buildingAI.GetEducationLevel3())
                                data.Education3 = true;
                            data.m_workBuilding = (ushort)0;
                            data.m_flags &= ~Citizen.Flags.Student;
                            if ((data.m_flags & Citizen.Flags.Original) == Citizen.Flags.None || data.EducationLevel != Citizen.Education.ThreeSchools || (instance2.m_fullyEducatedOriginalResidents++ != 0 || Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements == SimulationMetaData.MetaBool.True))
                                break;
                            ThreadHelper.dispatcher.Dispatch((System.Action)(() =>
                            {
                                if (Steam.achievements["ClimbingTheSocialLadder"].achieved)
                                    return;
                                Steam.achievements["ClimbingTheSocialLadder"].Unlock();
                            }));
                            break;
                        }
                    }
                    else if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[num1]))
                    {
                        data.m_workBuilding = (ushort)0;
                        data.m_flags &= ~Citizen.Flags.Student;
                        break;
                    }
                }
                num1 = num3;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        private static void Die(uint citizenID, ref Citizen data)
        {
            data.Sick = false;
            data.Dead = true;
            data.SetParkedVehicle(citizenID, (ushort)0);
            if ((data.m_flags & Citizen.Flags.MovingIn) != Citizen.Flags.None)
                return;
            ushort num = data.GetBuildingByLocation();
            if ((int)num == 0)
                num = data.m_homeBuilding;
            if ((int)num == 0)
                return;
            DistrictManager instance = Singleton<DistrictManager>.instance;
            Vector3 worldPos = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num].m_position;
            byte district = instance.GetDistrict(worldPos);
            ++instance.m_districts.m_buffer[(int)district].m_deathData.m_tempCount;
        }

        /// <summary>
        /// Determines whether to make the person move. Less likely to want to go
        /// anywhere if there's a lot of people already out and about.
        /// </summary>
        /// <returns>Whether to move</returns>
        public static bool DoRandomMove()
        {
            uint vehiclesInGame = (uint)Singleton<VehicleManager>.instance.m_vehicleCount;
            uint peopleInGame = (uint)Singleton<CitizenManager>.instance.m_instanceCount;
            if (vehiclesInGame * 65536U > peopleInGame * 16384U)
                return Singleton<SimulationManager>.instance.m_randomizer.UInt32(16384U) > vehiclesInGame;
            return Singleton<SimulationManager>.instance.m_randomizer.UInt32(65536U) > peopleInGame;
        }

        public static bool StartMoving(uint citizenID, ref Citizen data, ushort sourceBuilding, ushort targetBuilding)
        {
            CitizenInfo _citizenInfo = data.GetCitizenInfo(citizenID);

            if (_citizenInfo != null)
            {
                CitizenManager _citizenManager = Singleton<CitizenManager>.instance;

                if (targetBuilding == sourceBuilding || targetBuilding == 0 || (Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].m_flags & Building.Flags.Active) == Building.Flags.None)
                    return false;

                if (data.m_instance != 0)
                {
                    _citizenInfo.m_citizenAI.SetTarget(data.m_instance, ref _citizenManager.m_instances.m_buffer[data.m_instance], targetBuilding);
                    data.CurrentLocation = Citizen.Location.Moving;
                    return true;
                }

                if (sourceBuilding == 0)
                {
                    sourceBuilding = data.GetBuildingByLocation();
                    if (sourceBuilding == 0)
                        return false;
                }

                ushort _citizenInstance;
                if (!_citizenManager.CreateCitizenInstance(out _citizenInstance, ref Singleton<SimulationManager>.instance.m_randomizer, _citizenInfo, citizenID))
                    return false;

                _citizenInfo.m_citizenAI.SetSource(_citizenInstance, ref _citizenManager.m_instances.m_buffer[(int)_citizenInstance], sourceBuilding);
                _citizenInfo.m_citizenAI.SetTarget(_citizenInstance, ref _citizenManager.m_instances.m_buffer[(int)_citizenInstance], targetBuilding);
                data.CurrentLocation = Citizen.Location.Moving;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
