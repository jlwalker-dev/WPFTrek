using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WPFTrek.Utilities;

/*
 * This is the map module shown in the lower half of the console.  When the LRS scans a sector, the 
 * information is transfered to this map so that the user can see it on the display.
 */
namespace WPFTrek.Game
{
    class GameMap
    {
        static readonly object pblock = new object();

        MainWindow game;
        private int[] myMap;
        private Label[] gameMap;
        private int boardSize = 0;
        private int mapGridSize = 0;


        /*
         * Constructor for class to set up reference to MainWindow
         */
        public GameMap(MainWindow game)
        {
            this.game = game;
            NewGame();
        }


        /*
         * Set up this module for a new game
         */
        public void NewGame() { 
            lock (pblock)
            {
                WriteToLog.write("*** Building map panel ***");
                game.mapPanel.Children.Clear();

                this.boardSize = game.GameBoard.GetSize();
                this.mapGridSize = boardSize + 2;

                // create a blank map
                myMap = new int[boardSize * boardSize];
                this.gameMap = new Label[boardSize * boardSize];

                double gridHeight = 340 / mapGridSize;
                double gridLength = 800 / mapGridSize;

                // Now set up the map grid panel
                for (int row = 0; row < mapGridSize; row++)
                {
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new System.Windows.GridLength(gridHeight, System.Windows.GridUnitType.Pixel);
                    gridRow.MaxHeight = gridHeight;
                    game.mapPanel.RowDefinitions.Add(gridRow);

                    for (int col = 0; col < mapGridSize; col++)
                    {
                        ColumnDefinition gridCol = new ColumnDefinition();
                        gridCol.Width = new System.Windows.GridLength(gridLength, System.Windows.GridUnitType.Pixel);
                        gridCol.MaxWidth = gridLength;
                        game.mapPanel.ColumnDefinitions.Add(gridCol);
                    }
                }

                for (int row = 0; row < boardSize; row++)
                {
                    Label l = new Label();
                    l.Width = gridLength;
                    l.Height = gridHeight;
                    l.Content = (row + 1).ToString();
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(l, 0);
                    Grid.SetColumn(l, row + 1);
                    game.mapPanel.Children.Add(l);

                    Label l2 = new Label();
                    l2.Width = gridLength;
                    l2.Height = gridHeight;
                    l2.Content = (row + 1).ToString();
                    l2.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(l2, row + 1);
                    Grid.SetColumn(l2, 0);
                    game.mapPanel.Children.Add(l2);


                    for (int col = 0; col < boardSize; col++)
                    {
                        myMap[ToLoc(row, col)] = -1;

                        // set up the label def for this grid location
                        gameMap[ToLoc(row, col)] = new Label();
                        gameMap[ToLoc(row, col)].Width = gridLength;
                        gameMap[ToLoc(row, col)].Height = gridHeight;
                        gameMap[ToLoc(row, col)].Content = "---";
                        gameMap[ToLoc(row, col)].HorizontalContentAlignment = HorizontalAlignment.Center;
                        Grid.SetRow(gameMap[ToLoc(row, col)], row + 1);
                        Grid.SetColumn(gameMap[ToLoc(row, col)], col + 1);
                        game.mapPanel.Children.Add(gameMap[ToLoc(row, col)]);
                    }
                }
            }
        }



        /*
         * Return loc value based on row,col input
         */
        private int ToLoc(int row, int col)
        {
            return row * boardSize + col;
        }


        /*
         * @param (int) loc - cell from the map
         * @return (int) - the myMap population based on loc position
         * 
         */
        public int GetMyMap(int loc)
        {
            if (loc >= 0 && loc < boardSize * boardSize)
                return myMap[loc];
            else
                return -2;
        }


        /*
         * Place the population of a cell onto the user's map
         * 
         * @param (int) loc
         * @returns (int) population to put into cell
         * 
         */
        public void SetMyMap(int loc, int value)
        {
            lock (pblock)
            {
                if (loc >= 0 && loc < boardSize * boardSize)
                {
                    this.myMap[loc] = value;
                    String strVal = ("000" + value.ToString());

                    if (gameMap[loc] != null)
                    {
                        SetGameMapContent(loc, strVal.Substring(strVal.Length - 3));
                        SetGameMapForeGround(loc, ((value > 99 ? (value % 100 > 9 ? Brushes.Red : Brushes.Blue) : (value > 9 ? Brushes.Black : Brushes.Gray))));
                    }
                }
            }
        }



        /*
         * Change the colors of the labels for movement of the Enterprise
         * 
         */
        public void EnterpriseMapMove(int locFrom, int locTo)
        {
            lock (pblock)
            {
                if (locFrom >= 0 && locFrom < boardSize * boardSize)
                {
                    SetGameMapBackGround(locFrom, Brushes.White);
                    //WriteToLog.write(string.Format("    Moving from {0}", locFrom));
                    //gameMap[locFrom].Background = Brushes.White;
                }

                if (locTo >= 0 && locTo < boardSize * boardSize)
                {
                    SetGameMapBackGround(locTo, Brushes.Cyan);
                    //WriteToLog.write(string.Format("    Moving to {0}", locTo));
                    //gameMap[locTo].Background = Brushes.Cyan;
                }
            }
        }


        /*
         * Update the label for the game map location array
         */
        private void SetGameMapContent(int loc, string val)
        {
            lock(pblock)
            {
                game.Dispatcher.Invoke(() =>
                {
                    gameMap[loc].Content = val;
                });
            }
        }


        /* 
         * Sets the background color of the label cell
         */
        private void SetGameMapBackGround(int loc, Brush brushColor)
        {
            lock(pblock)
            {
                game.Dispatcher.Invoke(() =>
                {
                    gameMap[loc].Background = brushColor;
                });
            }
        }


        /*
         * Sets the foreground color of the label cell
         */
        private void SetGameMapForeGround(int loc, Brush brushColor)
        {
            lock (pblock)
            {
                game.Dispatcher.Invoke(() =>
                {
                    gameMap[loc].Foreground = brushColor;
                });
            }
        }
    }
}
