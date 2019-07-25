using WPFTrek.Game;
using WPFTrek.Utilities;

/*
 * Phasers are a beam weapon that allow you to shoot as long as you
 * have main power.  Phaser fire is split among all enemy, so multiple
 * targets require many more shots.  Typically, two shots per enemy
 * ship is average because they are not very big ships.
 *
 * Some alterations would be needed here to support mulitple enemy ship types.
 * 
 */
namespace WPFTrek.Controllers
{
    class PhaserController : ControllerClass, ControllerInterface
    {
        public PhaserController(MainWindow game) : base(game)
        {
            base.Init("Phasers", 0, 100);

        }


        public bool Execute()
        {
            bool executed = false;
            int kCounter = 0;
            double power = 0;
            _game.Debug("In Phasers");

            if (IsHealthy())
            {
                // fire phasers at all klingons
                for (int i = 0; i < _game.GameObjects.GetGameObjects().Count; i++)
                {
                    kCounter += (_game.GameObjects.GetGameObject(i).Type == GameObjects.KLINGON ? 1 : 0);
                }

                if (kCounter < 1)
                {
                    _game.ComsChatter("Sir, there are no Klingons in system!");
                }
                else
                {
                    power = (int) Dialogs.GetValue("Phasers","Enter power level for Phasers", 0, 100);

                    if (power > 0)
                    {
                        // add time for each shot fired
                        _game.GameBoard.StarDateAdd(.05);
                        executed = true;

                        _game.AdjustEnergy(-power / 10.0); // ajust main reserves

                        power = power / kCounter; // split the power between all enemy

                        int i = 0;
                        while (i < _game.GameObjects.GetGameObjects().Count)
                        {
                            if (_game.GameObjects.GetGameObject(i).Type == GameObjects.KLINGON)
                            {
                                _game.GameObjects.GetGameObject(i).Health -= GameUtility.HitPower(_game.GameObjects.GetGameObject(i), _game.SRS, power);

                                if (_game.GameObjects.GetGameObject(i).Health <= 0)
                                {
                                    _game.ComsChatter("Klingon at (" + (_game.GameObjects.GetGameObject(i).Row + 1) + "," + (_game.GameObjects.GetGameObject(i).Col + 1) + ") was destroyed!");
                                    _game.SRS.RemoveStarObject(_game.GameObjects.GetGameObject(i).Row, _game.GameObjects.GetGameObject(i).Col);
                                }
                                else
                                {
                                    _game.ComsChatter("Klingon at (" + (_game.GameObjects.GetGameObject(i).Row + 1) + "," + (_game.GameObjects.GetGameObject(i).Col + 1) + ") was hit and is down to " + _game.GameObjects.GetGameObject(i).Health + "% power");
                                    i++;
                                }
                            }
                            else
                                i++;
                        }
                    }
                }
            }
            else
            {
                _game.ComsChatter("Mr Chekov reports that phasers are inoperative!");
            }

            return executed;
        }
    }
}
