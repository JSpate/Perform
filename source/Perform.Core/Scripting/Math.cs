using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perform.Scripting
{
    public static class MathMethods
    {
        public static double Sin(int degrees)
        {
            double radians = degrees * (Math.PI / 180);
            return Math.Sin(radians);
        }

        public static double Sin(double degrees)
        {
            double radians = degrees * (Math.PI / 180);
            return Math.Sin(radians);
        }
    }
}
