using WPFTrek.Utilities;

/*
 * The Impulse engines are used to move in a sector.  Without
 * impulse, you would not be able to dock with the starbase
 * or manuever around enemy ships.
 * 
 */
namespace WPFTrek.Controllers
{
    class ImpulseController : ControllerClass, ControllerInterface
    {
        public ImpulseController(MainWindow game) : base(game)
        {
            base.Init("Impulse", 0, 100);

        }


        /*
         * Get course and distance, then send that infomation on to the
         * GameObjects class to create an animation track.
         * 
         */
        public bool Execute()
        {
            bool executed = false;

            _game.Debug("In Impulse");

            if (Usable())
            {
                // get the course
                double course = Dialogs.CourseDialog("Impulse");

                // if a valid course, move the enterprise
                if (course > 0)
                {
                    int distance = Dialogs.DurationDialog("Impulse", 12);

                    if (distance > 0)
                    {
                        int dist = _game.GameObjects.SetObjectMovement(Course.CreateTrackList(_game.SRS.GetMyRow(), _game.SRS.GetMyCol(), course), distance);
                        executed = true;
                    }
                }
            }
            else
            {
                // send error messsage to console that impulse is down
                _game.ComsChatter("Mr Scot reports impulse engines are still down!");

                // offer to fix it
                if (_game.GetAlertLevel() != MainWindow.REDALERT)
                {
                    double starDays = _game.DamageControl.CalculateDamageTime(this.getDamage());
                    double spend =(double) Dialogs.GetValue("Damage Control","It will take " + starDays.ToString() + " star days to completetly fix the impulse engines.  Choose how many to spend.", 0, starDays);

                    if (spend > 0)
                    {
                        int i = (int)(this.getDamage() * (spend / starDays));
                        this.RepairDamage(i);
                    }
                }
            }

            return executed;

        }

    }
}
