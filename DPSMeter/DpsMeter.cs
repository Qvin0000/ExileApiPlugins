using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Exile;
using Exile.PoEMemory.MemoryObjects;
using JM.LinqFaster;
using PoEMemory.Components;
using Shared.Enums;
using Shared.Helpers;
using Shared.Nodes;
using SharpDX;

namespace DPSMeter
{

    public class CacheLife
    {
        public long Life { get; set; }

        
    }
    
    public class DpsMeter : BaseSettingsPlugin<DPSMeterSettings>
    {
        private const double DPS_PERIOD = 0.2;
        private DateTime lastTime;
        private double MaxDpsAoe;
        private double MaxDpsSingle;
        private double CurrentDpsAoe;
        private double CurrentDpsSingle;
        private double CurrentDmgAoe;
        private double CurrentDmgSingle;
        private double[] SingleDamageMemory = new double[20];
        private double[] AOEDamageMemory = new double[20];
        private double time;
        public double TotalLifeAroundMonster;

        public override void OnLoad()
        {
            CanUseMultiThreading = true;
        }
        public DpsMeter()
        {
            Order = 50;
        }

        public override void AreaChange(AreaInstance area)
        {
            Clear(area);
        }

        public override bool Initialise()
        {

            GameController.LeftPanel.WantUse(() => Settings.Enable);
            return true;

        }

        private void Clear(AreaInstance area)
        {
            MaxDpsAoe = 0;
            MaxDpsSingle = 0;
            CurrentDpsAoe = 0;
            CurrentDpsSingle = 0;
            CurrentDmgAoe = 0;
            CurrentDmgSingle = 0;
            lastTime = DateTime.Now;
            SingleDamageMemory = new double[20];
            AOEDamageMemory = new double[20];
        }

        public override Job Tick() {
            if (!Settings.Enable) return null;
            if (Settings.MultiThreading)
            {
              return GameController.MultiThreadManager.AddJob(TickLogic, nameof(DpsMeter));
            }
            TickLogic();
            return null;
        }


        void TickLogic() {
            time += GameController.DeltaTime;
            if (time >= Settings.UpdateTime)
            {
                time = 0;
                CalculateDps(out var aoe, out var single);

                //Shift array
                //{ 1, 2, 3, 4, 5 }
                //{ 0, 1, 2, 3, 4 }
                Array.Copy(AOEDamageMemory, 0, AOEDamageMemory, 1, AOEDamageMemory.Length - 1);
                Array.Copy(SingleDamageMemory, 0, SingleDamageMemory, 1, SingleDamageMemory.Length - 1);

                AOEDamageMemory[0] = aoe;
                SingleDamageMemory[0] = single;
                if (single > 0)
                {
                    CurrentDmgAoe = aoe;
                    CurrentDmgSingle = single;

                    CurrentDpsAoe = AOEDamageMemory.SumF();
                    CurrentDpsSingle = SingleDamageMemory.SumF();



                    MaxDpsAoe = Math.Max(CurrentDpsAoe, MaxDpsAoe);
                    MaxDpsSingle = Math.Max(CurrentDpsSingle, MaxDpsSingle);

                }
            }
        }

