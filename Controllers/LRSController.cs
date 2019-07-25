/*
 * This controler looks at the game board sector and surrounding sectors and 
 * updates the GameMap class revealing those ares of the board.
 * 
 * If they are not working, you will always get a reading of the sector you
 * are in.
 * 
 */
namespace WPFTrek.Controllers
{
    class LRSController : ControllerClass, ControllerInterface
    {
        static readonly object pblock = new object();

        public LRSController(MainWindow game) : base(game)
        {
            base.Init("Long Range Sensors", 0, 100);
        }


        public bool Execute()
        {
            int row = _game.GameBoard.getMyLocation() / _boardSize;
            int col = _game.GameBoard.getMyLocation() % _boardSize;


            // We always map what's in our current sector unless SRS are also out
            if (IsHealthy() || _game.SRS.IsHealthy())
                _game.GameMap.SetMyMap(_game.GameBoard.getMyLocation(), _game.GameBoard.GetGameBoard());

            // Enterprise only updates all squares if LRS is working
            if (IsHealthy())
                SetLRS(row, col);

            return true;
        }

        /*
         * Load the LRS data (3x3) for the current location
         * This routine will also be called by probes
         * 
         */
        public void SetLRS(int row, int col)
        {
            _game.Debug("SetLRS for " + row + "," + col);

            for (int r = row - 1; r < row + 2; r++)
            {
                for (int c = col - 1; c < col + 2; c++)
                {
                    if (r >= 0 && r <= _boardSize && c >= 0 && c < _boardSize)
                    {
                        SetLRSCell(r * _boardSize + c);
                    }
                }
            }
        }

        public void SetLRS(int loc)
        {
            SetLRSCell(loc);
        }


        public void SetLRSCell(int loc)
        {
            int b = _game.GameBoard.GetGameBoard(loc);
            _game.GameMap.SetMyMap(loc, b);
        }
    }
}
