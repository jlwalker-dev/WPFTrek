using System;
using System.Windows.Forms;
using WPFTrek.Utilities;


/*
 * This is an example of a base class the is extended to make subclasses 
 * for the game.  Each system on the ship (phasers, torpedoes, etc) will
 * extend this class and inherit the capabilities of the class.
 * 
 * Common features, such as initization, docking, repairs, inflicting
 * damage, and others are written once.  This makes maintainance much 
 * easier and simplifies debugging.
 * 
 */
namespace WPFTrek.Controllers
{
    class ControllerClass
    {
        protected MainWindow _game;
        protected int _boardSize;

        // basic properties of each class
        private int maxAllowed = 0;     // max count allowed (such as torpedoes)
        private int currentCount = 0;       // current count
        private int maxHealth = 100;      // maximum health points
        private int currentHealth = 100;  // current health points
        private int healthPoint = 20;        // point at which system becomes inoperable

        private String description;     // description (phasers, warp engines, etc)

        // we just load the game object  reference during instantiation
        public ControllerClass(MainWindow g)
        {
            this._game = g;
            this._boardSize = _game.GameBoard.GetSize();
        }

        // Initialize as a specific type of controller.  Nothing really changes
        // based on what's sent, it's just recorded and used by the subclass 
        public void Init(String desc, int maxAllowed, int maxHealth)
        {
            this.description = desc;
            this.maxAllowed = maxAllowed;
            this.maxHealth = maxHealth;
            this.currentCount = maxAllowed;
            this.currentHealth = maxHealth;
            this.healthPoint = maxHealth / 5;
        }

        public bool Usable()
        {
            bool usable = IsHealthy();
            if (this.currentHealth < 20 && _game.GetAlertLevel() != MainWindow.REDALERT)
            {
                // get time to repair + 1
                double t = _game.DamageControl.CalculateDamageTime(getDamage()) + 1;

                // do you want to repair?
                bool answer = Dialogs.YesNoDialog("Repair Estimate","It will take " + string.Format("{0:0.0}", t) 
                    + " days to fabricate\nand install the parts to get the\n>system working again.\n\n"
                    + "Do you want to do that?")==DialogResult.Yes;

                // if yes, then repair between 22-44 percent
                if (answer)
                {
                    // adjust time
                    _game.GameBoard.StarDateAdd(t);
                    this.currentHealth = Dice.roll(20) + 22;
                    _game.ComsChatter("DC crews report that "+this.description + " is back online");
                    usable = true;
                }
            }

            return usable;
        }


        /*
         * Given a number of repair points, effect what repairs
         * we can.  Return how many repair points were used.
         */
        public int RepairDamage(int repairPoints)
        {
            //Utilities.writeToLog("    Controller " + this.description + ".repairDamage(" + repairPoints + ")");
            //Utilities.writeToLog("        currentHealth=" + (currentHealth));

            int usedPoints = repairPoints;

            // If we roll a multiple of 33, then use that as a basis for 
            // some extra repair work on this part of the ship, with no 
            // carryover to another part of the ship. 
            int diceRoll = Dice.roll(330);
            int extraRepairPointsExtra = (diceRoll % 33 == 0 ? diceRoll / 33 : 0);

            // Are there any repair points to use?
            if (repairPoints + extraRepairPointsExtra > 0)
            {
                if (currentHealth + repairPoints <= maxHealth)
                {
                    // we don't have enough for full repairs without 
                    // the extra repair points, so we're using up
                    // all of what's been sent to us
                    usedPoints = repairPoints;

                    // now add everything up
                    currentHealth += repairPoints + extraRepairPointsExtra;

                    // make sure we didn't go over with any extra points
                    currentHealth = (currentHealth > maxHealth ? maxHealth : currentHealth);
                }
                else
                {
                    // we're repairing it to full health and
                    // sending back how much we actually used
                    usedPoints = (maxHealth - currentHealth);
                    currentHealth = maxHealth;
                }
            }

            //Utilities.writeToLog("        new currentHealth=" + currentHealth + " and returning used=" + usedPoints);
            return usedPoints;
        }


        /*
         * Damage to the controller is sent here.  About a 3% chance of 
         * extra damage from a lucky hit.
         * 
         * @return (String) 
         */
        public String TakeDamage(int hitPoints)
        {
            String response = this.description + " takes " + hitPoints + " damage.";

            if (Dice.roll(100) % 33 == 0)
            {
                hitPoints += 5 + Dice.roll(10);
                response = "A lucky hit by the Klingon causes " + hitPoints + " damage.";
            }

            currentHealth = (currentHealth >= hitPoints ? currentHealth - hitPoints : 0);
            return (hitPoints > 0 ? response : "");
        }


        /*
         * By default we just "restock" when we dock with the
         * starbase.  Most controllers don't use this value.
         */
        public void Docked(bool docked)
        {
            if (docked)
            {
                currentCount = maxAllowed;
            }
        }


        /*
         * Minimum needed to setup a new game
         */
        public void NewGame()
        {
            this._boardSize = _game.GameBoard.GetSize();
            currentCount = maxAllowed;
            currentHealth = maxHealth;
        }


        /*
         * @return (Boolean) if controller should work
         */
        public bool IsHealthy()
        {
            return currentHealth >= healthPoint;
        }



        /*
         * @return (Boolean) is controller at full health?
         */
        public bool AtFullHealth()
        {
            return currentHealth == maxHealth;
        }


        /*
         * @return (int) percentage of total health
         */
        public int HealthPercent()
        {
            return currentHealth * 100 / maxHealth;

        }


        /*
         * @return (int) percentage of stocked items left
         */
        public int levelPercent()
        {
            return currentCount * 100 / maxAllowed;
        }


        /*
         * @param (int) value - amount to add or (-) subtract
         * 	or subtract to current amount
         */
        public void updateCurrentCount(int value)
        {
            currentCount = (currentCount + value < 0 ? 0 : (currentCount + value > maxAllowed ? maxAllowed : currentCount + value));
        }


        /*
         * @return (int) current count of stocked items
         */
        public int getCurrentCount()
        {
            return currentCount;
        }


        /*
         * @return (int) number of points needed to bring to full health
         */
        public int getDamage()
        {
            return maxHealth - currentHealth;
        }


        /*
         * @return (String) getter for description
         */
        public String getDesc()
        {
            return description;
        }


        /*
         * Set current health to max health
         */
        public void repairAllDamage()
        {
            currentHealth = maxHealth;
        }


        /*
         * @return (int) current health points
         */
        public int getHealth()
        {
            return currentHealth;
        }

    }
}
