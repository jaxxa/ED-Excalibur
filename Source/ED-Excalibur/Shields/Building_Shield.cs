﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

using RimWorld;
using Verse;
using Verse.Sound;

//using EnhancedDevelopment.Shields.ShieldUtils;

namespace EnhancedDevelopment.Excalibur.Shields
{
    [StaticConstructorOnStartup]
    public class Building_Shield : Building
    {

        Material currentMatrialColour;

        //Ratio of lost power per damage
        private const float powerToDamage = 1f;

        //Hit sound
        public static readonly SoundDef HitSoundDef = SoundDef.Named("Shields_HitShield");

        #region Constants

        #endregion

        #region Variables

        //UI elements
        private static Texture2D UI_DIRECT_ON;
        private static Texture2D UI_DIRECT_OFF;

        private static Texture2D UI_INDIRECT_ON;
        private static Texture2D UI_INDIRECT_OFF;

        private static Texture2D UI_INTERCEPT_DROPPOD_ON;
        private static Texture2D UI_INTERCEPT_DROPPOD_OFF;

        private static Texture2D UI_SHOW_ON;
        private static Texture2D UI_SHOW_OFF;

        //Default all flasgs to be shown initially
        protected bool m_BlockIndirect_Active = true;
        protected bool m_BlockDirect_Active = true;
        protected bool m_ShowVisually_Active = true;
        protected bool m_InterceptDropPod_Active = true;

        //Variables to store what modes the shield have avalable.
        protected bool m_BlockIndirect_Avalable;
        protected bool m_BlockDirect_Avalable;
        protected bool m_InterceptDropPod_Avalable;

        //variables that are read in from XML
        private int m_FieldIntegrity_Max;
        private int m_FieldIntegrity_Initial;
        public int m_Field_Radius;

        private int m_PowerRequired_Charging;
        private int m_PowerRequired_Sustaining;

        private int m_RechargeTickDelayInterval;
        private int m_RecoverWarmupDelayTicks;

        private float m_ColourRed;
        private float m_ColourGreen;
        private float m_ColourBlue;

        private int m_WarmupTicksRemaining = 0;
        private long m_TickCount = 0;

        CompPowerTrader m_Power;

        #endregion

        #region Properties

        public EnumShieldStatus CurrentStatus
        {
            get
            {
                return this.m_CurrentStatus;
            }
            set
            {
                this.m_CurrentStatus = value;

                if (this.m_CurrentStatus == EnumShieldStatus.ActiveSustaining)
                {
                    this.m_Power.powerOutputInt = this.m_PowerRequired_Sustaining;
                }
                else
                {
                    this.m_Power.powerOutputInt = this.m_PowerRequired_Charging;
                }
            }
        }
        private EnumShieldStatus m_CurrentStatus = EnumShieldStatus.Offline;

        private int FieldIntegrity_Current
        {
            get
            {
                return this.m_FieldIntegrity_Current;
            }
            set
            {
                if (value < 0)
                {
                    this.CurrentStatus = EnumShieldStatus.Offline;
                    this.m_FieldIntegrity_Current = 0;
                }
                else if (value > this.m_FieldIntegrity_Max)
                {
                    this.m_FieldIntegrity_Current = this.m_FieldIntegrity_Max;
                }
                else
                {
                    this.m_FieldIntegrity_Current = value;
                }
            }
        }
        private int m_FieldIntegrity_Current;

        #endregion

        #region Initilisation

        //Constructor

        static Building_Shield()
        {
            //Setup UI
            UI_DIRECT_OFF = ContentFinder<Texture2D>.Get("UI/DirectOff", true);
            UI_DIRECT_ON = ContentFinder<Texture2D>.Get("UI/DirectOn", true);
            UI_INDIRECT_OFF = ContentFinder<Texture2D>.Get("UI/IndirectOff", true);
            UI_INDIRECT_ON = ContentFinder<Texture2D>.Get("UI/IndirectOn", true);
            UI_INTERCEPT_DROPPOD_OFF = ContentFinder<Texture2D>.Get("UI/FireOff", true);
            UI_INTERCEPT_DROPPOD_ON = ContentFinder<Texture2D>.Get("UI/FireOn", true);

            UI_SHOW_ON = ContentFinder<Texture2D>.Get("UI/ShieldShowOn", true);
            UI_SHOW_OFF = ContentFinder<Texture2D>.Get("UI/ShieldShowOff", true);
        }

