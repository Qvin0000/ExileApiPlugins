using System;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using SharpDX;

namespace XPBar
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        #region ExpTable

        private readonly uint[] ExpTable =
        {
            525,
            176,
            3781,
            7184,
            12186,
            19324,
            29377,
            43181,
            61693,
            8599,
            117506,
            157384,
            207736,
            269997,
            346462,
            439268,
            551295,
            685171,
            843709,
            1030734,
            1249629,
            1504995,
            1800847,
            2142652,
            2535122,
            2984677,
            3496798,
            4080655,
            4742836,
            5490247,
            6334393,
            7283446,
            8384398,
            9541110,
            10874351,
            12361842,
            14018289,
            15859432,
            17905634,
            20171471,
            22679999,
            25456123,
            28517857,
            31897771,
            35621447,
            39721017,
            44225461,
            49176560,
            54607467,
            60565335,
            67094245,
            74247659,
            82075627,
            90631041,
            99984974,
            110197515,
            121340161,
            133497202,
            146749362,
            161191120,
            176922628,
            194049893,
            212684946,
            232956711,
            255001620,
            278952403,
            304972236,
            333233648,
            363906163,
            397194041,
            433312945,
            472476370,
            514937180,
            560961898,
            610815862,
            664824416,
            723298169,
            786612664,
            855129128,
            929261318,
            1009443795,
            1096169525,
            1189918242,
            1291270350,
            1400795257,
            1519130326,
            1646943474,
            1784977296,
            1934009687,
            2094900291,
            2268549086,
            2455921256,
            2658074992,
            2876116901,
            3111280300,
            3364828162,
            3638186694,
            3932818530,
            4250334444,
        };

        #endregion

        private uint CurDiff;
        private int CurLvl;
        private uint CurMax;
        private uint CurMin;

        public override void OnLoad()
        {
            var pExp = GameController.Player.GetComponent<Player>().XP;

            for (var i = 0; i < ExpTable.Length - 1; i++)
            {
                var exp1 = ExpTable[i];
                var exp2 = ExpTable[i + 1];

                if (pExp > exp1 && pExp < exp2)
                {
                    CurMin = exp1;
                    CurMax = exp2;
                    CurLvl = i + 2;
                    break;
                }
            }

            CurDiff = CurMax - CurMin;
        }

        public override void Render()
        {
            Initialise();

            //var expElement = GameController.Game.IngameState.UIRoot.GetChildFromIndices(1, 57, 12);
            var pExp = GameController.Player.GetComponent<Player>().XP;

            pExp -= CurMin;
            var proc = (float) pExp / CurDiff;
            proc *= 100;
            var procStr = CurLvl + ": " + Math.Round(proc, 3) + "%";
            var size = Graphics.MeasureText(procStr, 20);

            var scrRect = GameController.Window.GetWindowRectangle();

            var center = new Vector2(scrRect.X + scrRect.Width / 2, scrRect.Height - 10);
            center.Y -= 5;
            var textRect = center;
            textRect.Y -= 5;
            Graphics.DrawText(procStr, textRect, Color.White, FontAlign.Center); // - new Vector2(size.Width / 2, size.Height / 2)

            var drawRect = new RectangleF(center.X - 5 - size.X / 2, center.Y - size.Y / 2, size.X + 10, size.Y);

            Graphics.DrawBox(drawRect, Color.Black);
        }
    }
}
