using System;

using WPFTrek.Game;
using WPFTrek.Controllers;

/* 
 * Some simple routines used in the system that did't rise to the level of
 * creating seperate static classes
 */
namespace WPFTrek.Utilities
{
    class GameUtility
    {

        public static int HitPower(StarObject starObject, SRSController srs,  double power)
        {
            double distance = getDistance(starObject.Row, starObject.Col, srs.GetMyRow(), srs.GetMyCol());
            distance= power / Math.Pow(distance, 0.4);
            return (int)(power < 5 ? 5 : distance);
        }


        /*
         * The square of the hypotenuse is equal to the sum of the square of the other two sides
         * See?  You really do use that stuff in real life!
         *
         */
        public static double getDistance(int r0, int c0, int r1, int c1)
        {
            return Math.Sqrt(Math.Abs(Math.Pow((r0 - r1), 2) + Math.Pow((c0 - c1), 2)));
        }

    }
}
