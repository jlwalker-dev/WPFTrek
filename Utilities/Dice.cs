using System;

/*
 * Dice roll returns integer value of 1 - sides
 * 
 */
namespace WPFTrek.Utilities
{
    static class Dice
    {
        static Random die = new Random();

        public static int roll(int sides)
        {
            return die.Next(1, sides);
        }
    }
}
