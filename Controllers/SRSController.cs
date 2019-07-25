using System;
using System.Windows;
using System.Windows.Controls;

using WPFTrek.Utilities;
using WPFTrek.Game;

/*
 * Short Range Sensors controller
 * 
 * This routine keeps the sector map in the upper right corner
 * of the console screen updated.  When entering a sector, the
 * pieces are placed randomly onto the sector map.
 * 
 * This controller does not take damage.  I have a hard time figuring
 * out what to do if you can't "see out the window".  To that end, I
 * simply say if the SRS were ever to go down, you would alerady
 * be dead.
 * 
 * Because the pieces are not kept in memory once you leave the 
 * sector, all remaining enemy are brought back to full strength
 * when you come back.
 * 
 * The sector map is a two dimension int array which holds the
 * type of object in each element of the array.  Type values
 * are taken from the GameObjects class:
 * 
 *  EMPTYSPACE, ASTEROID, KLINGON, STARBASE, ENTERPRISE, TORPEDO
 *
 * There is also a two dimensional grid of labels which are
 * assigned to the SRSPanel on the MainWindow providing visual
 * updates to the panel.
 * 
 * It would be very possible to add other game pieces to this board
 * (like Romulans or Klingong Battle Cruisers) by altering the 
 * GameObjects and GameBoard classes to support them.  Some minor 
 * alterations to other classes may be needed, depending on the
 * desired effects.
 * 
 * A simple EnemyController might also allow for easier handling
 * of the enemy ships providing individual ships the abilty to move 
 * and manuever during game play.
 * 
 */
namespace WPFTrek.Controllers
{
    class SRSController : ControllerClass, ControllerInterface
    {
        private int[,] srsGrid = new int[10, 10];
        private Label[,] srsGridMap = new Label[10, 10];
        private String[] token = { ".", "*", ">K<", "<S>", "<E>" };

        int myRow = 0;
        int myCol = 0;

        public SRSController(MainWindow game) : base(game)
        {
            base.Init("Short Range Sensors", 0, 100);

            // Set up SRS Panel
            for (int row = 0; row < 10; row++)
            {
                Label l = new Label();
                l.Width = 40;
                l.Height = 25;
                l.Content = (row+1).ToString();
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l, 0);
                Grid.SetColumn(l, row+1);
                game.srsPanel.Children.Add(l);

                Label l2 = new Label();
                l2.Width = 40;
                l2.Height = 25;
                l2.Content = (row + 1).ToString();
                l2.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(l2, row+1);
                Grid.SetColumn(l2, 0);
                game.srsPanel.Children.Add(l2);

                for (int col = 0; col < 10; col++)
                {
                    srsGrid[row, col] = 0;
                    srsGridMap[row, col] = new Label();
                    srsGridMap[row, col].Width = 40;
                    srsGridMap[row, col].Height = 25;
                    srsGridMap[row, col].Content = ".";
                    srsGridMap[row, col].HorizontalContentAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(srsGridMap[row, col], row+1);
                    Grid.SetColumn(srsGridMap[row, col], col+1);
                    game.srsPanel.Children.Add(srsGridMap[row, col]);
                }
            }
        }


        public bool Execute()
        {
            _game.Debug("Execute SRS");
            _game.GameObjects.ClearObjects();
            InitGrid(-1, -1);
            return true;
        }


        /*
         * Set up the SRS map for the sector, randomly placing objects
         * onto the board with certain pieces requiring space between
         * other objects when placing.
         * 
         * If invalid row/col information is passed in, the Enterprise
         * will be randomly placed onto the board.
         */
        public void InitGrid(int row, int col)
        {
            int count, attempts;
            int population = _game.GameBoard.GetGameBoard();

            // clear the array
            _game.Debug("Clear array");
            for (int r = 0; r < 10; r++)
            {
                for (int c = 0; c < 10; c++)
                {
                    SetShortRangeSensors(r, c, 0);
                }
            }

            // Place the Enterprise
            _game.Debug("Place Enterprise");
            if (row < 0 || col < 0)
            {
                // don't randomly put the Enterprise near the edges
                this.myRow = Dice.roll(10 - 2);
                this.myCol = Dice.roll(10 - 2);
            }
            else
            {
                // must have moved from a different sector
                this.myRow = row;
                this.myCol = col;
            }

            AddStarObject(GameObjects.ENTERPRISE, myRow, myCol, 0);


            // Add asteroids, make sure they are next to each other
            _game.Debug("Add asteroids");
            count = population % 10;
            attempts = 0;
            while (count > 0 && attempts < 100)
            {
                attempts++;
                row = Dice.roll(10 - 2); ;  // don't want asteroids on the edges
                col = Dice.roll(10 - 2); ;

                if (AddStarObject(GameObjects.ASTEROID, row, col, 1))
                    count--;
            }

            // add starbase
            _game.Debug("Adding Starbase");
            while (population > 99)
            {
                row = Dice.roll(10 - 2);
                col = Dice.roll(10 - 2);

                // don't put the starbase near anything
                if (AddStarObject(GameObjects.STARBASE, row, col, 1))
                    break;
            }

            // klingons
            _game.Debug("Adding Klingons");
            count = (population % 100) / 10;
            while (count > 0)
            {
                row = Dice.roll(10 - 1);
                col = Dice.roll(10 - 1);

                if (AddStarObject(GameObjects.KLINGON, row, col, 0))
                    count--;
            }

            _game.Debug("Exiting SRS");
        }


