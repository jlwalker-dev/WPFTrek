using System;

using WPFTrek.Utilities;

/*
 * Damage control handles rapairs to the ship as long as there
 * are enough DC crew people available (health).  The crews will
 * work on critical systems to bring them back to health, with 
 * a very occassional full repair, during ship play.
 * 
 * You can also hae DC crews affect partial or full repair to the
 * ship when there are no enemy in the sector, but that wastes time
 * as they take half the amount of time to affect repairs when docked
 * to a starbase.
 * 
 * How long it takes is adjusted by the repairPointsPerStarDate.  The
 * game was released at 40 and that might be too generous as I have
 * won many games with 5+ days remaining.
 * 
 */
namespace WPFTrek.Controllers
{
    class DamageControl : ControllerClass, ControllerInterface
    {
        private double repairPointsPerStarDate = 40;

        public DamageControl(MainWindow game) : base(game)
        {
            base.Init("Damage Control", 0, 100);
        }


        /*
         * damage control controller - used to controller class 
         * for future version expansion
         */
        public bool Execute()
        {
            if (IsHealthy())
            {
                // DC crews are always working on certain parts of the
                // ship if they are not healthy (unless they fall to zero)
                // in which case they are not reparable without spending time
                Fix(_game.Shields);
                Fix(_game.Phasers);
                Fix(_game.Warp);
                Fix(_game.Impulse);
            }

            return true;
        }


        /* 
         * Calculates the amount of time that will be needed to repair 
         * all damages based on the repairPointsPerStarDate value
         * 
         * @return (double) stardays to complete all repairs to 1 significant digit
         */
        public double CalculateDamageTime(int damage)
        {
            double repairTime = damage / repairPointsPerStarDate;
            repairTime *= (_game.IsDocked() ? 0.5 : 1); // takes half the time when docked
            repairTime = Math.Round((repairTime < 0.1 ? 0.1 : repairTime),1);
            return repairTime;
        }


        /* 
         * DC works on certain controls (if they haven't
         * fallen to zero) until it can be considered healthy.
         * And occasionally something just gets fixed
         */
        public void Fix(ControllerClass obj)
        {
            if (!obj.IsHealthy() && obj.getHealth() > 0)
            {
                // if it's healthy, we might get some extra work done
                // if it's not healthy, we're going to make progress
                obj.RepairDamage(Dice.roll(3 - 1));
                if (obj.getDamage() == 0)
                {
                    _game.ComsChatter("Damage control chief reports repairs completed on " + obj.getDesc() + "!");
                }
                else if (obj.IsHealthy())
                {
                    _game.ComsChatter("Damage control chief reports that the " + obj.getDesc() + " is working!");
                }

            }
            else
            {
                // Occasionally Scotty or Spock figures something out
                if (!obj.IsHealthy() && Dice.roll(100) % 30 == 0)
                {
                    obj.RepairDamage(50);
                    _game.ComsChatter("Mr Sott pulled a miracle fix on the " + obj.getDesc() + ", Captain!");
                }
            }
        }


