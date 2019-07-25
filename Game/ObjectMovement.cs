using System;
using WPFTrek.Utilities;

/*
 * Used to hold the list of animation commands
 * 
 */
namespace WPFTrek.Game
{
    class ObjectMovement
    {
        /*
         *  class used in object animation
         */
        private int type = 0;
        private string action = string.Empty;
        private int row = -1;
        private int col = -1;

        public int Type { get => type; set => type = value; }
        public string Action { get => action; set => action = value; }
        public int Row { get => row; set => row = value; }
        public int Col { get => col; set => col = value; }


        // constructor to short cut adding values
        public ObjectMovement(int type, String action, int row, int col)
        {
            this.Type = type;
            this.Action = action;
            this.Row = row;
            this.Col = col;

            WriteToLog.write("ObjectMovement.add Type=" + type.ToString() + " Action=" + action + " @ " + row.ToString() + "," + col.ToString());
        }
    }
}