        //Dummy override
        //This is called before Spawn Setup.
        public override void PostMake()
        {
            base.PostMake();
            //Log.Message("PM");
        }

        //On spawn, get the power component reference
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            //Log.Message("SS");

            base.SpawnSetup(map, respawningAfterLoad);
            this.m_Power = base.GetComp<CompPowerTrader>();

            Comp_ShieldBuilding _CompShield = this.GetComp<Comp_ShieldBuilding>();
            //_CompShield.props
            if (_CompShield.props is CompProperties_ShieldBuilding)
            {
                //Read in variables from the custom MyThingDef
                m_FieldIntegrity_Max = ((CompProperties_ShieldBuilding)_CompShield.props).m_FieldIntegrity_Max;
                m_FieldIntegrity_Initial = ((CompProperties_ShieldBuilding)_CompShield.props).m_FieldIntegrity_Initial;
                m_Field_Radius = ((CompProperties_ShieldBuilding)_CompShield.props).m_Field_Radius;

                m_PowerRequired_Charging = ((CompProperties_ShieldBuilding)_CompShield.props).m_PowerRequiredCharging;
                m_PowerRequired_Sustaining = ((CompProperties_ShieldBuilding)_CompShield.props).m_PowerRequiredSustaining;

                m_RechargeTickDelayInterval = ((CompProperties_ShieldBuilding)_CompShield.props).m_RechargeTickDelayInterval;
                m_RecoverWarmupDelayTicks = ((CompProperties_ShieldBuilding)_CompShield.props).m_RecoverWarmupDelayTicks;

                m_BlockIndirect_Avalable = ((CompProperties_ShieldBuilding)_CompShield.props).m_BlockIndirect_Avalable;
                m_BlockDirect_Avalable = ((CompProperties_ShieldBuilding)_CompShield.props).m_BlockDirect_Avalable;
                m_InterceptDropPod_Avalable = ((CompProperties_ShieldBuilding)_CompShield.props).m_InterceptDropPod_Avalable;


                m_ColourRed = ((CompProperties_ShieldBuilding)_CompShield.props).m_ColourRed;
                m_ColourGreen = ((CompProperties_ShieldBuilding)_CompShield.props).m_ColourGreen;
                m_ColourBlue = ((CompProperties_ShieldBuilding)_CompShield.props).m_ColourBlue;

            }
            else
            {
                Log.Error("Shield definition not of type \"ShieldBuildingThingDef\"");
            }
        }
        #endregion

        #region Methods

        public override void Tick()
        {
            base.Tick();
            this.m_TickCount += 1;

            this.UpdateShieldStatus();

            this.TickRecharge();
        }

        public bool IsActive()
        {
            //return true;
            return (this.CurrentStatus == EnumShieldStatus.ActiveCharging ||
                 this.CurrentStatus == EnumShieldStatus.ActiveDischarging ||
                 this.CurrentStatus == EnumShieldStatus.ActiveSustaining);
        }

