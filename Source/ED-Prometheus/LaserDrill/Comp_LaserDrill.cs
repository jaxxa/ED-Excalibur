﻿using EnhancedDevelopment.Prometheus.Core;
using EnhancedDevelopment.Prometheus.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using static EnhancedDevelopment.Prometheus.Core.GameComponent_Prometheus_Quest;

namespace EnhancedDevelopment.Prometheus.LaserDrill
{
    [StaticConstructorOnStartup]
    class Comp_LaserDrill : ThingComp
    {

        public enum EnumLaserDrillState
        {
            Scanning,
            LowPower,
            ReadyToActivate
        }

        #region Variables

        //Saved
        private int DrillScanningRemaining;

        //Unsaved
        private CompProperties_LaserDrill Properties;

        private static Texture2D UI_LASER_ACTIVATE;

        private EnumLaserDrillState m_CurrentStaus = EnumLaserDrillState.Scanning;

        #endregion Variables

        #region Initilisation

        static Comp_LaserDrill()
        {

            UI_LASER_ACTIVATE = ContentFinder<Texture2D>.Get("UI/DirectOn", true);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            //this._PowerComp = this.parent.GetComp<CompPowerTrader>();
            //this._FlickComp = this.parent.GetComp<CompFlickable>();
            this.Properties = this.props as CompProperties_LaserDrill;

            if (!respawningAfterLoad)
            {
                this.SetRequiredDrillScanningToDefault();
            }

        }

        #endregion Initilisation

