using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultApp
{
    public class MetroColor
    {
        private static Random rand = new Random();
        public static int Count = 22;
        public enum MetroColors
        {
            Transparent = -3,
            White = -2,
            Black = -1,
            DarkGray = 0,
            Silver = 1,
            Orange = 2,
            Brown = 3,
            Red = 4,
            Blue = 5,
            SunFlower = 6,
            Magenta = 7,
            Purple = 8,
            Lime = 9,
            Teal = 10,
            Pink = 11,
            LightBlue = 12,
            Emerald = 13,
            Crimson = 14,
            Concrete = 15,
            Cloud = 16,
            Asbestos = 17,
            Cobalt = 18,
            Indigo = 19,
            DarkBlue = 20,
            LightGray = 21
        }
        public static MetroColors GetRandomColor()
        {
            return (MetroColors)rand.Next(0, Count);
        }
    }
}
