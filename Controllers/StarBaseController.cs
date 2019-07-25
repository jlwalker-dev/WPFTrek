using System;
using System.Collections.Generic;
using System.Linq;

using WPFTrek.Game;
using WPFTrek.Utilities;

/*
 * This controller tracks starbases in the game and sends an
 * update for sectors around it at a rate of one sector per stardate.
 */
namespace WPFTrek.Controllers
{
    class StarBaseController : ControllerClass, ControllerInterface
    {
        public const int CHECK_IN_ODDS = 75;
        private double lastUpdate = 0;

        public class StarBase 
        {
            // standard elements about the base
            public int gameBoardLoc;
            public Boolean sendUpdates = true;
            public int baseNo = 0;
            public int Health = 100;

            // we only care about the values of these elements
            // if they are less than the current stardate
            public double destroyedAfter = 9999.9;  // reset if Klingons in area to current SD+1.5
            public double lastAlert = 9999.9; // used to send out calls for help every .25 SD

            public StarBase(int loc)
            {
                this.baseNo++;
                this.gameBoardLoc = loc;
            }
        }

        List<StarBase> starBases = new List<StarBase>();

        public StarBaseController(MainWindow game) :base(game)
        {
        }


        public new void NewGame()
        {
            starBases = new List<StarBase>();
            base.NewGame();
        }


        /*
         * Each starbase sends periodic sector update
         * 
         */
        public Boolean Execute()
        {
            int s = 0;
            int k = 0;
            _game.Debug("In StarBase");

            // Remove destroyed starbase, check for klingons in sector for current starbases
            while (s < starBases.Count())
            {
                k = _game.GameMap.GetMyMap(starBases.ElementAt(s).gameBoardLoc);

                if (k > 99)
                {
                    k = (k % 100) / 10;

                    if (k > 0 && starBases.ElementAt(s).destroyedAfter > 9000.0)
                    {
                        // we just discovered klingons in the sector, set destroyed stardate
                        starBases.ElementAt(s).destroyedAfter = Math.Round(_game.GameBoard.CurrentStarDate() + (4.0 / (double)k), 1);
                        _game.ComsChatter("\r\n* * * Starbase " + starBases.ElementAt(s).baseNo + " is under attack * * *");
                        starBases.ElementAt(s).lastAlert = _game.GameBoard.CurrentStarDate();
                    }

                    if (k == 0 && starBases.ElementAt(s).destroyedAfter < 9000.0)
                    {
                        starBases.ElementAt(s).destroyedAfter = 9999.0;
                    }
                }

                s++;
            }


            // watch the clock on any starbases under attack
            s = 0;
            while (s < starBases.Count())
            {
                if (starBases.ElementAt(s).destroyedAfter < 9000.0)
                {
                    // have we passed the destroyed after date?
                    if (starBases.ElementAt(s).destroyedAfter < _game.GameBoard.CurrentStarDate())
                    {
                        // blow it up - but don't tell anyone
                        _game.GameBoard.RemoveObjectFromBoard(GameObjects.STARBASE, starBases.ElementAt(s).gameBoardLoc);
                        starBases.Remove(starBases.ElementAt(s));
                        continue;
                    }
                    else
                    {
                        if (_game.GameBoard.CurrentStarDate() - starBases.ElementAt(s).lastAlert > 0.25)
                        {
                            _game.ComsChatter("* * * Starbase " + starBases.ElementAt(s).baseNo + " is under attack! * * *");
                            starBases.ElementAt(s).lastAlert = _game.GameBoard.CurrentStarDate();
                        }
                    }
                }

                s++;
            }


            // return any updates from starbases
            if (starBases.Count() > 0)
            {
                if (lastUpdate - _game.GameBoard.CurrentStarDate() > 0.25)
                {
                    lastUpdate = _game.GameBoard.CurrentStarDate();

                    bool done = false;

                    for (int i = 0; i < starBases.Count(); i++)
                    {
                        done = false;
                        s = 100;

                        while (!done && s > 0 && starBases.ElementAt(i).sendUpdates)
                        {
                            int gbRow = starBases.ElementAt(i).gameBoardLoc / _boardSize;
                            int gbCol = starBases.ElementAt(i).gameBoardLoc % _boardSize;
                            s--;
                            int row = gbRow - 3 + Dice.roll(6);
                            int col = gbCol - 3 + Dice.roll(6);
                            int loc = row * _boardSize + col;

                            if (row >= 0 && row< _boardSize && col >= 0 && col < _boardSize)
                            {
                                if (_game.GameMap.GetMyMap(loc) < 0)
                                {
                                    _game.GameMap.SetMyMap(loc, _game.GameBoard.GetGameBoard(loc));
                                    _game.ComsChatter("Starbase " + starBases.ElementAt(i).baseNo
                                            + " completed sensor sweep of sector ("
                                            + (row % 8 + 1) + "," + (col % 8 + 1) + ")");
                                    done = true;
                                }
                            }
                        }

                        if (s < 3)
                        {
                            // not enough open cells, so no more updates
                            starBases.ElementAt(i).sendUpdates = false;
                        }
                    }
                }
            }

            _game.Debug("Exit Starbases");
            return true;
        }


        // Add a starbase at the requested row,col
        public void AddStarbase(int loc)
        {
            if (loc >= 0 && loc < _boardSize * _boardSize)
            {
                // add to the list
                starBases.Add(new StarBase(loc));

                // update myMap for local population and give check in msg
                _game.GameMap.SetMyMap(loc, _game.GameBoard.GetGameBoard(loc));
                _game.ComsChatter("Starbase " + starBases.Count() + " checking in!");
            }
        }
    }
}
