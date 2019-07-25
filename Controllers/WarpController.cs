using System;
using System.Collections.Generic;
using System.Linq;

using WPFTrek.Utilities;

/* 
 * Warp drives controller - allows for rapid movement
 * through the know galaxy.  Each sector move take up
 * .1 star dates at warp and .5 at impulse.
 * 
 * Warp adds to the main energy total while impulse drains
 * energy.  If warp is down, impulse will be offered as
 * an option.
 */
namespace WPFTrek.Controllers
{
    class WarpController : ControllerClass, ControllerInterface
    {
        public WarpController(MainWindow g) : base(g)
        {
            base.Init("Warp", 0, 100);
        }


        public bool Execute()
        {
            bool clearToWarp = Usable();
            bool executed = false;
            _game.Debug("In Warp");

            if (!clearToWarp) // && _game.impulse.isHealthy())
            {
                // warp is down, but you can use impulse to go where you want
                // at a cost of .5 star dates per warp... do you want to do that?
                String dialog = "The warp engines are down, but you can accomplish\n"
                            + "the same effect by using impulse engines at one half\n"
                            + "stardate for each sector traversed.\n\n"
                            + "Do you want to continue?";

                clearToWarp = Dialogs.YesNoDialog("Warp Down", dialog) == System.Windows.Forms.DialogResult.Yes;
            }


            if (clearToWarp)
            {
                int loc = _game.GameBoard.getMyLocation();
                int row = loc / _boardSize;
                int col = loc % _boardSize;

                double course = Dialogs.CourseDialog("Warp");

                if (course >= 1D)
                {
                    //System.out.println("Starting Track at "+row+","+col);
                    List<Track> track = Course.CreateTrackList(row, col, course);
                    int maxDuration = Dialogs.DurationDialog("Warp",12);

                    _game.Debug("Track length="+track.Count.ToString());

                    int i = (maxDuration > track.Count ? track.Count : maxDuration);

                    if (i > 0)
                    {
                        bool inGalaxy = true;
                        executed = true;

                        while (i > 0)
                        {
                            i--;


                            // Run DC for any unhealthy critical systems for each sector traversed
                            // If running on impulse, we have more time to do work, so more improvement
                            // would be expected for each critical system.
                            //int a = (IsHealthy() ? 0 : RepairDamage(IsHealthy() ? 1 : 4));
                            //a = (_game.impulse.isHealthy() ? 0 : _game.Impulse.repairDamage(isHealthy() ? 1 : 3));
                            //a = (_game.phasers.isHealthy() ? 0 : _game.Phasers.repairDamage(isHealthy() ? 1 : 3));
                            //a = (_game.phasers.isHealthy() ? 0 : _game.Torpedoes.repairDamage(isHealthy() ? 1 : 3));

                            // get coordinates
                            int c = track.ElementAt(i).Col;
                            int r = track.ElementAt(i).Row;

                            WriteToLog.write("Warp Track "+i+" - "+r+","+c);

                            // is the destination in the galaxy?
                            if (r >= 0 && r < _boardSize && c >= 0 && c < _boardSize)
                            {
                                // found a valid track location, moving to that sector
                                // add time and energy for each sector traversed
                                _game.GameBoard.StarDateAdd((i+1) * (IsHealthy() ? 0.1 : .5));
                                _game.AdjustEnergy((IsHealthy() ? 5 : -1) * (i+1));

                                i = 0;
                                _game.GameBoard.setMyLocation(r * _boardSize + c);
                                _game.ComsChatter("Now in sector " + _game.GameBoard.GetLocation());
                            }
                            else
                            {
                                // we can't go out of bounds
                                if (inGalaxy)
                                {
                                    _game.ComsChatter("Lt Uhura reports: 'Captain, Star Fleet forbids us from leaving the galaxy.'");
                                    inGalaxy = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _game.ComsChatter("Mr Scot says 'I'm sorry Captain, but me poor wee bairns are down!;");
            }

            return executed;
        }
    }
}
