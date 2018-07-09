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
        public static void UpdateResearch()
        {

           if (GameComponent_Excalibur.Instance.Comp_Quest.m_QuestStatus >= 1)
            {
                ResearchHelper.QuestUnlock("Research_ED_Excalibur_AnalyseStrangeSignal");
            }


            if (GameComponent_Excalibur.Instance.Comp_Quest.m_QuestStatus >= 4)
            {
                ResearchHelper.QuestUnlock("Research_ED_Excalibur_Fabrication");
                ResearchHelper.QuestComplete("Research_ED_Excalibur_Fabrication");
            }


        }

        public static void QuestUnlock(string researchName)
        {
            ResearchProjectDef _Quest = DefDatabase<ResearchProjectDef>.GetNamed(researchName);
            _Quest.prerequisites.Remove(_Quest);
        }

        public static void QuestComplete(string researchName)
        {
            ResearchProjectDef _Quest = DefDatabase<ResearchProjectDef>.GetNamed(researchName);
            Find.ResearchManager.InstantFinish(_Quest, false);
        }


    }
}
