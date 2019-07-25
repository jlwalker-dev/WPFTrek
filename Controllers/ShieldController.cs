using System;
using WPFTrek.Utilities;

/*
 * Shields are imperfect, they only protect a a certain amount and
 * the rest leaks through to potentially damage the ship.  As hits
 * mount up, the shield efficiency lowers allowing more ship damage
 * unless you divert main power to the shields.
 * 
 * The shields, like torpedoes are a two part controller.  The
 * first part is the ability to use shields (health) and the
 * second part is the strength of the sheilds (currentCount).
 * 
 * An interesting alteration would be to allow the player to
 * put shield power back into the main energy reserves.
 * 
 */
namespace WPFTrek.Controllers
{
    class ShieldController : ControllerClass, ControllerInterface
    {
        // base effectiveness for shields at full strength
        public static double EFFICIENCY = 0.97;

        private bool active = false;

        public ShieldController(MainWindow game) : base(game)
        {
            // 100 energy & 100 hit points on controller
            base.Init("Shields", 100, 100);

        }


        /*
         * Raise and lower shields
         * 
         */
        public bool Execute()
        {
            bool result = false;

            if (getHealth()>0)
            {
                active = !active;
                _game.ComsChatter((active?"Raising":"Lowering")+" shields...");
                result = true;
            }
            else
            {
                active = false;
                _game.ComsChatter("I'm sorry Captain, but sheilds are inoperative");
            }

            return result;
        }


        /*
         * Are shields up?
         * 
         */
        public bool AreUp()
        {
            return active && getHealth()>0;
        }


        /*
         * Dirvert power from main to shields
         * 
         */
        public void divertPower()
        {
            double e = _game.GetEnergyLevel();
            int p;

            if (this.getHealth() == 0)
            {
                _game.ComsChatter("Soctty reports, 'Captain, yur askin' fer a miracle!  The shield controls ha' been destroyed.'");
            }
            else if (this.levelPercent() == 100)
            {
                _game.ComsChatter("Sheilds are at full strength, sir!");
            }
            else if(! IsHealthy())
            {
                _game.ComsChatter("Shields controls are are inoperative!");
            }
            else
            {
                if (e < 10.0)
                {
                    // You can't divert the last 10% of energy to the shields
                    _game.ComsChatter("Ship's energy levels are too low to divert to shields!");
                }
                else
                {
                    // we can't allow them to go below 10% power
                    p= (100 - this.levelPercent()) / 2 + 1; // power needed for full strength
                    e = Math.Round(((e - 10) > p ? p : (e - 10)), 0); // max power allowed
                    p = (int) Dialogs.GetValue("Divert Power to Shields","Amount of power to divert to shields?", 0, e);

                    if (p > 0)
                    {
                        if (p == (int) e)
                            base.Docked(true); // just makes current count = full count
                        else
                            this.updateCurrentCount(p * 2 - 1); // shield health is improved by 2 times energy diverted

                        _game.AdjustEnergy(-p);
                    }
                }
            }
        }


        /*
         * When docked, shield power goes to full strength
         * and are automatically lowered
         * 
         */
        public new void Docked(bool docked)
        {
            base.Docked(docked);
            if (docked && AreUp()) Execute();
        }


        /*
         * We start the game with shields down
         * 
         */
        public new void NewGame()
        {
            active = false;
            base.NewGame();
        }

        
        /*
         * Adjust active status if shield health fails
         * 
         */
        public new string TakeDamage(int d)
        {
            String r = base.TakeDamage(d);

            this.active = (AreUp() && IsHealthy());
            return r;
        }
    }
}
