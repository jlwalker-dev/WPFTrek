using WPFTrek.Utilities;

/*
 * I may have seen reference to this in some other game, but
 * a probe can be launched into another sector to show
 * what is there and in surrounding sectors.  This was
 * added for the larger boards (16, 24 & 32 size grids).
 * 
 * In the larger grid games, you might want to limit its
 * reach to a number of sectors from the ship or give
 * it the ability to scan further than the immediate 
 * surrounding sectors.
 * 
 */
namespace WPFTrek.Controllers
{
    class ProbeController : ControllerClass, ControllerInterface
    {
        public ProbeController(MainWindow game) : base(game)
        {
            base.Init("Probes", 3, 100);

        }



        /*
         * Launch a proble and get the results.  Probles are nothing but warp
         * engine, power core, and LRS so they are fast, single-use devices
         * that return the results quickly.
         * 
         */
        public bool Execute()
        {
            bool executed = false;
            _game.Debug("In Probes");

            if (IsHealthy() && getCurrentCount() > 0)
            {
                // Target sector
                string sect = Dialogs.BasicInputDialog("Probe Launch", "Send probe to what sector?");

                // if a valid course, fire the probe
                if (sect!=null)
                {
                    string[] sects = sect.Split(',');
                    if (sects.Length==2)
                    {
                        int row;
                        int col;

                        if (int.TryParse(sects[0], out row) && int.TryParse(sects[1], out col))
                        {
                            if (row > 0 && row <= _boardSize && col > 0 && col <= _boardSize) 
                            {
                                base.updateCurrentCount(-1);
                                _game.LRS.SetLRS(row-1, col-1);
                                _game.GameBoard.StarDateAdd(.1);
                                _game.ComsChatter("Mr Spock says 'The probe data is coming in, Captain");
                                executed = true;
                            }
                        }
                    }

                    if(! executed)
                    {
                        _game.ComsChatter("The bridge crew are confused by your response");
                    }
                }
            }
            else
            {
                _game.ComsChatter("Mr Chekov reports that there are no probes unavailable!");
            }

            return executed;
        }

    }
}