        public bool CheckPowerOn()
        {
            if (this.m_Power != null)
            {
                if (this.m_Power.PowerOn)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateShieldStatus()
        {
            Boolean _PowerAvalable = this.CheckPowerOn();

            switch (this.CurrentStatus)
            {

                case (EnumShieldStatus.Offline):

                    //If it is offline bit has Power that initialising
                    if (_PowerAvalable)
                    {
                        this.CurrentStatus = EnumShieldStatus.Initilising;
                        //this.m_warmupTicksCurrent = GenDate.SecondsToTicks(this.m_shieldRecoverWarmup);
                        this.m_WarmupTicksRemaining = this.m_RecoverWarmupDelayTicks;
                        //this.CellsToProtect = null;
                    }
                    break;


                case (EnumShieldStatus.Initilising):
                    if (_PowerAvalable)
                    {
                        if (this.m_WarmupTicksRemaining > 0)
                        {
                            this.m_WarmupTicksRemaining--;
                            //  Log.Message(this.m_warmupTicksCurrent.ToString());
                        }
                        else
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                            this.FieldIntegrity_Current = this.m_FieldIntegrity_Initial;
                        }
                    }
                    else
                    {
                        this.CurrentStatus = EnumShieldStatus.Offline;
                    }
                    break;


                case (EnumShieldStatus.ActiveDischarging):
                    if (_PowerAvalable)
                    {
                        this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                    }
                    else if (this.FieldIntegrity_Current <= 0)
                    {
                        this.CurrentStatus = EnumShieldStatus.Offline;
                    }
                    break;
                case (EnumShieldStatus.ActiveCharging):
                    if (this.FieldIntegrity_Current < 0)
                    {
                        this.CurrentStatus = EnumShieldStatus.Offline;
                    }
                    else
                    {
                        if (!_PowerAvalable)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveDischarging;
                        }
                        else if (this.FieldIntegrity_Current >= this.m_FieldIntegrity_Max)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveSustaining;
                        }
                    }
                    break;

                case (EnumShieldStatus.ActiveSustaining):
                    if (!_PowerAvalable)
                    {
                        this.CurrentStatus = EnumShieldStatus.ActiveDischarging;
                    }
                    else
                    {
                        if (this.FieldIntegrity_Current < this.m_FieldIntegrity_Max)
                        {
                            this.CurrentStatus = EnumShieldStatus.ActiveCharging;
                        }
                    }
                    break;
            }
        }

        public void TickRecharge()
        {
            if (Find.TickManager.TicksGame % this.m_RechargeTickDelayInterval == 0)
            {
                if (this.CurrentStatus == EnumShieldStatus.ActiveCharging)
                {
                    this.FieldIntegrity_Current++;
                }
                else if (this.CurrentStatus == EnumShieldStatus.ActiveDischarging)
                {
                    this.FieldIntegrity_Current--;
                }
            }
        }

        /// <summary>
        /// Finds all projectiles at the position and destroys them
        /// </summary>
        /// <param name="square">The current square to protect</param>
        /// <param name="flag_direct">Block direct Fire</param>
        /// <param name="flag_indirect">Block indirect Fire</param>
        virtual public void ProtectSquare(IntVec3 square)
        {
            //Ignore squares outside the map
            if (!square.InBounds(this.Map))
            {
                return;
            }
            List<Thing> things = this.Map.thingGrid.ThingsListAt(square);
            List<Thing> thingsToDestroy = new List<Thing>();

            for (int i = 0, l = things.Count(); i < l; i++)
            {

                if (things[i] != null && things[i] is Projectile)
                {
                    //Assign to variable
                    Projectile pr = (Projectile)things[i];
                    if (!pr.Destroyed && ((this.m_BlockIndirect_Avalable && this.m_BlockDirect_Active && pr.def.projectile.flyOverhead) || (this.m_BlockDirect_Avalable && this.m_BlockIndirect_Active && !pr.def.projectile.flyOverhead)))
                    {
                        bool wantToIntercept = true;

                        //Check OverShoot
                        if (pr.def.projectile.flyOverhead)
                        {
                            if (this.WillTargetLandInRange(pr))
                            {
                                //Log.Message("Fly Over");
                            }
                            else
                            {
                                wantToIntercept = false;
                                //Log.Message("In Range");
                            }
                        }


                        if (wantToIntercept)
                        {

                            //Detect proper collision using angles
                            Quaternion targetAngle = pr.ExactRotation;

                            Vector3 projectilePosition2D = pr.ExactPosition;
                            projectilePosition2D.y = 0;

                            Vector3 shieldPosition2D = EnhancedDevelopment.Excalibur.Shields.Utilities.VectorsUtils.IntVecToVec(this.Position);

                            shieldPosition2D.y = 0;

                            Quaternion shieldProjAng = Quaternion.LookRotation(projectilePosition2D - shieldPosition2D);


                            if ((Quaternion.Angle(targetAngle, shieldProjAng) > 90))
                            {

                                //On hit effects
                                MoteMaker.ThrowLightningGlow(pr.ExactPosition, this.Map, 0.5f);
                                //On hit sound
                                HitSoundDef.PlayOneShot((SoundInfo)new TargetInfo(this.Position, this.Map, false));

                                //Damage the shield
                                ProcessDamage(pr.def.projectile.DamageAmount);
                                //add projectile to the list of things to be destroyed
                                thingsToDestroy.Add(pr);
                            }
                        }
                        else
                        {
                            //Log.Message("Skip");
                        }

                    }
                }
            }
            foreach (Thing currentThing in thingsToDestroy)
            {
                currentThing.Destroy();
            }

        }

        /// <summary>
        /// Damage the Shield
        /// </summary>
        /// <param name="damage">The amount of damage to give</param>
        public void ProcessDamage(int damage)
        {
            //Prevent negative health
            if (this.FieldIntegrity_Current <= 0)
                return;
            this.FieldIntegrity_Current -= (int)(((float)damage) * powerToDamage);
        }

        /// <summary>
        /// Checks if the projectile will land within the shield or pass over.
        /// </summary>
        /// <param name="projectile">The specific projectile that is being checked</param>
        /// <returns>True if the projectile will land close, false if it will be far away.</returns>
        public bool WillTargetLandInRange(Projectile projectile)
        {
            Vector3 targetLocation = GetTargetLocationFromProjectile(projectile);

            if (Vector3.Distance(this.Position.ToVector3(), targetLocation) > this.m_Field_Radius)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Vector3 GetTargetLocationFromProjectile(Projectile projectile)
        {
            FieldInfo fieldInfo = projectile.GetType().GetField("destination", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3 reoveredVector = (Vector3)fieldInfo.GetValue(projectile);
            return reoveredVector;
        }

        private void interceptPods()
        {
            if (this.m_InterceptDropPod_Avalable && this.m_InterceptDropPod_Active)
            {
                IEnumerable<Thing> dropPods = this.Map.listerThings.ThingsOfDef(ThingDefOf.ActiveDropPod);

                if (dropPods != null)
                {
                    //Test to protect the entire map from droppods.
                    //IEnumerable<Thing> closeFires = dropPods.Where<Thing>(t => t.Position.InHorDistOf(this.position, 9999999.0f));

                    IEnumerable<Thing> closeFires = dropPods.Where<Thing>(t => t.Position.InHorDistOf(this.Position, this.m_Field_Radius));

                    foreach (RimWorld.ActiveDropPod currentDropPod in closeFires.ToList())
                    {
                        //currentDropPod.Destroy();

                        GenExplosion.DoExplosion(currentDropPod.Position, this.Map, 1, DamageDefOf.Bomb, currentDropPod);

                        currentDropPod.Destroy(DestroyMode.Vanish);


                        // BodyPartDamageInfo bodyPartDamageInfo = new BodyPartDamageInfo(new BodyPartHeight?(), new BodyPartDepth?(BodyPartDepth.Outside));
                        //new ExplosionInfo()
                        //{
                        //    center = currentDropPod.Position,
                        //    radius = 1,
                        //    dinfo = new DamageInfo(DamageDefOf.Flame, 10, currentDropPod)

                        //}.DoExplosion();

                    }
                }
            }
        }

        #endregion

        #region Drawing

        public override void Draw()
        {
            base.Draw();
            this.DrawShields();
        }

        /// <summary>
        /// Draw the shield Field
        /// </summary>
        public void DrawShields()
        {
            if (!this.IsActive() || !this.m_ShowVisually_Active)
            {
                return;
            }

            //Draw field
            this.DrawField(EnhancedDevelopment.Excalibur.Shields.Utilities.VectorsUtils.IntVecToVec(base.Position));

        }

        public override void DrawExtraSelectionOverlays()
        {
            //    GenDraw.DrawRadiusRing(base.Position, shieldField.shieldShieldRadius);
        }

        public void DrawSubField(IntVec3 center, float radius)
        {
            this.DrawSubField(EnhancedDevelopment.Excalibur.Shields.Utilities.VectorsUtils.IntVecToVec(center), radius);
        }

        //Draw the field on map
        public void DrawField(Vector3 center)
        {
            DrawSubField(center, m_Field_Radius);
        }

        public void DrawSubField(Vector3 position, float shieldShieldRadius)
        {
            position = position + (new Vector3(0.5f, 0f, 0.5f));

            Vector3 s = new Vector3(shieldShieldRadius, 1f, shieldShieldRadius);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(position, Quaternion.identity, s);

            if (currentMatrialColour == null)
            {
                //Log.Message("Creating currentMatrialColour");
                currentMatrialColour = SolidColorMaterials.NewSolidColorMaterial(new Color(m_ColourRed, m_ColourGreen, m_ColourBlue, 0.15f), ShaderDatabase.MetaOverlay);
            }

            UnityEngine.Graphics.DrawMesh(EnhancedDevelopment.Excalibur.Shields.Utilities.Graphics.CircleMesh, matrix, currentMatrialColour, 0);

        }

        #endregion

        #region UI

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (this.IsActive())
            {
                stringBuilder.AppendLine("Shield: " + this.FieldIntegrity_Current + "/" + this.m_FieldIntegrity_Max);
            }
            else if (this.CurrentStatus == EnumShieldStatus.Initilising)
            {
                //stringBuilder.AppendLine("Initiating shield: " + ((warmupTicks * 100) / recoverWarmup) + "%");
                stringBuilder.AppendLine("Ready in " + Math.Round(GenTicks.TicksToSeconds(m_WarmupTicksRemaining)) + " seconds.");
                //stringBuilder.AppendLine("Ready in " + m_warmupTicksCurrent + " seconds.");
            }
            else
            {
                stringBuilder.AppendLine("Shield disabled!");
            }

            if (m_Power != null)
            {
                string text = m_Power.CompInspectStringExtra();
                if (!text.NullOrEmpty())
                {
                    stringBuilder.Append(text);
                }
                else
                {
                    stringBuilder.Append("Error, No Power Comp Text.");
                }
            }
            else
            {
                stringBuilder.Append("Error, No Power Comp.");
            }

            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Add the stock Gizmoes
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (m_BlockDirect_Avalable)
            {
                if (m_BlockIndirect_Active)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchDirect();
                    act.icon = UI_DIRECT_ON;
                    act.defaultLabel = "Block Direct";
                    act.defaultDesc = "On";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchDirect();
                    act.icon = UI_DIRECT_OFF;
                    act.defaultLabel = "Block Direct";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (m_BlockIndirect_Avalable)
            {
                if (m_BlockDirect_Active)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchIndirect();
                    act.icon = UI_INDIRECT_ON;
                    act.defaultLabel = "Block Indirect";
                    act.defaultDesc = "On";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchIndirect();
                    act.icon = UI_INDIRECT_OFF;
                    act.defaultLabel = "Block Indirect";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (m_InterceptDropPod_Avalable)
            {
                if (m_InterceptDropPod_Active)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchInterceptDropPod();
                    act.icon = UI_INTERCEPT_DROPPOD_ON;
                    act.defaultLabel = "Intercept DropPod";
                    act.defaultDesc = "On";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchInterceptDropPod();
                    act.icon = UI_INTERCEPT_DROPPOD_OFF;
                    act.defaultLabel = "Intercept DropPod";
                    act.defaultDesc = "Off";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }


            if (true)
            {
                if (m_ShowVisually_Active)
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchVisual();
                    act.icon = UI_SHOW_ON;
                    act.defaultLabel = "Show Visually";
                    act.defaultDesc = "Show";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
                else
                {

                    Command_Action act = new Command_Action();
                    //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                    act.action = () => this.SwitchVisual();
                    act.icon = UI_SHOW_OFF;
                    act.defaultLabel = "Show Visually";
                    act.defaultDesc = "Hide";
                    act.activateSound = SoundDef.Named("Click");
                    //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                    //act.groupKey = 689736;
                    yield return act;
                }
            }

            if (true)
            {
                Command_Action act = new Command_Action();
                //act.action = () => Designator_Deconstruct.DesignateDeconstruct(this);
                act.action = () => this.FindUpgrades();
                act.icon = UI_SHOW_ON;
                act.defaultLabel = "Find Upgrades";
                act.defaultDesc = "Find Upgrades";
                act.activateSound = SoundDef.Named("Click");
                //act.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
                //act.groupKey = 689736;
                yield return act;
            }
        }

        private void SwitchDirect()
        {
            m_BlockIndirect_Active = !m_BlockIndirect_Active;
        }

        private void SwitchIndirect()
        {
            m_BlockDirect_Active = !m_BlockDirect_Active;
        }

        private void SwitchInterceptDropPod()
        {
            m_InterceptDropPod_Active = !m_InterceptDropPod_Active;
        }

        private void SwitchVisual()
        {
            m_ShowVisually_Active = !m_ShowVisually_Active;
        }

        private void FindUpgrades()
        {
            this.Map.listerBuildings.allBuildingsColonist.Where(x => x.AllComps.Any(c => c as Comp_ShieldUpgrade != null)).ToList().ForEach(b =>
            {
                b.AllComps.ForEach( c => {
                    CompProperties_ShieldUpgrade _Props = c.props as CompProperties_ShieldUpgrade;
                    if (_Props != null)
                    {
                        Log.Message("Found Comp, mp= " + _Props.m_FieldIntegrity_Multiplier);

                    }
                    
                });

            }
            );
        }
              
        #endregion

        #region Saving

        //Saving game
        public override void ExposeData()
        {
            base.ExposeData();

            //  Scribe_Deep.LookDeep(ref shieldField, "shieldField");

            Scribe_Values.Look(ref m_BlockIndirect_Active, "m_BlockIndirect_Active");
            Scribe_Values.Look(ref m_BlockDirect_Active, "m_BlockDirect_Active");
            Scribe_Values.Look(ref m_ShowVisually_Active, "m_ShowVisually_Active");
            Scribe_Values.Look(ref m_InterceptDropPod_Active, "m_InterceptDropPod_Active");

            Scribe_Values.Look(ref m_BlockIndirect_Avalable, "m_BlockIndirect_Avalable");
            Scribe_Values.Look(ref m_BlockDirect_Avalable, "m_BlockDirect_Avalable");
            Scribe_Values.Look(ref m_InterceptDropPod_Avalable, "m_InterceptDropPod_Avalable");

            Scribe_Values.Look(ref m_FieldIntegrity_Max, "m_FieldIntegrity_Max");
            Scribe_Values.Look(ref m_FieldIntegrity_Initial, "m_FieldIntegrity_Initial");
            Scribe_Values.Look(ref m_Field_Radius, "m_Field_Radius");

            Scribe_Values.Look(ref m_PowerRequired_Charging, "m_PowerRequired_Charging");
            Scribe_Values.Look(ref m_PowerRequired_Sustaining, "m_PowerRequired_Sustaining");

            Scribe_Values.Look(ref m_RechargeTickDelayInterval, "m_shieldRechargeTickDelay");
            Scribe_Values.Look(ref m_RecoverWarmupDelayTicks, "m_shieldRecoverWarmup");

            Scribe_Values.Look(ref m_ColourRed, "m_colourRed");
            Scribe_Values.Look(ref m_ColourGreen, "m_colourGreen");
            Scribe_Values.Look(ref m_ColourBlue, "m_colourBlue");

            Scribe_Values.Look(ref m_WarmupTicksRemaining, "m_WarmupTicksRemaining");

            Scribe_Values.Look(ref m_CurrentStatus, "m_CurrentStatus");
            Scribe_Values.Look(ref m_FieldIntegrity_Current, "m_FieldIntegrity_Current");
        }

        #endregion

    }
}