        #region Overrides

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.DrillScanningRemaining, "DrillScanningRemaining", 0);
        } //PostExposeData()

        public override void CompTickRare()
        {

            if (this.DrillScanningRemaining <= 0)
            {
                if (GameComponent_Prometheus.Instance.Comp_Quest.ResourceGetReserveStatus(EnumResourceType.Power) < Mod_EDPrometheus.Settings.LaserDrill.RequiredDrillShipPower)
                {
                    this.m_CurrentStaus = EnumLaserDrillState.LowPower;
                }
                else
                {
                    this.m_CurrentStaus = EnumLaserDrillState.ReadyToActivate;
                }

            }
            else
            {
                this.m_CurrentStaus = EnumLaserDrillState.Scanning;

                this.DrillScanningRemaining = this.DrillScanningRemaining - 1;
            }


            base.CompTickRare();

        } //CompTickRare()

        public override string CompInspectStringExtra()
        {
            // return base.CompInspectStringExtra();

            StringBuilder _StringBuilder = new StringBuilder();

            //if (this.parent.Map != null && this.parent.Map.GetComponent<MapComp_LaserDrill>() != null)

            {
                //if (!this.parent.Map.GetComponent<MapComp_LaserDrill>().IsActive(this.parent))
                //{
                //    _StringBuilder.Append("Drill Status: Offline, Waiting for another drill to finish.");
                //}
                //else

                if (this.m_CurrentStaus == EnumLaserDrillState.LowPower)
                {
                    _StringBuilder.AppendLine("Scan complete");
                    _StringBuilder.AppendLine("Low Power on Ship");
                }
                else if (this.m_CurrentStaus == EnumLaserDrillState.ReadyToActivate)
                {
                    _StringBuilder.AppendLine("Scan complete");
                    _StringBuilder.AppendLine("Ready for Laser Activation");
                }
                else if (this.m_CurrentStaus == EnumLaserDrillState.Scanning)
                {
                    _StringBuilder.AppendLine("Scanning in Progress - Remaining: " + this.DrillScanningRemaining);
                }

                _StringBuilder.Append("Ship Power: " + (GameComponent_Prometheus.Instance.Comp_Quest.ResourceGetReserveStatus(EnumResourceType.Power).ToString() + " / " + Mod_EDPrometheus.Settings.LaserDrill.RequiredDrillShipPower).ToString());

            }

            return _StringBuilder.ToString();
        } //CompInspectStringExtra()

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            //return base.CompGetGizmosExtra();

            //Add the stock Gizmoes
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (this.m_CurrentStaus == EnumLaserDrillState.ReadyToActivate)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.TriggerLaser();
                act.icon = UI_LASER_ACTIVATE;
                act.defaultLabel = "Activate Laser";
                act.defaultDesc = "Activate Laser";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }

            if (this.m_CurrentStaus == EnumLaserDrillState.ReadyToActivate)
            {
                Command_Action act = new Command_Action();
                act.action = () => this.TriggerLaserToFill();
                act.icon = UI_LASER_ACTIVATE;
                act.defaultLabel = "Activate Laser Fill";
                act.defaultDesc = "Activate Laser Fill";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }

        } //CompGetGizmosExtra()

        public override void PostDeSpawn(Map map)
        {
            this.SetRequiredDrillScanningToDefault();
            base.PostDeSpawn(map);
        }

        #endregion Overrides

        #region Methods

        private void SetRequiredDrillScanningToDefault()
        {
            this.DrillScanningRemaining = Mod_EDPrometheus.Settings.LaserDrill.RequiredDrillScanning;
        }

        public Thing FindClosestGeyser()
        {
            List<Thing> steamGeyser = this.parent.Map.listerThings.ThingsOfDef(ThingDefOf.SteamGeyser);
            Thing currentLowestGeyser = null;

            double lowestDistance = double.MaxValue;

            foreach (Thing currentGuyser in steamGeyser)
            {
                //if (currentGuyser.SpawnedInWorld)
                if (currentGuyser.Spawned)
                {
                    if (this.parent.Position.InHorDistOf(currentGuyser.Position, 5))
                    {
                        double distance = Math.Sqrt(Math.Pow((this.parent.Position.x - currentGuyser.Position.x), 2) + Math.Pow((this.parent.Position.y - currentGuyser.Position.y), 2));

                        if (distance < lowestDistance)
                        {

                            lowestDistance = distance;
                            currentLowestGeyser = currentGuyser;
                        }
                    }
                }
            }
            return currentLowestGeyser;
        }

        public void TriggerLaserToFill()
        {
            if (this.FindClosestGeyser() != null)
            {
                Messages.Message("SteamGeyser Removed.", MessageTypeDefOf.TaskCompletion);
                this.FindClosestGeyser().DeSpawn();
                GameComponent_Prometheus.Instance.Comp_Quest.ResourceRequestReserve(Core.GameComponent_Prometheus_Quest.EnumResourceType.Power, Mod_EDPrometheus.Settings.LaserDrill.RequiredDrillShipPower);
                this.ShowLaserVisually();

                this.parent.Destroy(DestroyMode.Vanish);
            }
            else
            {
                Messages.Message("SteamGeyser not found to Remove.", MessageTypeDefOf.NegativeEvent);
            }
        }

        public void TriggerLaser()
        {
            Messages.Message("SteamGeyser Created.", MessageTypeDefOf.TaskCompletion);
            GameComponent_Prometheus.Instance.Comp_Quest.ResourceRequestReserve(Core.GameComponent_Prometheus_Quest.EnumResourceType.Power, Mod_EDPrometheus.Settings.LaserDrill.RequiredDrillShipPower);
            this.ShowLaserVisually();
            GenSpawn.Spawn(ThingDef.Named("SteamGeyser"), this.parent.Position, this.parent.Map);

            //Destroy

            this.parent.Destroy(DestroyMode.Vanish);
        }

        private void ShowLaserVisually()
        {
            IntVec3 _Position = IntVec3.FromVector3(new UnityEngine.Vector3(parent.Position.x, parent.Position.y, parent.Position.z - 2));
            LaserDrillVisual _LaserDrillVisual = (LaserDrillVisual)GenSpawn.Spawn(ThingDef.Named("LaserDrillVisual"), _Position, parent.Map, WipeMode.Vanish);
        }

        #endregion


    } //Comp_LaserDrill

}

