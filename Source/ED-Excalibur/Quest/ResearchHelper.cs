﻿using EnhancedDevelopment.Excalibur.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EnhancedDevelopment.Excalibur.Quest
{
    static class ResearchHelper
    {
        public static void UpdateQuestStatusResearch()
        {

            if (GameComponent_Excalibur.Instance.Comp_Quest.m_QuestStatus >= 1)
            {
                ResearchHelper.QuestUnlock("Research_ED_Excalibur_AnalyseStrangeSignal");
            }
            
            if (GameComponent_Excalibur.Instance.Comp_Quest.m_QuestStatus >= 4)
            {
                GameComponent_Excalibur.Instance.Comp_Quest.ShipSystem_Fabrication.UpgradeTo(1);

                //ResearchHelper.QuestComplete("Research_ED_Excalibur_Fabrication");
            }        

        }

        public static void QuestUnlock(string researchName)
        {
            ResearchProjectDef _QuestResearch = DefDatabase<ResearchProjectDef>.GetNamed(researchName);

            if (_QuestResearch.requiredResearchFacilities != null)
            {
                _QuestResearch.requiredResearchFacilities.Clear();
            }
        }

        public static void QuestComplete(string researchName)
        {
            ResearchHelper.QuestUnlock(researchName);
            ResearchProjectDef _QuestResearch = DefDatabase<ResearchProjectDef>.GetNamed(researchName);
            Find.ResearchManager.FinishProject(_QuestResearch, false);
        }

        public static Boolean IsResearched(string researchName)
        {
            ResearchProjectDef _Quest = DefDatabase<ResearchProjectDef>.GetNamed(researchName);
            return _Quest.IsFinished;
        }

    }
}