        private void CalculateDps(out long aoeDamage, out long singleDamage) {
            TotalLifeAroundMonster = 0;
            aoeDamage = 0;
            singleDamage = 0;
            foreach (var monster in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
            {
                var cacheLife = monster.GetHudComponent<CacheLife>();
                if(cacheLife==null) continue;
                var life = monster.GetComponent<Life>();
                if (life == null) continue;
                if (!monster.IsAlive && Settings.HasCullingStrike.Value)
                {
                    continue;
                }
                int hp = monster.IsAlive ? life.CurHP + life.CurES : 0;

                
                if (hp > -1 && hp < 30000000)
                {
                    TotalLifeAroundMonster += hp;
                    if (cacheLife.Life != hp)
                    {
                        var dmg = cacheLife.Life - hp;
                        if (dmg > life.MaxHP + life.MaxES)
                            dmg = life.MaxHP + life.MaxES;

                        aoeDamage += dmg;
                        singleDamage = Math.Max(singleDamage, dmg);
                    }

                    cacheLife.Life = hp;
                }
            }
            
        }

        public override void EntityAdded(Entity Entity)
        {
            if (!Entity.HasComponent<Monster>() || !Entity.IsHostile || !Entity.IsAlive) return;

            var life = Entity.GetComponent<Life>();
            if (life != null)
            {
                Entity.SetHudComponent(new CacheLife {Life = life.CurHP > 0 ? life.CurHP + life.CurES : 0});
            }
            
           
        }

       
        private string aoe_hit = "aoe hit";
        private string hit = "hit";
        private string aoe_dps = "aoe dps";
        private string dps = "dps";
        private string max_aoe_dps = "max aoe dps";
        private string max_dps = "max dps";
        private string hp = "hp";

        public override void Render()
        {
            if (GameController.Area.CurrentArea==null || !Settings.ShowInTown && GameController.Area.CurrentArea.IsTown ||!Settings.ShowInTown && GameController.Area.CurrentArea.IsHideout)
            { return; }

            var position = GameController.LeftPanel.StartDrawPoint;
            var startY = position.Y;
            var measury = Graphics.MeasureText($"12345678 {max_aoe_dps}");
            var positionLeft = position.Translate(-measury.X, 0);
            System.Numerics.Vector2 drawText;

            if (Settings.ShowCurrentHitDamage.Value)
            {
                if (Settings.ShowAOE.Value)
                {
                    drawText = Graphics.DrawText(CurrentDmgAoe.ToString(), positionLeft, Settings.DpsFontColor,FontAlign.Left);
                    drawText = Graphics.DrawText(aoe_hit, position, Settings.DpsFontColor,FontAlign.Right);
                    position.Y += drawText.Y;
                    positionLeft.Y += drawText.Y;
                }
                drawText = Graphics.DrawText(CurrentDmgSingle.ToString(), positionLeft, Settings.DpsFontColor, FontAlign.Left);
                drawText = Graphics.DrawText(hit, position, Settings.DpsFontColor, FontAlign.Right);
                position.Y += drawText.Y;
                positionLeft.Y += drawText.Y;
            }

            if (Settings.ShowAOE.Value)
            {
                drawText= Graphics.DrawText(CurrentDpsAoe.ToString(), positionLeft, Settings.PeakFontColor, FontAlign.Left);
                drawText= Graphics.DrawText(aoe_dps, position, Settings.PeakFontColor, FontAlign.Right);
                position.Y += drawText.Y;
                positionLeft.Y += drawText.Y;
            }

            drawText = Graphics.DrawText(CurrentDpsSingle.ToString(), positionLeft, Settings.PeakFontColor, FontAlign.Left);
            drawText = Graphics.DrawText(dps, position, Settings.PeakFontColor, FontAlign.Right);
            position.Y += drawText.Y;
            positionLeft.Y += drawText.Y;
            if (Settings.ShowAOE.Value)
            {
                drawText = Graphics.DrawText(MaxDpsAoe.ToString(), positionLeft, Settings.PeakFontColor, FontAlign.Left);
                drawText = Graphics.DrawText(max_aoe_dps, position, Settings.PeakFontColor, FontAlign.Right);
                position.Y += drawText.Y;
                positionLeft.Y += drawText.Y;
            }
            drawText = Graphics.DrawText(MaxDpsSingle.ToString(), positionLeft, Settings.PeakFontColor, FontAlign.Left);
            drawText = Graphics.DrawText(max_dps, position, Settings.PeakFontColor, FontAlign.Right);
            position.Y += drawText.Y;
            positionLeft.Y += drawText.Y;
            
            drawText = Graphics.DrawText(TotalLifeAroundMonster.ToString(), positionLeft, Settings.PeakFontColor, FontAlign.Left);
            drawText = Graphics.DrawText(hp, position, Settings.PeakFontColor, FontAlign.Right);
            position.Y += drawText.Y;
            positionLeft.Y += drawText.Y;
            var width = 150;
            var bounds = new RectangleF(positionLeft.X-50, startY+3,measury.X+50,position.Y-startY);

            Graphics.DrawImage("preload-new.png", bounds, Settings.BackgroundColor);
            GameController.LeftPanel.StartDrawPoint = position;

        }
    }
}