        /*
         *  Add an object to the starObjects list and place it into the 
         * SRS map at the requested coordinates.  Optionally require a certain
         * amount of distance between other objects.  Return true if placement
         *  was successful.
         *  
         */
        public bool AddStarObject(int type, int row, int col, int distance)
        {
            bool ok2Place = true;

            if (srsGrid[row,col] == 0)
            {
                if (distance > 0)
                {
                    for (int r = row - distance; r <= row + distance; r++)
                    {
                        for (int c = col - distance; c <= col + distance; c++)
                        {
                            if (r >= 0 && r < 10 && c >= 0 && c < 10)
                            {
                                try
                                {
                                    ok2Place = ok2Place && srsGrid[r, c] == 0;
                                    //_game.GameObjects.AddStarObject(GameObjects.ENTERPRISE, myRow, myCol);
                                }
                                catch (Exception ex)
                                {
                                    WriteToLog.write("SRS Error - " + ex.Message);
                                }
                            }
                        }
                    }
                }

                if (ok2Place)
                {
                    _game.GameObjects.AddStarObject(type, row, col);
                    _game.Debug("Placing at " + row.ToString() + "," + col.ToString());

                    // place it on the map and signal a successful insertion
                    SetShortRangeSensors(row, col, type);
                    type = 0;
                }
                else
                {
                    _game.Debug("Failed to place at "+row.ToString()+","+col.ToString());
                }
            }

            return type == 0;
        }


        /*
         *  Removing an asteroid, klingon, or starbase
         * from the current location
         */
        public void RemoveStarObject(int row, int col)
        {
            this.srsGridMap[row,col].Content=".";
            _game.GameObjects.RemoveStarObjectAt(row, col);
        }



        /*
         * Get the srs map's value at this location
         */
        public int GetShortRangeSensors(int row, int col)
        {
            return this.srsGrid[row,col];
        }

        /*
         * Get the srs map's JLabel at this location
         */
        public Label GetSRSLabel(int row, int col)
        {
            return this.srsGridMap[row, col];
        }


        /*
         *  update the SRS board and the correct JLabel.  This is
         *
         * very quick and helps keep things pseudo-thread-safe
         * 
         */
        public void SetShortRangeSensors(int row, int col, int value)
        {
            _game.Dispatcher.Invoke(() =>
            {
                this.srsGrid[row, col] = value;
                srsGridMap[row, col].Content = _game.GameObjects.GetObjectAt(value);
            });
        }


        /*
         * look to see if there is a starbase in an adjacent cell
         */
        public bool AreWeDocked()
        {
            bool result = false;

            // this goes in srs
            // if we're done, check to see if we've docked with a starbase
            for (int i = this.myRow - 1; i < this.myRow + 2; i++)
            {
                for (int j = this.myCol - 1; j < this.myCol + 2; j++)
                {
                    if (i >= 0 && i < 10 && j >= 0 && j < 10)
                    {
                        if (this.srsGrid[i, j] == 3)
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }


        /*
         *  Return the row coordinate of the Enterprise
         */
        public int GetMyRow()
        {
            return this.myRow;
        }

        /*
         *  Set the row coordinate of the Enterprise
         */
        public void SetMyRow(int value)
        {
            this.myRow = value;
        }


        /*
         *  Return the column coordinate of the Enterprise
         */
        public int GetMyCol()
        {
            return this.myCol;
        }


        /*
         *  Set the column coordinate of the Enterprise
         */
        public void SetMyCol(int value)
        {
            this.myCol = value;
        }



        public string GetGridLegend(int row, int col)
        {
            return token[srsGrid[row, col]];
        }


        public void MoveSRSOjbect(int fromRow, int fromCol, int toRow, int toCol)
        {
            if(fromRow>=0 && fromRow<10 && fromCol>=0 && fromCol<10 && toRow>=0 && toRow<10 && toCol>=0 && toCol<10) { 
                SetGrid(toRow, toCol, srsGrid[fromRow, fromCol]);
                SetGrid(fromRow, fromCol, 0);
                //gameObjects.moveObject(fromRow,fromCol,toRow,toCol);
            }
        }

        public void SetGrid(int row, int col, int value)
        {
            srsGrid[row, col] = value;
            srsGridMap[row, col].Content = GetGridLegend(row, col);
        }

    }
}
