﻿using EnhancedDevelopment.Excalibur.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EnhancedDevelopment.Excalibur.Transporter
{
    class Building_Transporter : Building
    {


        public override IEnumerable<Gizmo> GetGizmos()
        {
            //return base.GetGizmos();

            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.SendMaterials();
                //    act.icon = UI_ADD_RESOURCES;
                act.defaultLabel = "SendMaterials";
                act.defaultDesc = "SendMaterials";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }

        }

        private void SendMaterials()
        {
            List<Thing> _FoundThings = GenRadial.RadialDistinctThingsAround(this.Position, this.Map, 5, true).Where(x => x.def.category == ThingCategory.Item).Where(x => x.Spawned).Where(x => x.def.defName == "Steel").ToList();

            if (_FoundThings.Any())
            {
                //Log.Message("Found:" + foundThings.Count().ToString());

                //GameComponent_Transporter.TransportControler().StoreThing(_FoundThings);

                _FoundThings.ForEach(_x =>
                {

                    GameComponent_Excalibur.Instance.Quest.AddReserveMaterials(_x.stackCount);

                    //Log.Message("Removing: " + x.def.defName);
                    this.DisplayTransportEffect(_x);

                    _x.DeSpawn();

                        // Tell the MapDrawer that here is something thats changed
                        this.Map.mapDrawer.MapMeshDirty(_x.Position, MapMeshFlag.Things, true, false);
                });


            }

        }


        private void DisplayTransportEffect(Thing thingToTransport)
        {

            MoteMaker.MakeStaticMote(thingToTransport.Position, thingToTransport.Map, ThingDefOf.Mote_ExplosionFlash, 10);
            //int num2 = 10;
            //for (int i = 0; i < num2; i++)
            //{
            //    MoteMaker.ThrowDustPuff(_Thing.Position, _Thing.Map, Rand.Range(0.8f, 1.2f));
            //}

        }

    }
}
