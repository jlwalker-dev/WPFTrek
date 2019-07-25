using System;

/*
 * This holds the gameboard which contains the population of the galaxy.
 * Each cell is a sector and the population is described as a three
 * digit number.
 * 
 * Ex: 123 -> 1 Starbase, 2 Enemy Ships, 3 Asteroids
 * 
 * To include KBC & Romulan ships, you'll need to, at a minimum, update:
 *  GameObjects to hold all board pieces in the game along with their definitions.
 *  SRSController & ObjectMovement to handle the new definitions and hold all game pieces
 *  This class to create them during startup
 *  
 *  You might also consider allowing certain enemy ships to move toward and threaten starbases
 *  via impulse or very infrequent warp jumps.  You might also consider enemy ships in nearby
 *  sectors occassionally jumping into your sector to join in on a battle.
 *  
 */
namespace WPFTrek.Game
{
    class GameBoard
    {
        private int boardSize = 8;
        private int mapGridSize = 10;

        private MainWindow game;
        private int[] gameBoard;
        private int myLocation = 0;
        private int klingonCount;
        private double starDate = 3421;
        private double endDate = 0;


        public GameBoard(MainWindow game)
        {
            this.game = game;
        }


        public void NewGame(int gridSize)
        {
            int kSectors = 0;
            int t;

            this.boardSize = (gridSize < 8 || gridSize > 24 ? 8 : (gridSize / 8) * 8); // multiples of 8 only please
            this.mapGridSize = boardSize + 2;
            this.gameBoard = new int[boardSize * boardSize];

            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    // 40% chance of 1-4 klingons in each cell
                    t = (Utilities.Dice.roll(100) > 60 ? Utilities.Dice.roll(100) / 30 + 1 : 0);
                    klingonCount += t;
                    kSectors += (t > 0 ? 1 : 0);

                    // 0 - 5 stars in system - 5% chance of no stars
                    t = (t * 10) + (Utilities.Dice.roll(100) + 15) / 20;
                    gameBoard[row * boardSize + col] = t;
                }
            }

            // ad starbases - nothing in Delta quadrant
            gameBoard[sectorRoll(3, 2, 3, 2)] += 100;

            if (boardSize == 16)
            {
                gameBoard[sectorRoll(4, 9, 4, 2)] += 100;
                gameBoard[sectorRoll(4, 9, 4, 9)] += 100;
            }

            if (boardSize == 24)
            {
                gameBoard[sectorRoll(3, 7, 3, 9)] += 100;

                gameBoard[sectorRoll(3, 12, 3, 9)] += 100;
                gameBoard[sectorRoll(3, 17, 3, 9)] += 100;

                gameBoard[sectorRoll(3, 12, 3, 9)] += 100;
                gameBoard[sectorRoll(3, 17, 3, 9)] += 100;

            }

            this.endDate = this.starDate + kSectors;
        }



        /*
         * Place the starting location someplace on the gameboard
         * 
         */
        public void RandomEnterpriseLocation()
        {
            myLocation = sectorRoll(boardSize - 1, 0, boardSize / 2, 0);

            int a = GetGameBoard();
            while (a % 100 > 9)
            {
                myLocation = sectorRoll(boardSize - 1, 0, boardSize / 2, 0);
                a = GetGameBoard();
            }

            setMyLocation(myLocation);
        }


        /*
         * Get a random sector in a given area of the board 
         * 
         */
        private int sectorRoll(int d1, int o1, int d2, int o2) {
            return (Utilities.Dice.roll(d1) + o1) * boardSize + (Utilities.Dice.roll(d2) + o2);
        }


        /*
         * Return the current location
         * 
         */
        public String GetLocation()
        {
            return (myLocation / boardSize).ToString() + "," + (myLocation % boardSize).ToString();
        }


        /*
         * Update the stardate for the game
         * 
         */
        public void StarDateAdd(double time)
        {
            starDate += time;
        }


        /*
         * Return the game's current stardate
         */
        public double CurrentStarDate()
        {
            return starDate;
        }


        /*
         * How much time left in the game?
         */
        public double TimeLeft()
        {
            return this.endDate - this.starDate;
        }


        /*
         * Remove an object from the indicated
         * location on the board
         */
        public void RemoveObjectFromBoard(int type, int loc)
        {
            if (type == 2)
            {
                this.klingonCount--;
            }

            gameBoard[loc] -= (type == 1 ? 1 : (type == 2 ? 10 : 100));
        }


        /*
         * Add an object to the indicated
         * location on the board
         */
        public void AddObjectToBoard(int type, int loc)
        {
            if (type == 2)
                this.klingonCount++;

            gameBoard[loc] += (type == 1 ? 1 : (type == 2 ? 10 : 100));
        }


        /*
         * Return the quadrant name and sector
         * 
         */
        public String[] GetLocationInfo()
        {
            int row = this.myLocation / boardSize;
            int col = this.myLocation % boardSize;

            String[] results = new String[2];
            results[0] = GetQuadName(row, col);
            results[1] = "(" + (row % 8 + 1) + "," + (col % 8 + 1) + ")";
            return results;
        }


        /* 
         * Return the name of the quadrant based on board row & column
         * 8x8 board is just alpha quadrant, while 16x16 follows the
         * Star Trek designation.  24x24 throws in some constellation
         * names to round out the list
         */
        public String GetQuadName(int row, int col)
        {
            String response;

            if (boardSize == 8)
            {
                response = "Alpha";
            }
            else
            {
                String[] names = { "Gamma", "Delta", "Alpha", "Beta" };
                response = names[(row / (boardSize / 2)) * 2 + (col / (boardSize / 2))];
            }

            return response;
        }


        /* 
         * Population is returned in the following format:
         * 	Starbase count *100 + Klingon count *10 + asteroid count
         *	Example: 132 = 1 Starbase, 3 Klingons, 2 Asteroids
         * 
         * @return (int) population of the current location of the Enterprise
         * 
         */
        public int GetGameBoard()
        {
            return GetGameBoard(this.myLocation);
        }


        /*
         * @param (int) row of game board
         * @param (int) col of game board
         * @return (int) the gameBoard population based on row,col
         * 
         */
        public int GetGameBoard(int row, int col)
        {
            return GetGameBoard(row * boardSize + col);
        }


        /*
         * Get the population of the specified location
         * 
         */
        public int GetGameBoard(int loc)
        {
            int response = -1;
            if (loc >= 0 && loc < boardSize * boardSize)
            {
                response = gameBoard[loc];
            }

            return response;
        }


        /*
         * @param (int) location value of game board
         * @return (int) population of game board location
         * 
         */
        public void SetGameBoard(int value)
        {
            gameBoard[this.myLocation] = value;
        }


        /*
         * Return the size of the board
         * 
         */
        public int GetSize()
        {
            return boardSize;
        }


        /*
         * @return the location of Enterprise
         * 
         */
        public int getMyLocation()
        {
            return myLocation;
        }


        /*
         * @param myLocation setter
         */
        public void setMyLocation(int myLocation)
        {
            if (myLocation >= 0 && myLocation < boardSize * boardSize)
            {
                game.GameMap.EnterpriseMapMove(this.myLocation, myLocation);
                this.myLocation = myLocation;
            }
        }


        /*
         * @return number of klingons on the board
         * 
         */
        public int getKlingonCount()
        {
            return this.klingonCount;
        }
    }
}
