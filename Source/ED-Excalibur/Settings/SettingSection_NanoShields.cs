﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace EnhancedDevelopment.Excalibur.Settings
{
    class SettingSection_NanoShields : SettingSection
    {

        private const int DEFAULT_NANO_SHIELD_CHARGE_LEVEL_MAX = 200;
        private const int DEFAULT_NANO_SHIELD_BUILDING_CHARGE_DELAY = 30;
        private const int DEFAULT_NANO_SHIELD_BUILDING_CHARGE_AMOUNT = 1;
        private const int DEFAULT_NANO_SHIELD_BUILDING_RESERVE_POWER_MAX = 400;

        public int NanoShieldChargeLevelMax = SettingSection_NanoShields.DEFAULT_NANO_SHIELD_CHARGE_LEVEL_MAX;
        public int NanoShieldBuildingChargeDelay = SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_DELAY;
        public int NanoShieldBuildingChargeAmount = SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_AMOUNT;

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = 250f;
            listing_Standard.Begin(canvas);
            //listing_Standard.set_ColumnWidth(rect.get_width() - 4f);

            //ShieldChargeLevelMax
            listing_Standard.GapLine(12f);
            listing_Standard.Label("ShieldChargeLevelMax (Default " + SettingSection_NanoShields.DEFAULT_NANO_SHIELD_CHARGE_LEVEL_MAX.ToString() + "):" + this.NanoShieldChargeLevelMax.ToString());
            listing_Standard.Gap(12f);
            Listing_Standard _listing_Standard_ShieldChargeLevelMax = new Listing_Standard();
            _listing_Standard_ShieldChargeLevelMax.Begin(listing_Standard.GetRect(30f));
            _listing_Standard_ShieldChargeLevelMax.ColumnWidth = 70;
            _listing_Standard_ShieldChargeLevelMax.IntAdjuster(ref this.NanoShieldChargeLevelMax, 5, 1);
            _listing_Standard_ShieldChargeLevelMax.NewColumn();
            _listing_Standard_ShieldChargeLevelMax.IntAdjuster(ref this.NanoShieldChargeLevelMax, 20, 1);
            _listing_Standard_ShieldChargeLevelMax.NewColumn();
            _listing_Standard_ShieldChargeLevelMax.IntSetter(ref this.NanoShieldChargeLevelMax, SettingSection_NanoShields.DEFAULT_NANO_SHIELD_CHARGE_LEVEL_MAX, "Default");
            _listing_Standard_ShieldChargeLevelMax.End();

            //BuildingChargeDelay
            listing_Standard.GapLine(12f);
            listing_Standard.Label("BuildingChargeDelay (Default " + SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_DELAY .ToString() + "measuered in Ticks):" + this.NanoShieldBuildingChargeDelay.ToString());
            listing_Standard.Gap(12f);
            Listing_Standard _listing_Standard_BuildingChargeDelay = new Listing_Standard();
            _listing_Standard_BuildingChargeDelay.Begin(listing_Standard.GetRect(30f));
            _listing_Standard_BuildingChargeDelay.ColumnWidth = 70;
            _listing_Standard_BuildingChargeDelay.IntAdjuster(ref this.NanoShieldBuildingChargeDelay, 1, 1);
            _listing_Standard_BuildingChargeDelay.NewColumn();
            _listing_Standard_BuildingChargeDelay.IntAdjuster(ref this.NanoShieldBuildingChargeDelay, 10, 1);
            _listing_Standard_BuildingChargeDelay.NewColumn();
            _listing_Standard_BuildingChargeDelay.IntSetter(ref this.NanoShieldBuildingChargeDelay, SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_DELAY, "Default");
            _listing_Standard_BuildingChargeDelay.End();

            //BuildingChargeAmount
            listing_Standard.GapLine(12f);
            listing_Standard.Label("BuildingChargeAmount (Default " + SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_AMOUNT.ToString() + "):" + this.NanoShieldBuildingChargeAmount.ToString());
            listing_Standard.Gap(12f);
            Listing_Standard _listing_Standard_BuildingChargeAmount = new Listing_Standard();
            _listing_Standard_BuildingChargeAmount.Begin(listing_Standard.GetRect(30f));
            _listing_Standard_BuildingChargeAmount.ColumnWidth = 70;
            _listing_Standard_BuildingChargeAmount.IntAdjuster(ref this.NanoShieldBuildingChargeAmount, 1, 1);
            _listing_Standard_BuildingChargeAmount.NewColumn();
            _listing_Standard_BuildingChargeAmount.IntAdjuster(ref this.NanoShieldBuildingChargeAmount, 10, 1);
            _listing_Standard_BuildingChargeAmount.NewColumn();
            _listing_Standard_BuildingChargeAmount.IntSetter(ref this.NanoShieldBuildingChargeAmount, SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_AMOUNT, "Default");
            _listing_Standard_BuildingChargeAmount.End();

            listing_Standard.GapLine(12f);

            listing_Standard.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.NanoShieldChargeLevelMax, "NanoShieldChargeLevelMax", SettingSection_NanoShields.DEFAULT_NANO_SHIELD_CHARGE_LEVEL_MAX);
            Scribe_Values.Look(ref this.NanoShieldBuildingChargeDelay, "NanoShieldBuildingChargeDelay", SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_DELAY);
            Scribe_Values.Look(ref this.NanoShieldBuildingChargeAmount, "NanoShieldBuildingChargeAmount", SettingSection_NanoShields.DEFAULT_NANO_SHIELD_BUILDING_CHARGE_AMOUNT);
        }

        public override string Name()
        {
            return "Nano Shields";
        }
    }
}
