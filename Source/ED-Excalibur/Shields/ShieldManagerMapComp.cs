﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EnhancedDevelopment.Excalibur.Shields
{
    [StaticConstructorOnStartup]
    class ShieldManagerMapComp : MapComponent
    {

        public static readonly SoundDef HitSoundDef = SoundDef.Named("Shields_HitShield");

        private List<Projectile> m_Projectiles = new List<Projectile>();

        public ShieldManagerMapComp(Map map) : base(map)
        {
            this.map = map;
            //   map.components.<ShieldManagerMapComp>();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            // Log.Message("MapCompTick");
        }

        public bool WillDropPodBeIntercepted(DropPodIncoming dropPodToTest, bool hostile)
        {
            IEnumerable<Building_Shield> _ShieldBuildings = map.listerBuildings.AllBuildingsColonistOfClass<Building_Shield>();
            if (_ShieldBuildings.Any(x => x.WillInterceptDropPod(dropPodToTest) ))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool WillProjectileBeBlocked(Verse.Projectile projectile)
        {

            IEnumerable<Building_Shield> _ShieldBuildings = map.listerBuildings.AllBuildingsColonistOfClass<Building_Shield>();
            
            if (_ShieldBuildings.Any(x =>  x.WillProjectileBeBlocked(projectile)))
            {
                //Log.Message("Blocked");

                //On hit effects
                MoteMaker.ThrowLightningGlow(projectile.ExactPosition, this.map, 0.5f);
                //On hit sound
                HitSoundDef.PlayOneShot((SoundInfo)new TargetInfo(projectile.Position, projectile.Map, false));

                projectile.Destroy();
                return true;
            }

            return false;
        }
                   

    }
}
