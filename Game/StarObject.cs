/*
 * Used to track objects in the SRS grid
 * 
 */
namespace WPFTrek.Game
{
    class StarObject
    {
        private int type = 0;
        private int row = 0;
        private int col = 0;
        private int health = 0;

        public int Type { get => type; set => type = value; }
        public int Row { get => row; set => row = value; }
        public int Col { get => col; set => col = value; }
        public int Health { get => health; set => health = value; }
    }
}
