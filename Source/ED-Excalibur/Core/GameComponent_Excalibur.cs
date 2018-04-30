﻿using EnhancedDevelopment.Excalibur.Quest.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace EnhancedDevelopment.Excalibur.Core
{
    class GameComponent_Excalibur : Verse.GameComponent
    {

        public static GameComponent_Excalibur Instance;

        public GameComponent_Excalibur_NanoShields Shields;
        public GameComponent_Excalibur_Quest Quest;

        private List<GameComponent_BaseClass> m_SubComponents = new List<GameComponent_BaseClass>();

        #region Variables


        #endregion

        public GameComponent_Excalibur(Game game)
        {
            //Record the instance for easy acess.
            GameComponent_Excalibur.Instance = this;

            this.Shields = new GameComponent_Excalibur_NanoShields();
            this.m_SubComponents.Add(this.Shields);

            this.Quest = new GameComponent_Excalibur_Quest();
            this.m_SubComponents.Add(this.Quest);

        }

        //public override void GameComponentUpdate()
        //{
        //    Log.Message("GC.Update");
        //    base.GameComponentUpdate();
        //}

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            int _CurrentTick = Find.TickManager.TicksGame;

            this.m_SubComponents.ForEach(x => x.TickIfRequired(_CurrentTick));

            //this.Shields.Tick();
            //this.Quest.Tick();
            
        }

        public override void ExposeData()
        {
            base.ExposeData();
            this.m_SubComponents.ForEach(x => x.ExposeData());

            //this.Quest.ExposeData();
            //this.Shields.ExposeData();
        }

    }
}

