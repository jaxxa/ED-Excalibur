﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using EnhancedDevelopment.Excalibur.Core;

namespace EnhancedDevelopment.Excalibur.Quest.Dialog
{
    class Dialog_Excalibur : Window
    {

        #region Enumerations

        public enum EnumDialogTabSelection
        {
            SystemStatus,
            Buildings
        }

        #endregion

        #region Fields

        Vector2 m_ScrollPosition = new Vector2();
        int _TabSelectionHeight = 30;
        int _FooterSectionHeight = 35;

        private EnumDialogTabSelection m_CurrentTab = EnumDialogTabSelection.Buildings;

        #endregion

        #region Constructor

        public Dialog_Excalibur()
        {
            this.resizeable = false;
            this.optionalTitle = "E.D.S.N Exclibur";
            //this.CloseButSize = new Vector2(50, 50);

            this.doCloseButton = false;
            this.doCloseX = false;
            this.closeOnClickedOutside = false;
            this.doCloseX = true;


            //this.SetInitialSizeAndPosition();
        }

        #endregion

        #region Override Methods

        public override void DoWindowContents(Rect inRect)
        {

            this.DoGuiWholeWindowContents(inRect);

            //Widgets.ButtonText(inRect, "Button1", true, false, true);
            // throw new NotImplementedException();
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1024f, (float)UI.screenHeight);
            }
        }

        #endregion

        #region Gui Methods

        private void DoGuiWholeWindowContents(Rect totalCanvas)
        {
            // Headder --------------------------------------------------------
            Rect _TabSelectionRect = totalCanvas.TopPartPixels(_TabSelectionHeight);
            this.DoGuiHeadder(_TabSelectionRect);

            // Main Content ---------------------------------------------------

            Rect _WindowContent = new Rect(totalCanvas.xMin, _TabSelectionRect.yMax, totalCanvas.width, totalCanvas.height - _TabSelectionHeight - _FooterSectionHeight);
            //Widgets.ButtonText(_WindowContent, "_WindowContent", true, false, true);


            if (this.m_CurrentTab == EnumDialogTabSelection.SystemStatus)
            {
                this.DoGuiSystemStatus(_WindowContent);
            }
            else if (this.m_CurrentTab == EnumDialogTabSelection.Buildings)
            {
                this.DoGuiBuilding(_WindowContent);
            }

            // Footer (System Status) -----------------------------------------

            Rect _Footer = totalCanvas.BottomPartPixels(_FooterSectionHeight);
            //Widgets.ButtonText(_Footer, "_Footer", true, false, true);
            this.DoGuiFooter(_Footer);

        }

        public void DoGuiHeadder(Rect rectContentWindow)
        {

            // Headder to Select Tabs -----------------------------------------

            //Widgets.ButtonText(_TabSelectionRect, "_TabSelectionRect", true, false, true);

            WidgetRow _ButtonWidgetRow = new WidgetRow(rectContentWindow.x, rectContentWindow.y, UIDirection.RightThenDown, 99999f, 4f);
            if (_ButtonWidgetRow.ButtonText("Buildings", null, true, true))
            {
                this.m_CurrentTab = EnumDialogTabSelection.Buildings;
                //Find.WindowStack.Add(new Dialog_BillConfig(this, ((Thing)base.billStack.billGiver).Position));
            }

            if (_ButtonWidgetRow.ButtonText("System Status", null, true, true))
            {
                this.m_CurrentTab = EnumDialogTabSelection.SystemStatus;
                //Find.WindowStack.Add(new Dialog_BillConfig(this, ((Thing)base.billStack.billGiver).Position));
            }
            Widgets.DrawLineHorizontal(rectContentWindow.xMin, rectContentWindow.yMax, rectContentWindow.width);

        }

        public void DoGuiFooter(Rect rectContentWindow)
        {
            Widgets.DrawLineHorizontal(rectContentWindow.xMin, rectContentWindow.yMin, rectContentWindow.width);

            Widgets.TextArea(rectContentWindow.LeftHalf().LeftHalf(), "Nano Materials: " + Core.GameComponent_Excalibur.Instance.Comp_Quest.ResourceGetReserveStatus(Core.GameComponent_Excalibur_Quest.EnumResourceType.NanoMaterials).ToString() + " / " + Core.GameComponent_Excalibur.Instance.Comp_Quest.NanoMaterialsTarget.ToString(), true);


            Listing_Standard _listing_Standard_ShieldChargeLevelMax = new Listing_Standard();
            _listing_Standard_ShieldChargeLevelMax.Begin(rectContentWindow.LeftHalf().RightHalf());
            _listing_Standard_ShieldChargeLevelMax.ColumnWidth = 70;
            _listing_Standard_ShieldChargeLevelMax.IntAdjuster(ref Core.GameComponent_Excalibur.Instance.Comp_Quest.NanoMaterialsTarget, 1, 1);
            _listing_Standard_ShieldChargeLevelMax.NewColumn();
            _listing_Standard_ShieldChargeLevelMax.IntAdjuster(ref Core.GameComponent_Excalibur.Instance.Comp_Quest.NanoMaterialsTarget, 10, 1);
            _listing_Standard_ShieldChargeLevelMax.NewColumn();
            _listing_Standard_ShieldChargeLevelMax.IntSetter(ref Core.GameComponent_Excalibur.Instance.Comp_Quest.NanoMaterialsTarget, 10, "Default");
            _listing_Standard_ShieldChargeLevelMax.End();
        }

        public void DoGuiSystemStatus(Rect rectContentWindow)
        {

            float _ViewContentHeight = (Core.GameComponent_Excalibur.Instance.Comp_Quest.m_ShipSystems.Count() + 1) * ShipSystem.m_Height + 6f;

            Widgets.TextArea(rectContentWindow.TopPartPixels(20), "Ship Status", true);

            GUI.color = Color.white;
            Rect outRect = new Rect(rectContentWindow.x, rectContentWindow.y + 20, rectContentWindow.width, rectContentWindow.height - _FooterSectionHeight);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, _ViewContentHeight);

            //Above scroll view
            Widgets.DrawLineHorizontal(outRect.xMin, outRect.yMin, outRect.width);

            Widgets.BeginScrollView(outRect, ref m_ScrollPosition, viewRect, true);

            float num = 0f;
            for (int i = 0; i < Core.GameComponent_Excalibur.Instance.Comp_Quest.m_ShipSystems.Count(); i++)
            {
                ShipSystem _BuildingInProgress = Core.GameComponent_Excalibur.Instance.Comp_Quest.m_ShipSystems[i];
                Rect rect3 = _BuildingInProgress.DoInterface(0f, num, viewRect.width, i);
                //if (!bill.DeletedOrDereferenced && Mouse.IsOver(rect3))
                //{
                //    result = bill;
                //}
                num += rect3.height + 6f;
                Widgets.DrawLineHorizontal(viewRect.x, num, viewRect.width);
            }

            Widgets.EndScrollView();
        }

        public void DoGuiBuilding(Rect rectContentWindow)
        {

            Core.GameComponent_Excalibur.Instance.Comp_Fabrication.AddNewBuildingsUnderConstruction();

            Widgets.TextArea(rectContentWindow.TopPartPixels(20), "Building" + Core.GameComponent_Excalibur.Instance.Comp_Fabrication.BuildingsUnderConstruction.Count.ToString(), true);

            // Widgets.ButtonText(windowContent, "_WindowContent", true, false, true);


            // Vector2 _WindowSize = ITab_Fabrication.WinSize;
            //Rect _DrawingSpace = new Rect(0f, 0f, _WindowSize.x, _WindowSize.y).ContractedBy(10f);

            Rect _DrawingSpace = rectContentWindow;

            Rect _MainWindow = _DrawingSpace.TopPartPixels(_DrawingSpace.height - 25);
            Rect _InfoBar = _DrawingSpace.BottomPartPixels(25);

            //Func<List<FloatMenuOption>> _ConstructionOptionsMaker = delegate
            //{

            //    List<FloatMenuOption> _List = new List<FloatMenuOption>();

            //    DefDatabase<ThingDef>.AllDefs.ToList().ForEach(x =>
            //    {
            //        Fabrication.CompProperties_Fabricated _FabricationCompPropeties = x.GetCompProperties<Fabrication.CompProperties_Fabricated>();
            //        if (_FabricationCompPropeties != null && x.researchPrerequisites.All(r => r.IsFinished || string.Equals(r.defName, "Research_ED_Excalibur_Quest_Unlock")))
            //        {

            //            _List.Add(new FloatMenuOption(x.label + " - RU: " + _FabricationCompPropeties.RequiredMaterials + " P: " + _FabricationCompPropeties.RequiredPower, delegate {
            //               // GameComponent_Excalibur.Instance.Comp_Fabrication.OrderBuilding(x, this.SelectedCompTransponder.parent.Position, this.SelectedCompTransponder.parent.Map);
            //                GameComponent_Excalibur.Instance.Comp_Fabrication.OrderBuilding(x);
            //            }, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, x)));
            //        }
            //    }
            //    );

            //    return _List;
            //};



            GameComponent_Excalibur.Instance.Comp_Fabrication.DoListing(_MainWindow, ref this.m_ScrollPosition, ref viewHeight);


            Widgets.TextArea(_InfoBar, "RU:" + GameComponent_Excalibur.Instance.Comp_Quest.ResourceGetReserveStatus(GameComponent_Excalibur_Quest.EnumResourceType.ResourceUnits) + " Power: " + GameComponent_Excalibur.Instance.Comp_Quest.ResourceGetReserveStatus(GameComponent_Excalibur_Quest.EnumResourceType.Power).ToString(), true);



        }

        float viewHeight = 1000f;

        #endregion

    }
}
