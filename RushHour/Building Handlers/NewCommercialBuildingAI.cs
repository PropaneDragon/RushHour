using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using RushHour.Redirection;
using UnityEngine;
using RushHour.Events;

namespace RushHour.BuildingHandlers
{
    [TargetType(typeof(CommercialBuildingAI))]
    internal class NewCommercialBuildingAI
    {
        [RedirectMethod]
        public static void SimulationStepActive(CommercialBuildingAI thisAI, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            //This is a mess because I pulled it directly from the decompiled code and patched it up slightly.
            //It works though, and that's all I'm bothered about for now.

            if (thisAI)
            {
                int num21;
                int num22;
                int num23;
                int num24;
                int num25;
                int taxRate;
                int num42;
                int num43;
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[(int)district].m_servicePolicies;
                DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[(int)district].m_taxationPolicies;
                DistrictPolicies.CityPlanning cityPlanningPolicies = instance.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
                instance.m_districts.m_buffer[(int)district].m_servicePoliciesEffect |= servicePolicies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.RecreationalUse | DistrictPolicies.Services.ExtraInsulation | DistrictPolicies.Services.NoElectricity | DistrictPolicies.Services.OnlyElectricity);
                switch (thisAI.m_info.m_class.m_subService)
                {
                    case ItemClass.SubService.CommercialLow:
                        if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow)) != (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow))
                            instance.m_districts.m_buffer[(int)district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComLow | DistrictPolicies.Taxation.TaxLowerComLow);
                        instance.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & (DistrictPolicies.CityPlanning.LightningRods | DistrictPolicies.CityPlanning.SmallBusiness); ;
                        break;
                    case ItemClass.SubService.CommercialHigh:
                        if ((taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh)) != (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh))
                            instance.m_districts.m_buffer[(int)district].m_taxationPoliciesEffect |= taxationPolicies & (DistrictPolicies.Taxation.TaxRaiseComHigh | DistrictPolicies.Taxation.TaxLowerComHigh);
                        instance.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & (DistrictPolicies.CityPlanning.LightningRods | DistrictPolicies.CityPlanning.BigBusiness);
                        break;
                    case ItemClass.SubService.CommercialLeisure:
                        instance.m_districts.m_buffer[(int)district].m_taxationPoliciesEffect |= taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure;
                        instance.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & (DistrictPolicies.CityPlanning.LightningRods | DistrictPolicies.CityPlanning.NoLoudNoises);
                        break;
                    case ItemClass.SubService.CommercialTourist:
                        instance.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & DistrictPolicies.CityPlanning.LightningRods;
                        break;
                    case ItemClass.SubService.CommercialEco:
                        instance.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= cityPlanningPolicies & DistrictPolicies.CityPlanning.LightningRods;
                        break;
                }
                Citizen.BehaviourData behaviour = new Citizen.BehaviourData();
                int aliveWorkerCount = 0;
                int totalWorkerCount = 0;
                int workPlaceCount = 0;
                int a = NewPrivateBuildingAI.HandleWorkers(thisAI, buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount, ref workPlaceCount);

                if ((buildingData.m_flags & Building.Flags.Evacuating) != Building.Flags.None)
                {
                    a = 0;
                }

                int width = buildingData.Width;
                int length = buildingData.Length;
                int num8 = MaxIncomingLoadSize(thisAI);
                int aliveCount = 0;
                int totalCount = 0;
                GetVisitBehaviour(thisAI, buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount);
                int visitCount = thisAI.CalculateVisitplaceCount(new Randomizer((int)buildingID), width, length);
                int emptyVisitCount = Mathf.Max(0, visitCount - totalCount);
                int num13 = visitCount * 500;
                int num14 = Mathf.Max(num13, num8 * 4);
                int num15 = thisAI.CalculateProductionCapacity(new Randomizer(buildingID), width, length);
                TransferManager.TransferReason incomingTransferReason = GetIncomingTransferReason(thisAI);
                TransferManager.TransferReason outgoingTransferReason = GetOutgoingTransferReason(thisAI, buildingID);
                if (((incomingTransferReason != TransferManager.TransferReason.None) && (a != 0)) && (num15 != 0))
                {
                    int b = num14 - buildingData.m_customBuffer1;
                    int num17 = Mathf.Max(0, Mathf.Min(a, (((b * 200) + num14) - 1) / num14));
                    int num18 = ((num15 * num17) + 9) / 10;
                    num18 = Mathf.Max(0, Mathf.Min(num18, b));
                    buildingData.m_customBuffer1 = (ushort)(buildingData.m_customBuffer1 + ((ushort)num18));
                }
                if (a != 0)
                {
                    int num19 = num14;
                    if (incomingTransferReason != TransferManager.TransferReason.None)
                        num19 = Mathf.Min(num19, (int)buildingData.m_customBuffer1);
                    if (outgoingTransferReason != TransferManager.TransferReason.None)
                        num19 = Mathf.Min(num19, num14 - (int)buildingData.m_customBuffer2);
                    a = Mathf.Max(0, Mathf.Min(a, (num19 * 200 + num14 - 1) / num14));
                    int num20 = (visitCount * a + 9) / 10;
                    if (Singleton<SimulationManager>.instance.m_isNightTime)
                    {
                        num20 = (num20 + 1) >> 1;
                    }
                    num20 = Mathf.Max(0, Mathf.Min(num20, num19));
                    if (incomingTransferReason != TransferManager.TransferReason.None)
                        buildingData.m_customBuffer1 -= (ushort)num20;
                    if (outgoingTransferReason != TransferManager.TransferReason.None)
                        buildingData.m_customBuffer2 += (ushort)num20;
                    a = (num20 + 9) / 10;
                }
                thisAI.GetConsumptionRates(new Randomizer((int)buildingID), a, out num21, out num22, out num23, out num24, out num25);
                int heatingConsumption = 0;
                if (num21 != 0 && instance.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation))
                {
                    if ((servicePolicies & DistrictPolicies.Services.ExtraInsulation) != DistrictPolicies.Services.None)
                    {
                        heatingConsumption = Mathf.Max(1, (num21 * 3 + 8) >> 4);
                        num25 = (num25 * 95) / 100;
                    }
                    else
                    {
                        heatingConsumption = Mathf.Max(1, (num21 + 2) >> 2);
                    }
                }
                if (num24 != 0 && (servicePolicies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None)
                {
                    num24 = Mathf.Max(1, num24 * 85 / 100);
                    num25 = (num25 * 95) / 100;
                }
                ItemClass.SubService subService = thisAI.m_info.m_class.m_subService;
                if (subService == ItemClass.SubService.CommercialLeisure)
                {
                    if ((buildingData.m_flags & Building.Flags.HighDensity) != Building.Flags.None)
                    {
                        taxRate = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialHigh, thisAI.m_info.m_class.m_level, taxationPolicies);
                    }
                    else
                    {
                        taxRate = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow, thisAI.m_info.m_class.m_level, taxationPolicies);
                    }
                    if ((taxationPolicies & DistrictPolicies.Taxation.DontTaxLeisure) != DistrictPolicies.Taxation.None)
                    {
                        taxRate = 0;
                    }
                    if (((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None) && Singleton<SimulationManager>.instance.m_isNightTime)
                    {
                        num21 = (num21 + 1) >> 1;
                        num22 = (num22 + 1) >> 1;
                        num23 = (num23 + 1) >> 1;
                        num24 = (num24 + 1) >> 1;
                        num25 = 0;
                    }
                }
                else if ((subService == ItemClass.SubService.CommercialTourist) || (subService == ItemClass.SubService.CommercialEco))
                {
                    if ((buildingData.m_flags & Building.Flags.HighDensity) != Building.Flags.None)
                    {
                        taxRate = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialHigh, thisAI.m_info.m_class.m_level, taxationPolicies);
                    }
                    else
                    {
                        taxRate = Singleton<EconomyManager>.instance.GetTaxRate(ItemClass.Service.Commercial, ItemClass.SubService.CommercialLow, thisAI.m_info.m_class.m_level, taxationPolicies);
                    }
                }
                else
                {
                    taxRate = Singleton<EconomyManager>.instance.GetTaxRate(thisAI.m_info.m_class, taxationPolicies);
                }
                if (a != 0)
                {
                    int num28 = HandleCommonConsumption(thisAI, buildingID, ref buildingData, ref frameData, ref num21, ref heatingConsumption, ref num22, ref num23, ref num24, servicePolicies);
                    a = (a * num28 + 99) / 100;
                    if (a != 0)
                    {
                        int num32;
                        int num33;
                        int amount = num25;
                        if (amount != 0)
                        {
                            if (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialLow)
                            {
                                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.SmallBusiness) != DistrictPolicies.CityPlanning.None)
                                {
                                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 12, thisAI.m_info.m_class);
                                    amount *= 2;
                                }
                            }
                            else if ((thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialHigh) && ((cityPlanningPolicies & DistrictPolicies.CityPlanning.BigBusiness) != DistrictPolicies.CityPlanning.None))
                            {
                                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 25, thisAI.m_info.m_class);
                                amount *= 3;
                            }
                            if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
                            {
                                amount = (amount * 105 + 99) / 100;
                            }
                            amount = Singleton<EconomyManager>.instance.AddPrivateIncome(amount, ItemClass.Service.Commercial, thisAI.m_info.m_class.m_subService, thisAI.m_info.m_class.m_level, taxRate);
                            int num30 = (behaviour.m_touristCount * amount + (aliveCount >> 1)) / Mathf.Max(1, aliveCount);
                            int num31 = Mathf.Max(0, amount - num30);
                            if (num31 != 0)
                            {
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.CitizenIncome, num31, thisAI.m_info.m_class);
                            }
                            if (num30 != 0)
                            {
                                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.TourismIncome, num30, thisAI.m_info.m_class);
                            }
                        }
                        thisAI.GetPollutionRates(a, cityPlanningPolicies, out num32, out num33);
                        if (num32 != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
                        {
                            Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num32, num32, buildingData.m_position, 60f);
                        }
                        if (num33 != 0)
                        {
                            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num33, buildingData.m_position, 60f);
                        }
                        if (num28 < 100)
                        {
                            buildingData.m_flags |= Building.Flags.RateReduced;
                        }
                        else
                        {
                            buildingData.m_flags &= ~Building.Flags.RateReduced;
                        }
                        buildingData.m_flags |= Building.Flags.Active;
                    }
                    else
                        buildingData.m_flags &= ~(Building.Flags.RateReduced | Building.Flags.Active);
                }
                else
                {
                    num21 = 0;
                    heatingConsumption = 0;
                    num22 = 0;
                    num23 = 0;
                    num24 = 0;
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Electricity | Notification.Problem.Water | Notification.Problem.Sewage | Notification.Problem.Flood | Notification.Problem.Heating);
                    buildingData.m_flags &= ~(Building.Flags.RateReduced | Building.Flags.Active);
                }
                int health = 0;
                int wellbeing = 0;
                float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                if (behaviour.m_healthAccumulation != 0)
                {
                    if (aliveWorkerCount + aliveCount != 0)
                    {
                        health = (behaviour.m_healthAccumulation + (aliveWorkerCount + aliveCount >> 1)) / (aliveWorkerCount + aliveCount);
                    }
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Health, behaviour.m_healthAccumulation, buildingData.m_position, radius);
                }
                if (behaviour.m_wellbeingAccumulation != 0)
                {
                    if (aliveWorkerCount + aliveCount != 0)
                    {
                        wellbeing = (behaviour.m_wellbeingAccumulation + (aliveWorkerCount + aliveCount >> 1)) / (aliveWorkerCount + aliveCount);
                    }
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Wellbeing, behaviour.m_wellbeingAccumulation, buildingData.m_position, radius);
                }
                int num37 = Citizen.GetHappiness(health, wellbeing) * 15 / 100;
                int num38 = aliveWorkerCount * 20 / workPlaceCount;
                if ((buildingData.m_problems & Notification.Problem.MajorProblem) == Notification.Problem.None)
                {
                    num37 += 20;
                }
                if (buildingData.m_problems == Notification.Problem.None)
                {
                    num37 += 25;
                }
                num37 += Mathf.Min(num38, (buildingData.m_customBuffer1 * num38) / num14);
                num37 += num38 - Mathf.Min(num38, (buildingData.m_customBuffer2 * num38) / num14);
                int num39 = (int)(8 - thisAI.m_info.m_class.m_level);
                int num40 = (int)(11 - thisAI.m_info.m_class.m_level);
                if (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialHigh)
                {
                    ++num39;
                    ++num40;
                }
                if (taxRate < num39)
                    num37 += num39 - taxRate;
                if (taxRate > num40)
                    num37 -= taxRate - num40;
                if (taxRate >= num40 + 4)
                {
                    if ((int)buildingData.m_taxProblemTimer != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(32U) == 0)
                    {
                        int num41 = taxRate - num40 >> 2;
                        buildingData.m_taxProblemTimer = (byte)Mathf.Min((int)byte.MaxValue, (int)buildingData.m_taxProblemTimer + num41);
                        if ((int)buildingData.m_taxProblemTimer >= 96)
                        {
                            buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.TaxesTooHigh | Notification.Problem.MajorProblem);
                        }
                        else if ((int)buildingData.m_taxProblemTimer >= 32)
                        {
                            buildingData.m_problems = Notification.AddProblems(buildingData.m_problems, Notification.Problem.None | Notification.Problem.TaxesTooHigh);
                        }
                    }
                }
                else
                {
                    buildingData.m_taxProblemTimer = (byte)Mathf.Max(0, (int)buildingData.m_taxProblemTimer - 1);
                    buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.None | Notification.Problem.TaxesTooHigh);
                }
                GetAccumulation(thisAI, new Randomizer((int)buildingID), a, taxRate, cityPlanningPolicies, taxationPolicies, out num42, out num43);
                if (num42 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, num42, buildingData.m_position, radius);
                }
                if (num43 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num43);
                }
                num37 = Mathf.Clamp(num37, 0, 100);
                buildingData.m_health = (byte)health;
                buildingData.m_happiness = (byte)num37;
                buildingData.m_citizenCount = (byte)(aliveWorkerCount + aliveCount);
                HandleDead(thisAI, buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
                int crimeAccumulation = behaviour.m_crimeAccumulation / 10;
                if (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialLeisure)
                {
                    crimeAccumulation = crimeAccumulation * 5 + 3 >> 2;
                }
                if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None)
                {
                    crimeAccumulation = crimeAccumulation * 3 + 3 >> 2;
                }
                HandleCrime(thisAI, buildingID, ref buildingData, crimeAccumulation, (int)buildingData.m_citizenCount);
                int crimeBuffer = (int)buildingData.m_crimeBuffer;
                if (aliveWorkerCount != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveWorkerCount, buildingData.m_position, radius);
                    int num46 = ((behaviour.m_educated0Count * 100) + (behaviour.m_educated1Count * 50)) + (behaviour.m_educated2Count * 30);
                    num46 = (num46 / aliveWorkerCount) + 50;
                    buildingData.m_fireHazard = (byte)num46;
                }
                else
                {
                    buildingData.m_fireHazard = (byte)0;
                }
                if (buildingData.m_citizenCount != 0)
                {
                    crimeBuffer = (crimeBuffer + (buildingData.m_citizenCount >> 1)) / buildingData.m_citizenCount;
                }
                else
                {
                    crimeBuffer = 0;
                }
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
                SimulationManager manager2 = Singleton<SimulationManager>.instance;
                uint num51 = (uint)((manager2.m_currentFrameIndex & 0xf00) >> 8);
                if (((num51 == (buildingID & 15)) && ((thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialLow) || (thisAI.m_info.m_class.m_subService == ItemClass.SubService.CommercialHigh))) && ((Singleton<ZoneManager>.instance.m_lastBuildIndex == manager2.m_currentBuildIndex) && ((buildingData.m_flags & Building.Flags.Upgrading) == Building.Flags.None)))
                {
                    CheckBuildingLevel(thisAI, buildingID, ref buildingData, ref frameData, ref behaviour, aliveCount);
                }

                //Begin edited section
                if ((buildingData.m_flags & (Building.Flags.Upgrading | Building.Flags.Completed)) != Building.Flags.None)
                {
                    Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoCustomers | Notification.Problem.NoGoods);
                    if ((int)buildingData.m_customBuffer2 > num14 - (num13 >> 1) && aliveCount <= visitCount >> 1)
                    {
                        if (manager2.m_currentDayTimeHour > 19 && manager2.m_currentDayTimeHour < 20)
                        {
                            buildingData.m_outgoingProblemTimer = (byte)Mathf.Min(byte.MaxValue, buildingData.m_outgoingProblemTimer + 1);

                            if (buildingData.m_outgoingProblemTimer >= 192)
                            {
                                problem = Notification.AddProblems(problem, Notification.Problem.NoCustomers | Notification.Problem.MajorProblem);
                            }
                            else if (buildingData.m_outgoingProblemTimer >= 128)
                            {
                                problem = Notification.AddProblems(problem, Notification.Problem.NoCustomers);
                            }
                        }
                        else
                        {
                            buildingData.m_outgoingProblemTimer = 0;
                        }
                    }
                    else
                    {
                        buildingData.m_outgoingProblemTimer = (byte)0;
                    }

                    if (!CityEventManager.instance.EventStartsWithin(3D) && !CityEventManager.instance.EventTakingPlace() && !CityEventManager.instance.EventJustEnded())
                    {
                        if ((buildingData.m_customBuffer1 == 0) && (incomingTransferReason != TransferManager.TransferReason.None))
                        {
                            buildingData.m_incomingProblemTimer = (byte)Mathf.Min((int)byte.MaxValue, (int)buildingData.m_incomingProblemTimer + 1);
                            if (buildingData.m_incomingProblemTimer < 0x40)
                            {
                                problem = Notification.AddProblems(problem, Notification.Problem.NoGoods);
                            }
                            else
                            {
                                problem = Notification.AddProblems(problem, Notification.Problem.MajorProblem | Notification.Problem.NoGoods);
                            }
                        }
                        else
                        {
                            buildingData.m_incomingProblemTimer = (byte)0;
                        }

                        //Artifically shop at night to keep industry happy. Will give the effect of industry stocking up commercial over night.
                        //Note: ModifyMaterialBuffer is expensive, so if there's any performance impact with the mod now, it'll most likely be this.
                        float currentHour = manager2.m_currentDayTimeHour;
                        if ((currentHour > 20f || currentHour < 4f))
                        {
                            if (manager2.m_randomizer.Int32(80) < 2)
                            {
                                //Simulate 2 people buying things
                                int amount = -200;
                                thisAI.ModifyMaterialBuffer(buildingID, ref buildingData, TransferManager.TransferReason.Shopping, ref amount);
                            }
                        }
                        else if (Experiments.ExperimentsToggle.AllowActiveCommercialFix && manager2.m_randomizer.Int32(40) < 5) //Added in as a potential fix to random inactive buildings. Lack of customers still shuts down commercial.
                        {
                            int amount = -50;
                            thisAI.ModifyMaterialBuffer(buildingID, ref buildingData, TransferManager.TransferReason.Shopping, ref amount);
                        }
                    }
                    else
                    {
                        buildingData.m_incomingProblemTimer = 0;
                    }

                    //End edited section

                    buildingData.m_problems = problem;
                    instance.m_districts.m_buffer[(int)district].AddCommercialData(ref behaviour, health, num37, crimeBuffer, workPlaceCount, aliveWorkerCount, Mathf.Max(0, workPlaceCount - totalWorkerCount), visitCount, aliveCount, emptyVisitCount, (int)thisAI.m_info.m_class.m_level, num21, heatingConsumption, num22, num23, num24, num25, Mathf.Min(100, (int)buildingData.m_garbageBuffer / 50), (int)buildingData.m_waterPollution * 100 / (int)byte.MaxValue, (int)buildingData.m_finalImport, (int)buildingData.m_finalExport, thisAI.m_info.m_class.m_subService);
                    if ((int)buildingData.m_fireIntensity == 0 && incomingTransferReason != TransferManager.TransferReason.None)
                    {
                        int num52 = num14 - (int)buildingData.m_customBuffer1 - capacity - (num8 >> 1);
                        if (num52 >= 0)
                            Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, new TransferManager.TransferOffer()
                            {
                                Priority = num52 * 8 / num8,
                                Building = buildingID,
                                Position = buildingData.m_position,
                                Amount = 1,
                                Active = false
                            });
                    }
                    if ((int)buildingData.m_fireIntensity == 0 && outgoingTransferReason != TransferManager.TransferReason.None)
                    {
                        int num53 = (int)buildingData.m_customBuffer2 - aliveCount * 100;
                        if (num53 >= 100 && emptyVisitCount > 0)
                            Singleton<TransferManager>.instance.AddOutgoingOffer(outgoingTransferReason, new TransferManager.TransferOffer()
                            {
                                Priority = Mathf.Max(1, num53 * 8 / num14),
                                Building = buildingID,
                                Position = buildingData.m_position,
                                Amount = Mathf.Min(num53 / 100, emptyVisitCount),
                                Active = false
                            });
                    }

                    PrivateBuildingAI baseAI = thisAI as PrivateBuildingAI; //Because we don't have access to base here.

                    if (baseAI != null)
                    {
                        NewPrivateBuildingAI.SimulationStepActive(baseAI, buildingID, ref buildingData, ref frameData);
                    }

                    HandleFire(thisAI, buildingID, ref buildingData, ref frameData, servicePolicies);
                }
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
        public static int HandleCommonConsumption(CommercialBuildingAI thisAI, ushort buildingID, ref Building data, ref Building.Frame frameData, ref int electricityConsumption, ref int heatingConsumption, ref int waterConsumption, ref int sewageAccumulation, ref int garbageAccumulation, DistrictPolicies.Services policies)
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