        /*
         * This routine tells us how much time will be required to
         * get to full health and we can then select how much time
         * to lose getting some things fixed up.  Shields, warp 
         * engines, and phasers have priority for repair.
         * 
         */
        public void FixAllDamage()
        {
            WriteToLog.write("DamageControl.fixAllDamage ");
            if (_game.GetAlertLevel() != MainWindow.REDALERT)
            {
                double spend = 0;

                int damage = _game.Impulse.getDamage()
                        + _game.Warp.getDamage()
                        + _game.Torpedoes.getDamage()
                        + _game.Shields.getDamage()
                        + _game.Phasers.getDamage()
                        + _game.LRS.getDamage();

                if (damage == 0)
                {
                    _game.ComsChatter("Damage control reports all systems ready.");
                }
                else
                {
                    double starDays = _game.DamageControl.CalculateDamageTime(damage);

                    if (_game.IsDocked())
                    {
                        if (starDays > .3)
                            starDays = Math.Round(_game.IsDocked() ? starDays / 2 : starDays, 1);
                    }

                    WriteToLog.write("    to fix = " + starDays + " starDays");

                    if (starDays > 0)
                    {
                        spend = (double)Dialogs.GetValue("Damage Control", "It will take " + starDays.ToString() + " star days to fix all repairs.  Choose how many to spend.", 0, starDays);
                        WriteToLog.write("    requested=" + spend);

                        if (spend > 0)
                        {
                            if (spend >= starDays)
                            {
                                // just fix everything
                                _game.Impulse.repairAllDamage();
                                _game.Warp.repairAllDamage();
                                _game.Torpedoes.repairAllDamage();
                                _game.Shields.repairAllDamage();
                                _game.Phasers.repairAllDamage();
                                _game.LRS.repairAllDamage();

                                _game.ComsChatter("All damage was repaired on time.");
                            }
                            else
                            {
                                int repairAvailable = (int)(damage * (spend / starDays) + 1);
                                int repairSpread = repairAvailable / 4;

                                // TODO - put these controllers to a list or array...
                                // perhaps some sort of registration process to DC...
                                // Then loop through fixing anything not healthy, and
                                // then any critical component under 50%, and finally
                                // everything that has damage

                                // loop until we've either fixed everything or
                                // we run out of points to fix the damages
                                while (repairAvailable > 0)
                                {
                                    WriteToLog.write("    damage in=" + damage + "   repairAvailable=" + repairAvailable);

                                    // priority is given to shields, warp, and phasers
                                    repairAvailable -= _game.Shields.RepairDamage(repairAvailable > repairSpread ? repairSpread : repairAvailable);
                                    repairAvailable -= _game.Warp.RepairDamage(repairAvailable > repairSpread ? repairSpread : repairAvailable);
                                    repairAvailable -= _game.Phasers.RepairDamage(repairAvailable > repairSpread ? repairSpread : repairAvailable);

                                    // if below 50%, do some more work
                                    repairAvailable -= _game.Shields.RepairDamage((_game.Shields.HealthPercent() < 40 ? repairAvailable > repairSpread ? repairSpread : repairAvailable : 0));
                                    repairAvailable -= _game.Warp.RepairDamage((_game.Shields.HealthPercent() < 40 ? repairAvailable > repairSpread ? repairSpread : repairAvailable : 0));
                                    repairAvailable -= _game.Phasers.RepairDamage((_game.Shields.HealthPercent() < 40 ? repairAvailable > repairSpread ? repairSpread : repairAvailable : 0));

                                    repairAvailable -= _game.Impulse.RepairDamage(repairAvailable > repairSpread ? repairSpread : repairAvailable);
                                    repairAvailable -= _game.Torpedoes.RepairDamage(repairAvailable > repairSpread ? repairSpread : repairAvailable);
                                    repairAvailable -= _game.LRS.RepairDamage(repairAvailable > repairSpread ? repairSpread : repairAvailable);

                                    WriteToLog.write("    repairAvailable=" + repairAvailable);
                                }

                                // return any time left
                                starDays -= (damage > 0 ? starDays - damage / repairPointsPerStarDate : 0);

                                _game.ComsChatter("Damage control crews report ready.");
                            }
                        }

                        WriteToLog.write(string.Empty + starDays + " added to current date");
                        _game.GameBoard.StarDateAdd(starDays);
                    }
                }
            }
            else
            {
                _game.ComsChatter("Damage control crews are busy.");

                if(_game.Debug(""))
                {
                    // Cheat to get through game faster while testing
                    for(int row=0;row<10;row++)
                    {
                        for(int col=0;col<10;col++)
                        {
                            if (_game.SRS.GetShortRangeSensors(row, col) == 2)
                                _game.GameObjects.RemoveStarObjectAt(row, col);
                        }
                    }

                    _game.Shields.updateCurrentCount(100);
                }
            }
        }


        /*
         * Damage is recored here and randomly assigned
         */
        public string takingDamage(int hit)
        {
            string response = string.Empty;

            if (_game.IsDocked())
            {
                response = "Starbase shields protect the Enterprise";
            }
            else
            {
                hit = (hit < 5 ? 5 : hit);  // enemy always hits with at least 5 pts

                _game.Debug("Enemy hit =" + hit);

                if (_game.Shields.AreUp())
                {
                    _game.Debug("Shield power =" + _game.Shields.levelPercent());
                    // The further we are from the enemy, the less their beam weapons affect us
                    double hitAdjust = 1D - ((double) (_game.Shields.levelPercent()) * ShieldController.EFFICIENCY) / 100;
                    hit = (int)(((double) hit) * (hitAdjust > .5 ? .5 : hitAdjust));

                    _game.Debug("    Adjusted hit =" + hit);
                    _game.Shields.updateCurrentCount(-hit);
                }


                // Every hit has potential of damaging something
                hit += (hit > 4 ? 0 : 2);

                // shields always take a hit, but other stuff does also
                // TOTO - break out shield controls from shields
                //_game.Shields.updateCurrentCount(-hit/2);

                // if shields are up and healthy, about 30% chance of no damage
                // but if down or not healthy, then damage does occur elsewhere
                switch (Dice.roll(_game.Shields.IsHealthy() && _game.Shields.AreUp() ? 10 : 7))
                {
                    case 1:
                        response += _game.Shields.TakeDamage(hit);
                        break;
                    case 2:
                        response += _game.Warp.TakeDamage(hit);
                        break;
                    case 3:
                        response += _game.Impulse.TakeDamage(hit);
                        break;
                    case 4:
                        response += _game.Phasers.TakeDamage(hit);
                        break;
                    case 5:
                        response += _game.Torpedoes.TakeDamage(hit);
                        break;
                    case 6:
                        response += _game.LRS.TakeDamage(hit);
                        break;
                    case 7:
                        response += _game.DamageControl.TakeDamage(hit / 3);
                        break;
                }
            }
            return response;
        }


        /*
         * When docked, the DC crew is replenished with and endless supply
         * of eager and dedicated on-station redshirts. (John Scalzi's book
         * RedShirts is a fun read, BTW).
         * 
         */
        public new void Docked(bool docked)
        {
            if (docked && ! AtFullHealth())
            {
                base.repairAllDamage();
                _game.ComsChatter("Lt Uhura reports that additional damage control crew have arrived from the starbase");
            }
        }
    }
}
