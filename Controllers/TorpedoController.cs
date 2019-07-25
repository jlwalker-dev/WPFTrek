using System;

using WPFTrek.Utilities;

/* 
 * Torpedo controller class
 * Photon torpedoes almost always destroy an enemy ship.  Torpedoes
 * are fired along a course and they continue until they leave the 
 * sector or hit something.
 * 
 * Once all torpedoes are used up, you have to go back to a starbase
 * to restock your supply.
 */
namespace WPFTrek.Controllers
{

    class TorpedoController : ControllerClass, ControllerInterface
    {
        public TorpedoController(MainWindow game) : base(game)
        {
            base.Init("Torpedoes", 8, 100);

        }


        /*
         * Ask for the course and call GameObjects to create
         * an animation track for the torpedo
         */
        public bool Execute()
        {
            bool executed = false;
            _game.Debug("In Torpedoes");

            if (IsHealthy() && getCurrentCount()>0)
            {
                // get the course
                double course = Dialogs.CourseDialog("Torpedoes");

                // if a valid course, fire the torpedo
                if (course > 0)
                {
                    base.updateCurrentCount(-1);

                    // Enterprise with distance of -1 means fire a torpedo along the requested course
                    _game.GameObjects.SetObjectMovement(Course.CreateTrackList(_game.SRS.GetMyRow(), _game.SRS.GetMyCol(), course), -1);

                    // add time for each shot fired
                    _game.GameBoard.StarDateAdd(.1);
                    executed = true;
                }
            }
            else
            {
                _game.ComsChatter("Mr Chekov reports that photon torpedoes are unavailable!");
            }

            return executed;
        }

    }
}
