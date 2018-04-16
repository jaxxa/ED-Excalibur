﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace EnhancedDevelopment.Excalibur.LaserDrill
{
    [StaticConstructorOnStartup]
    class Building_LaserFiller : Building
    {
        private static Texture2D UI_ACTIVATE_GATE;
        private int drillWork = Settings.Mod_EDExcalibur.Settings.LaserDrill.RequiredFillWork;
        private CompPowerTrader powerComp;
        bool active = false;

        static Building_LaserFiller()
        {
            UI_ACTIVATE_GATE = ContentFinder<Texture2D>.Get("UI/nuke", true);
        }

        public override void SpawnSetup(Map map, Boolean respawnAfterLoading)
        {
            base.SpawnSetup(map, respawnAfterLoading);
            this.powerComp = this.GetComp<CompPowerTrader>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.drillWork, "drillWork", 0, false);
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            //Scribe_Values.LookValue<Thing>(ref this.targetSteamGeyser, "targetSteamGeyser", null, false);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.ActivateDrill();
                act.icon = UI_ACTIVATE_GATE;
                act.defaultLabel = "Activate Drill";
                act.defaultDesc = "Activate Drill";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }

        public Thing FindClosestGuyser()
        {
            List<Thing> steamGeysers = this.Map.listerThings.ThingsOfDef(ThingDefOf.SteamGeyser);
            Thing currentLowestGuyser = null;

            double lowestDistance = double.MaxValue;

            foreach (Thing currentGuyser in steamGeysers)
            {
                //if (currentGuyser.SpawnedInWorld)
                if (currentGuyser.Spawned)
                {
                    if (this.Position.InHorDistOf(currentGuyser.Position, 5))
                    {
                        double distance = Math.Sqrt(Math.Pow((this.Position.x - currentGuyser.Position.x), 2) + Math.Pow((this.Position.y - currentGuyser.Position.y), 2));

                        if (distance < lowestDistance)
                        {

                            lowestDistance = distance;
                            currentLowestGuyser = currentGuyser;
                        }
                    }
                }
            }
            return currentLowestGuyser;
        }

        public void ActivateDrill()
        {
            if (this.FindClosestGuyser() != null)
            {
                this.active = true;
            }
            else
            {

            }
        }

        public override void TickRare()
        {
            if (this.active)
            {
                if (this.powerComp.PowerOn)
                {
                    //Log.Message("Reducing count");
                    this.drillWork = this.drillWork - 1;
                }
                else
                {
                    //Log.Message("No Power for drill.");
                }

                if (this.drillWork <= 0)
                {

                    if (this.FindClosestGuyser() != null)
                    {
                        this.FindClosestGuyser().DeSpawn();
                        this.Destroy(DestroyMode.Vanish);
                    }
                }
            }
            base.Tick();
        }

        public override string GetInspectString()
        {
            StringBuilder _StringBuilder = new StringBuilder();

            if (this.active)
            {
                if (powerComp != null)
                {
                    if (this.powerComp.PowerOn)
                    {
                        _StringBuilder.AppendLine("Fill Status: Online");

                    }
                    else
                    {
                        _StringBuilder.AppendLine("Fill Status: Low Power");
                    }
                }
                else
                {
                    _StringBuilder.AppendLine("Fill Status: Low Power");
                }
            }
            else
            {
                _StringBuilder.AppendLine("No Steam Geyser found");
            }

            _StringBuilder.Append("Fill Work Remaining: " + this.drillWork);

            if (powerComp != null)
            {
                string _Text = powerComp.CompInspectStringExtra();
                if (!_Text.NullOrEmpty())
                {
                    _StringBuilder.AppendLine("");
                    _StringBuilder.Append(_Text);
                }
            }

            return _StringBuilder.ToString();
        }
    }
}
