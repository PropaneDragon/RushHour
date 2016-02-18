using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using RushHour.Redirection;
using UnityEngine;
using RushHour.Events;

namespace RushHour.BuildingHandlers
{
    [TargetType(typeof(CommercialBuildingAI))]
    public class NewCommercialBuildingAI
    {
        [RedirectMethod]
        public static void SimulationStepActive(CommercialBuildingAI thisAI, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            //This is a mess because I pulled it directly from the decompiled code and patched it up slightly.
            //It works though, and that's all I'm bothered about for now.

            if (thisAI)
            {
                DistrictManager instance1 = Singleton<DistrictManager>.instance;
                byte district = instance1.GetDistrict(buildingData.m_position);
                DistrictPolicies.Services policies = instance1.m_districts.m_buffer[(int)district].m_servicePolicies;
                DistrictPolicies.Taxation taxationPolicies = instance1.m_districts.m_buffer[(int)district].m_taxationPolicies;
                DistrictPolicies.CityPlanning cityPlanningPolicies = instance1.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
                instance1.m_districts.m_buffer[(int)district].m_servicePoliciesEffect |= policies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.RecreationalUse | DistrictPolicies.Services.ExtraInsulation | DistrictPolicies.Services.NoElectricity | DistrictPolicies.Services.OnlyElectricity);
                switch (thisAI.m_info.m_class.m_subService)
                {
                    case ItemClass.SubService.CommercialLow:
                        if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow)) != (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow))
                            instance1.m_districts.m_buffer[(int)district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow);
                        instance1.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & DistrictPolicies.CityPlanning.SmallBusiness;
                        break;
                    case ItemClass.SubService.CommercialHigh:
                        if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh)) != (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh))
                            instance1.m_districts.m_buffer[(int)district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh);
                        instance1.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & DistrictPolicies.CityPlanning.BigBusiness;
                        break;
                    case ItemClass.SubService.CommercialLeisure:
                        instance1.m_districts.m_buffer[(int)district].m_taxationPoliciesEffect |= taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure;
                        instance1.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises;
                        break;
                }
                Citizen.BehaviourData behaviour = new Citizen.BehaviourData();
                int aliveWorkerCount = 0;
                int totalWorkerCount = 0;
                int workPlaceCount = 0;
                int num1 = NewPrivateBuildingAI.HandleWorkers(thisAI, buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount, ref workPlaceCount);
                int width = buildingData.Width;
                int length = buildingData.Length;
                int num2 = MaxIncomingLoadSize(thisAI);
                int aliveCount = 0;
                int totalCount = 0;
                GetVisitBehaviour(thisAI, buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount);
                int visitCount = thisAI.CalculateVisitplaceCount(new Randomizer((int)buildingID), width, length);
                int num3 = Mathf.Max(0, visitCount - totalCount);
                int a1 = visitCount * 500;
                int num4 = Mathf.Max(a1, num2 * 4);
                TransferManager.TransferReason incomingTransferReason = GetIncomingTransferReason(thisAI);
                TransferManager.TransferReason outgoingTransferReason = GetOutgoingTransferReason(thisAI, buildingID);
                if (num1 != 0)
                {
                    int num5 = num4;
                    if (incomingTransferReason != TransferManager.TransferReason.None)
                        num5 = Mathf.Min(num5, (int)buildingData.m_customBuffer1);
                    if (outgoingTransferReason != TransferManager.TransferReason.None)
                        num5 = Mathf.Min(num5, num4 - (int)buildingData.m_customBuffer2);
                    int num6 = Mathf.Max(0, Mathf.Min(num1, (num5 * 200 + num4 - 1) / num4));
                    int a2 = (visitCount * num6 + 9) / 10;
                    if (Singleton<SimulationManager>.instance.m_isNightTime)
                        a2 = a2 + 1 >> 1;
                    int num7 = Mathf.Max(0, Mathf.Min(a2, num5));
                    if (incomingTransferReason != TransferManager.TransferReason.None)
                        buildingData.m_customBuffer1 -= (ushort)num7;
                    if (outgoingTransferReason != TransferManager.TransferReason.None)
                        buildingData.m_customBuffer2 += (ushort)num7;
                    num1 = (num7 + 9) / 10;
                }
                int electricityConsumption;
                int waterConsumption;
                int sewageAccumulation;
                int garbageAccumulation;
                int incomeAccumulation;
                thisAI.GetConsumptionRates(new Randomizer((int)buildingID), num1, out electricityConsumption, out waterConsumption, out sewageAccumulation, out garbageAccumulation, out incomeAccumulation);
                int heatingConsumption = 0;
                if (electricityConsumption != 0 && instance1.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation))
                {
                    if ((policies & DistrictPolicies.Services.ExtraInsulation) != DistrictPolicies.Services.None)
                    {
                        heatingConsumption = Mathf.Max(1, electricityConsumption * 3 + 8 >> 4);
                        incomeAccumulation = incomeAccumulation * 95 / 100;
                    }
                    else
                        heatingConsumption = Mathf.Max(1, electricityConsumption + 2 >> 2);
                }
                if (garbageAccumulation != 0 && (policies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
                {
                    garbageAccumulation = Mathf.Max(1, garbageAccumulation * 85 / 100);
                    incomeAccumulation = incomeAccumulation * 95 / 100;
                }
                int taxRate;
                switch (thisAI.m_info.m_class.m_subService)
                {
                    case ItemClass.SubService.CommercialLeisure:
                        taxRate = (buildingData.m_flags & Building.Flags.HighDensity) == Building.Flags.None ? Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow, thisAI.m_info.m_class.m_level, taxationPolicies) : Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialHigh, thisAI.m_info.m_class.m_level, taxationPolicies);
                        if ((taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure) != DistrictPolicies.Taxation.None)
                            taxRate = 0;
                        if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None && Singleton<SimulationManager>.instance.m_isNightTime)
                        {
                            electricityConsumption = electricityConsumption + 1 >> 1;
                            waterConsumption = waterConsumption + 1 >> 1;
                            sewageAccumulation = sewageAccumulation + 1 >> 1;
                            garbageAccumulation = garbageAccumulation + 1 >> 1;
                            incomeAccumulation = 0;
                            break;
                        }
                        break;
                    case ItemClass.SubService.CommercialTourist:
                        taxRate = (buildingData.m_flags & Building.Flags.HighDensity) == Building.Flags.None ? Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow, thisAI.m_info.m_class.m_level, taxationPolicies) : Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialHigh, thisAI.m_info.m_class.m_level, taxationPolicies);
                        break;
                    default:
                        taxRate = Singleton<EconomyManager>.instance.GetTaxRate(thisAI.m_info.m_class, taxationPolicies);
                        break;
                }
                if (num1 != 0)
                {
                    int num5 = HandleCommonConsumption(thisAI, buildingID, ref buildingData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, policies);
                    num1 = (num1 * num5 + 99) / 100;
                    if (num1 != 0)
                    {
                        int amount1 = incomeAccumulation;
                        if (amount1 != 0)
                        {
                            if (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialLow)
                            {
                                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SmallBusiness) != DistrictPolicies.CityPlanning.None)
                                {
                                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 12, thisAI.m_info.m_class);
                                    amount1 *= 2;
                                }
                            }
                            else if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.BigBusiness) != DistrictPolicies.CityPlanning.None)
                            {
                                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 25, thisAI.m_info.m_class);
                                amount1 *= 3;
                            }
                            if ((policies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
                                amount1 = (amount1 * 105 + 99) / 100;
                            int num6 = Singleton<EconomyManager>.instance.AddPrivateIncome(amount1, ItemClass.Service.Commercial, thisAI.m_info.m_class.m_subService, thisAI.m_info.m_class.m_level, taxRate);
                            int amount2 = (behaviour.m_touristCount * num6 + (aliveCount >> 1)) / Mathf.Max(1, aliveCount);
                            int amount3 = Mathf.Max(0, num6 - amount2);
                            if (amount3 != 0)
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.CitizenIncome, amount3, thisAI.m_info.m_class);
                            if (amount2 != 0)
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.TourismIncome, amount2, thisAI.m_info.m_class);
                        }
                        int groundPollution;
                        int noisePollution;
                        thisAI.GetPollutionRates(num1, cityPlanningPolicies, out groundPollution, out noisePollution);
                        if (groundPollution != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
                            Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, groundPollution, groundPollution, buildingData.m_position, 60f);
                        if (noisePollution != 0)
                            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, noisePollution, buildingData.m_position, 60f);
                        if (num5 < 100)
                            buildingData.m_flags |= Building.Flags.RateReduced;
                        else
                            buildingData.m_flags &= ~Building.Flags.RateReduced;
                        buildingData.m_flags |= Building.Flags.Active;
                    }
                    else
                        buildingData.m_flags &= ~(Building.Flags.RateReduced | Building.Flags.Active);
                }
                else
                {
                    electricityConsumption = 0;
                    heatingConsumption = 0;
                    waterConsumption = 0;
                    sewageAccumulation = 0;
                    garbageAccumulation = 0;
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Electricity | Notification.Problem.Water | Notification.Problem.Sewage | Notification.Problem.Flood | Notification.Problem.Heating);
                    buildingData.m_flags &= ~(Building.Flags.RateReduced | Building.Flags.Active);
                }
                int health = 0;
                int wellbeing = 0;
                float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                if (behaviour.m_healthAccumulation != 0)
                {
                    if (aliveWorkerCount + aliveCount != 0)
                        health = (behaviour.m_healthAccumulation + (aliveWorkerCount + aliveCount >> 1)) / (aliveWorkerCount + aliveCount);
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Health, behaviour.m_healthAccumulation, buildingData.m_position, radius);
                }
                if (behaviour.m_wellbeingAccumulation != 0)
                {
                    if (aliveWorkerCount + aliveCount != 0)
                        wellbeing = (behaviour.m_wellbeingAccumulation + (aliveWorkerCount + aliveCount >> 1)) / (aliveWorkerCount + aliveCount);
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Wellbeing, behaviour.m_wellbeingAccumulation, buildingData.m_position, radius);
                }
                int num8 = Citizen.GetHappiness(health, wellbeing) * 15 / 100;
                int a3 = aliveWorkerCount * 20 / workPlaceCount;
                if ((buildingData.m_problems & Notification.Problem.MajorProblem) == Notification.Problem.None)
                    num8 += 20;
                if (buildingData.m_problems == Notification.Problem.None)
                    num8 += 25;
                int num9 = num8 + Mathf.Min(a3, (int)buildingData.m_customBuffer1 * a3 / num4) + (a3 - Mathf.Min(a3, (int)buildingData.m_customBuffer2 * a3 / num4));
                int num10 = (int)(8 - thisAI.m_info.m_class.m_level);
                int num11 = (int)(11 - thisAI.m_info.m_class.m_level);
                if (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialHigh)
                {
                    ++num10;
                    ++num11;
                }
                if (taxRate < num10)
                    num9 += num10 - taxRate;
                if (taxRate > num11)
                    num9 -= taxRate - num11;
                if (taxRate >= num11 + 4)
                {
                    if ((int)buildingData.m_taxProblemTimer != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(32U) == 0)
                    {
                        int num5 = taxRate - num11 >> 2;
                        buildingData.m_taxProblemTimer = (byte)Mathf.Min((int)byte.MaxValue, (int)buildingData.m_taxProblemTimer + num5);
                        if ((int)buildingData.m_taxProblemTimer >= 96)
                            buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh | Notification.Problem.MajorProblem);
                        else if ((int)buildingData.m_taxProblemTimer >= 32)
                            buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh);
                    }
                }
                else
                {
                    buildingData.m_taxProblemTimer = (byte)Mathf.Max(0, (int)buildingData.m_taxProblemTimer - 1);
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh);
                }
                int entertainment;
                int attractiveness;
                GetAccumulation(thisAI, new Randomizer((int)buildingID), num1, taxRate, cityPlanningPolicies, taxationPolicies, out entertainment, out attractiveness);
                if (entertainment != 0)
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, entertainment, buildingData.m_position, radius);
                if (attractiveness != 0)
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, attractiveness);
                int happiness = Mathf.Clamp(num9, 0, 100);
                buildingData.m_health = (byte)health;
                buildingData.m_happiness = (byte)happiness;
                buildingData.m_citizenCount = (byte)(aliveWorkerCount + aliveCount);
                HandleDead(thisAI, buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
                int crimeAccumulation = behaviour.m_crimeAccumulation / 10;
                if (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialLeisure)
                    crimeAccumulation = crimeAccumulation * 5 + 3 >> 2;
                if ((policies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
                    crimeAccumulation = crimeAccumulation * 3 + 3 >> 2;
                HandleCrime(thisAI, buildingID, ref buildingData, crimeAccumulation, (int)buildingData.m_citizenCount);
                int num12 = (int)buildingData.m_crimeBuffer;
                if (aliveWorkerCount != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveWorkerCount, buildingData.m_position, radius);
                    int num5 = (behaviour.m_educated0Count * 100 + behaviour.m_educated1Count * 50 + behaviour.m_educated2Count * 30) / aliveWorkerCount + 50;
                    buildingData.m_fireHazard = (byte)num5;
                }
                else
                    buildingData.m_fireHazard = (byte)0;
                int crimeRate = (int)buildingData.m_citizenCount == 0 ? 0 : (num12 + ((int)buildingData.m_citizenCount >> 1)) / (int)buildingData.m_citizenCount;
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                if (incomingTransferReason != TransferManager.TransferReason.None)
                {
                    CalculateGuestVehicles(thisAI, buildingID, ref buildingData, incomingTransferReason, ref count, ref cargo, ref capacity, ref outside);
                    buildingData.m_tempImport = (byte)Mathf.Clamp(outside, (int)buildingData.m_tempImport, (int)byte.MaxValue);
                }
                buildingData.m_tempExport = (byte)Mathf.Clamp(behaviour.m_touristCount, (int)buildingData.m_tempExport, (int)byte.MaxValue);
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                if ((long)((_simulationManager.m_currentFrameIndex & 3840U) >> 8) == (long)((int)buildingID & 15) && (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialLow || thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialHigh) && ((int)Singleton<ZoneManager>.instance.m_lastBuildIndex == (int)_simulationManager.m_currentBuildIndex && (buildingData.m_flags & Building.Flags.Upgrading) == Building.Flags.None))
                    CheckBuildingLevel(thisAI, buildingID, ref buildingData, ref frameData, ref behaviour, aliveCount);
                if ((buildingData.m_flags & (Building.Flags.Completed | Building.Flags.Upgrading)) == Building.Flags.None)
                    return;
                Notification.Problem problems1 = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoCustomers | Notification.Problem.NoGoods);

                //Begin edited section

                if ((int)buildingData.m_customBuffer2 > num4 - (a1 >> 1) && aliveCount <= visitCount >> 1)
                {
                    if (_simulationManager.m_currentDayTimeHour > 19 && _simulationManager.m_currentDayTimeHour < 20)
                    {
                        buildingData.m_outgoingProblemTimer = (byte)Mathf.Min(byte.MaxValue, buildingData.m_outgoingProblemTimer + 1);

                        if (buildingData.m_outgoingProblemTimer >= 192)
                        {
                            problems1 = Notification.AddProblems(problems1, Notification.Problem.NoCustomers | Notification.Problem.MajorProblem);
                        }
                        else if (buildingData.m_outgoingProblemTimer >= 128)
                        {
                            problems1 = Notification.AddProblems(problems1, Notification.Problem.NoCustomers);
                        }
                    }
                    else
                    {
                        buildingData.m_outgoingProblemTimer = 0;
                    }
                }
                else
                    buildingData.m_outgoingProblemTimer = (byte)0;

                if (!CityEventManager.instance.EventStartsWithin(3D) && !CityEventManager.instance.EventTakingPlace() && !CityEventManager.instance.EventJustEnded())
                {
                    if ((int)buildingData.m_customBuffer1 == 0)
                    {
                        buildingData.m_incomingProblemTimer = (byte)Mathf.Min((int)byte.MaxValue, (int)buildingData.m_incomingProblemTimer + 1);
                        problems1 = (int)buildingData.m_incomingProblemTimer >= 64 ? Notification.AddProblems(problems1, Notification.Problem.NoGoods | Notification.Problem.MajorProblem) : Notification.AddProblems(problems1, Notification.Problem.NoGoods);
                    }
                    else
                        buildingData.m_incomingProblemTimer = (byte)0;

                    float currentHour = _simulationManager.m_currentDayTimeHour;

                    //Artifically shop at night to keep industry happy. Will give the effect of industry stocking up commercial over night.
                    //Note: ModifyMaterialBuffer is expensive, so if there's any performance impact with the mod now, it'll most likely be this.
                    if((currentHour > 8f || currentHour < 4f) && _simulationManager.m_randomizer.Int32(0, 10) > 8)
                    {
                        //Simulate 3 people buying things
                        int amount = -300;
                        thisAI.ModifyMaterialBuffer(buildingID, ref buildingData, TransferManager.TransferReason.Shopping, ref amount);
                    }
                }
                else
                {
                    buildingData.m_incomingProblemTimer = 0;
                }

                //End edited section

                buildingData.m_problems = problems1;
                instance1.m_districts.m_buffer[(int)district].AddCommercialData(ref behaviour, health, happiness, crimeRate, workPlaceCount, aliveWorkerCount, Mathf.Max(0, workPlaceCount - totalWorkerCount), visitCount, aliveCount, num3, (int)thisAI.m_info.m_class.m_level, electricityConsumption, heatingConsumption, waterConsumption, sewageAccumulation, garbageAccumulation, incomeAccumulation, Mathf.Min(100, (int)buildingData.m_garbageBuffer / 50), (int)buildingData.m_waterPollution * 100 / (int)byte.MaxValue, (int)buildingData.m_finalImport, (int)buildingData.m_finalExport, thisAI.m_info.m_class.m_subService);
                if ((int)buildingData.m_fireIntensity == 0 && incomingTransferReason != TransferManager.TransferReason.None)
                {
                    int num5 = num4 - (int)buildingData.m_customBuffer1 - capacity - (num2 >> 1);
                    if (num5 >= 0)
                        Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, new TransferManager.TransferOffer()
                        {
                            Priority = num5 * 8 / num2,
                            Building = buildingID,
                            Position = buildingData.m_position,
                            Amount = 1,
                            Active = false
                        });
                }
                if ((int)buildingData.m_fireIntensity == 0 && outgoingTransferReason != TransferManager.TransferReason.None)
                {
                    int num5 = (int)buildingData.m_customBuffer2 - aliveCount * 100;
                    if (num5 >= 100 && num3 > 0)
                        Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, new TransferManager.TransferOffer()
                        {
                            Priority = Mathf.Max(1, num5 * 8 / num4),
                            Building = buildingID,
                            Position = buildingData.m_position,
                            Amount = Mathf.Min(num5 / 100, num3),
                            Active = false
                        });
                }

                PrivateBuildingAI baseAI = thisAI as PrivateBuildingAI; //Because we don't have access to base here.

                if (baseAI != null)
                {
                    NewPrivateBuildingAI.SimulationStepActive(baseAI, buildingID, ref buildingData, ref frameData);
                }

                HandleFire(thisAI, buildingID, ref buildingData, ref frameData, policies);
            }
            else
            {
                Debug.LogError("Commercial building " + buildingID + " has no AI! This could have been bad.");
            }
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int MaxIncomingLoadSize(CommercialBuildingAI thisAI)
        {
            Debug.LogWarning("MaxIncomingLoadSize is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetVisitBehaviour(CommercialBuildingAI thisAI, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
        {
            Debug.LogWarning("GetVisitBehaviour is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetIncomingTransferReason(CommercialBuildingAI thisAI)
        {
            Debug.LogWarning("GetIncomingTransferReason is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetOutgoingTransferReason(CommercialBuildingAI thisAI, ushort buildingID)
        {
            Debug.LogWarning("GetOutgoingTransferReason is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int HandleCommonConsumption(CommercialBuildingAI thisAI, ushort buildingID, ref Building data, ref int electricityConsumption, ref int heatingConsumption, ref int waterConsumption, ref int sewageAccumulation, ref int garbageAccumulation, DistrictPolicies.Services policies)
        {
            Debug.LogWarning("HandleCommonConsumption is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetAccumulation(CommercialBuildingAI thisAI, Randomizer r, int productionRate, int taxRate, DistrictPolicies.CityPlanning cityPlanningPolicies, DistrictPolicies.Taxation taxationPolicies, out int entertainment, out int attractiveness)
        {
            entertainment = 0;
            attractiveness = 0;

            Debug.LogWarning("GetAccumulation is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleDead(CommercialBuildingAI thisAI, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            Debug.LogWarning("HandleDead is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleCrime(CommercialBuildingAI thisAI, ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            Debug.LogWarning("HandleCrime is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CalculateGuestVehicles(CommercialBuildingAI thisAI, ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            Debug.LogWarning("CalculateGuestVehicles is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CheckBuildingLevel(CommercialBuildingAI thisAI, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, ref Citizen.BehaviourData behaviour, int visitorCount)
        {
            Debug.LogWarning("CheckBuildingLevel is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleFire(CommercialBuildingAI thisAI, ushort buildingID, ref Building data, ref Building.Frame frameData, DistrictPolicies.Services policies)
        {
            Debug.LogWarning("HandleFire is not overridden!");
        }
    }
}
