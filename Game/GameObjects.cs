using System;
using System.Collections.Generic;
using System.Linq;

using WPFTrek.Utilities;


/*
 * This module keeps track of the objects and animation for the SRS
 * and provides the symbols for the SRS display
 * 
 * Future versions of this game might have all objects in the game 
 * defined for each sector.  Then when a player comes back into a
 * sector, everything will be where it was originally.  Adding
 * that change will also allow adding new pieces, such as
 * Klingon Battle Cruisers <K> and Romulans oRo.
 * 
 */
namespace WPFTrek.Game
{
    class GameObjects
    {
        static readonly object pblock = new object();

        public const int EMPTYSPACE = 0;
        public const int ASTEROID = 1;
        public const int KLINGON = 2;
        public const int STARBASE = 3;
        public const int ENTERPRISE = 4;
        // public const int BATTLECRUISER = 7;
        // public const int ROMULAN = 8;
        public const int TORPEDO = 9;

        private MainWindow game;

        private List<StarObject> starObjects = new List<StarObject>();
        private List<ObjectMovement> moveObjects = new List<ObjectMovement>();

        /*
         * Constructor for the class
         */
        public GameObjects(MainWindow g)
        {
            this.game = g;
            ClearObjects();
        }


        /*
         * Clear all objects from the sector board
         */
        public void ClearObjects()
        {
            starObjects = new List<StarObject>();
        }


        /*
         * Record an object to the starObjects list
         * 
         */
        public void AddStarObject(int type, int row, int col)
        {
            // remember this object for later use
            StarObject s = new StarObject();
            s.Row = row;        // row on sector map
            s.Col = col;        // column on sector map
            s.Type = type;  // type of object

            // hit points for the object
            s.Health = (type == GameObjects.ASTEROID ? 20 : (type == GameObjects.KLINGON ? 80 : 100));
            starObjects.Add(s);
        }


        /* 
         * Remove the object form the list that matches
         * the current row,col location
         * 
         */
        public void RemoveStarObjectAt(int row, int col)
        {
            // remove the object from the starObjects list
            // the list is very short so nothing fancy needed for the search
            for (int i = 0; i < starObjects.Count(); i++)
            {
                if (starObjects.ElementAt(i).Row == row && starObjects.ElementAt(i).Col == col)
                {
                    WriteToLog.write("gameObjects.removeStartObjectAt " + row + "," + col + " type=" + starObjects.ElementAt(i).Type);
                    game.GameBoard.RemoveObjectFromBoard(starObjects.ElementAt(i).Type, game.GameBoard.getMyLocation());
                    starObjects.Remove(starObjects.ElementAt(i));
                    i = starObjects.Count();
                }
            }
        }


        /*
         *  Return the map key for the object in the cell
         */
        public String GetObjectAt(int type)
        {
            String key = ".";

            switch (type)
            {
                case ASTEROID:
                    key = "*";
                    break;
                case KLINGON:
                    key = ">K<";
                    break;
                case -KLINGON:
                    key = "#K#";
                    break;
                case STARBASE:
                    key = "<S>";
                    break;
                case ENTERPRISE:
                    key = "<E>";
                    break;
                case TORPEDO:
                    key = "#";
                    break;
            }

            return key;
        }


        /*
         *  We're moving the Enterprise
         *  
         */
        public int SetObjectMovement(List<Track> track, int distance)
        {
            return SetObjectMovement(game.SRS.GetMyRow(), game.SRS.GetMyCol(), track, distance);
        }


        /* 
         * TODO - Klingons will typically move from - to and a track has to be created
         * 
         */
        public int SetObjectMovement(int fromRow, int fromCol, int toRow, int toCol, int distance)
        {
            // create a track from - to and send it on
            List<Track> track = new List<Track>();
            return SetObjectMovement(fromRow, fromCol, track, distance);
        }


        /*
         *  Create the list of movement animation actions
         *  
         */
        public int SetObjectMovement(int fromRow, int fromCol, List<Track> track, int distance)
        {
            // initialize everything
            int type = game.SRS.GetShortRangeSensors(fromRow, fromCol);
            int trackIndex = 0;
            int r, c, t;
            int dist = 0;
            bool done = false;
            bool animate = game.IsUsingAnimation();

            int lastr = fromRow;
            int lastc = fromCol;

            int currentLoc = game.GameBoard.getMyLocation();
            int gbLimit = game.GameBoard.GetSize() - 1;

            ObjectMovement m;

            // movement only works if there is a ship in the cell
            if (type == KLINGON || type == ENTERPRISE)
            {

                // If it's the enterprise, and distance is negative, we're 
                // firing a torpedo down the listed track
                if (type == 4 && distance < 0)
                {
                    // firing a torpedo!
                    type = TORPEDO;
                    distance = 99;

                    if (!animate)
                    {
                        m = new ObjectMovement(0, "CTorpedo Track", 0, 0);
                        moveObjects.Add(m);
                    }
                }

                // move the object along the track, stopping if it runs into something
                while (!done)
                {
                    r = track.ElementAt(trackIndex).Row;
                    c = track.ElementAt(trackIndex).Col;

                    if (r >= 0 && r < 10 && c >= 0 && c < 10)
                    {
                        if (r != fromRow || c != fromCol)
                        { // we don't want to mess with starting location
                            dist++;

                            //get what's in the cell along this track
                            t = game.SRS.GetShortRangeSensors(r, c);

                            if (t > 0)
                            {
                                // there is something at this location, so the 
                                // track ends here and something may happen
                                switch (type)
                                {
                                    case ENTERPRISE:
                                        // enterprise ran into something, stop engines
                                        m = new ObjectMovement(0, "CMr Scot says, 'Engines shut down to prevent collision.'", -1, -1);
                                        moveObjects.Add(m);
                                        break;

                                    case TORPEDO:
                                        // torpedo ran into something, make it go away
                                        m = new ObjectMovement(t, "CChekov yells, 'Direct hit, Kiptan!'", r, c);
                                        moveObjects.Add(m);

                                        for (int i = 0; i < starObjects.Count(); i++)
                                        {
                                            if (starObjects.ElementAt(i).Row == r && starObjects.ElementAt(i).Col == c)
                                            {
                                                // deliver 76 = 125 hit points so that klingons USUALLY die
                                                starObjects.ElementAt(i).Health -= Dice.roll(50) + 75;

                                                // did we destroy it?
                                                if (starObjects.ElementAt(i).Health < 1)
                                                {
                                                    m = new ObjectMovement(t, "D", r, c);
                                                    moveObjects.Add(m);
                                                }
                                            }
                                        }
                                        break;
                                }

                                // track ends
                                done = true;
                            }
                            else
                            {
                                // nothing found along the track (yet) so
                                // continue the movement animation
                                if (type == ENTERPRISE)
                                {
                                    m = new ObjectMovement(ENTERPRISE, "R", game.SRS.GetMyRow(), game.SRS.GetMyCol());
                                    moveObjects.Add(m);
                                    m = new ObjectMovement(ENTERPRISE, "A", r, c);
                                    moveObjects.Add(m);
                                    game.SRS.SetMyRow(r);
                                    game.SRS.SetMyCol(c);
                                }
                                else if (type != TORPEDO)
                                {
                                    m = new ObjectMovement(type, "R", lastr, lastc);
                                    moveObjects.Add(m);

                                    m = new ObjectMovement(type, "A", r, c);
                                    moveObjects.Add(m);
                                }
                                else
                                {
                                    m = new ObjectMovement(TORPEDO, "A", r, c);
                                    moveObjects.Add(m);
                                    moveObjects.Add(m);

                                    m = new ObjectMovement(TORPEDO, "R", r, c);
                                    moveObjects.Add(m);

                                    if(!animate)
                                    {
                                        m = new ObjectMovement(0, "C  (" + (r + 1).ToString() + "," + (c + 1).ToString()+")",0,0);
                                        moveObjects.Add(m);
                                    }
                                }
                            }

                            lastr = r;
                            lastc = c;
                        }
                    }
                    else
                    {
                        // the track takes us out of the sector, torpedos just sail away
                        // klingons will stop and the enterprise goes to a different sector
                        // if we're not at the edge of the galaxy
                        switch (type)
                        {
                            case ENTERPRISE:
                                int lr, lc;
                                int rfix = 0;
                                int cfix = 0;

                                // moving into other sector
                                while (!done)
                                {
                                    dist++;

                                    lr = currentLoc / game.GameBoard.GetSize();
                                    lc = currentLoc % game.GameBoard.GetSize();

                                    // are we trying to leave the galaxy?
                                    if (lr < 0 || lc < 0 || lr > gbLimit || lc > gbLimit)
                                    {
                                        m = new ObjectMovement(0, "CImpulse engines shut down at galactic border.", -1, -1);
                                        moveObjects.Add(m);
                                        done = true;
                                    }

                                    // continue to track through to the next sector(s)
                                    // rfix & cfix make sure the track appears to be in the grid
                                    // and when it leaves the grid, the sector location and fix 
                                    // values are updated
                                    if (!done)
                                    {
                                        currentLoc += (r + rfix < 0 ? -game.GameBoard.GetSize() : (r + rfix >= 10 ? game.GameBoard.GetSize() : 0));
                                        currentLoc += (c + cfix < 0 ? -1 : (c + cfix >= 10 ? 1 : 0));

                                        rfix += (r + rfix >= 10 ? -10 : 0);
                                        rfix += (r + rfix < 0 ? 10 : 0);
                                        cfix += (c + cfix >= 10 ? -10 : 0);
                                        cfix += (c + cfix < 0 ? 10 : 0);

                                        // have we gone the requested distance?
                                        if (dist < distance)
                                        {
                                            // we're just calculating moves, not showing
                                            trackIndex++;
                                            r = (track.ElementAt(trackIndex).Row);
                                            c = (track.ElementAt(trackIndex).Col);
                                        }
                                        else
                                        {
                                            // we're done, so set up the Location command
                                            m = new ObjectMovement(currentLoc, "L", r + rfix, c + cfix);
                                            moveObjects.Add(m);
                                            done = true;
                                        }
                                    }
                                }
                                break;

                            case TORPEDO:
                                m = new ObjectMovement(0, "CYou watch the torpedo sailing out of the sector; a clean miss!", -1, -1);
                                moveObjects.Add(m);
                                done = true;
                                break;
                        }
                    }

                    trackIndex++;

                    // we're done if we run out of track or we go the expected distance
                    done = (done || trackIndex >= track.Count() || dist >= distance);
                }
            }

            // Impulse burns power and time
            if(type==ENTERPRISE)
            {
                game.GameBoard.StarDateAdd(((double)dist) * 0.1);
                game.AdjustEnergy(dist * -0.1);
            }

            return dist; // actual distance traveled
        }


        public void MoveWithNoAnimation()
        {
            // this sucks
            string action = ShowObjectMovement();
            //Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle);

            WriteToLog.write("No Animation");

            while (action != null)
            {
                if(action[0].Equals('A'))
                {
                    game.ComsChatter("Track " + action.Substring(1));
                }

                WriteToLog.write("Action => " + action.Substring(1));
                action = ShowObjectMovement();
            }
        }

        /*
         * Process the top animation object and pop
         * it off the stack.  When done, return a null.
         * 
         */
        public String ShowObjectMovement()
        {
            lock (pblock)
            {
                WriteToLog.write("Showing Object Movement");

                String result = null;

                if (moveObjects != null && moveObjects.Count() > 0)
                {
                    // get the info and remove from the top of the stack
                    int type = moveObjects.ElementAt(0).Type;
                    String action = moveObjects.ElementAt(0).Action;
                    int row = moveObjects.ElementAt(0).Row;
                    int col = moveObjects.ElementAt(0).Col;
                    moveObjects.Remove(moveObjects.ElementAt(0));

                    WriteToLog.write("ShowObjectMovement Action " + action + " with " + type + " @ (" + row + "," + col + ")");

                    if (action.Substring(0, 1).Equals("F", StringComparison.OrdinalIgnoreCase))
                    {
                        // Enemy firing at Enterprise
                        if (action.Length > 1)
                        {
                            game.ComsChatter(action.Substring(1));
                        }
                        game.SRS.SetShortRangeSensors(row, col, type);
                        result = "F";
                    }
                    else if (action.Equals("L", StringComparison.OrdinalIgnoreCase))
                    {
                        // Landed in new sector location, clear objects, call SRS to redraw board
                        game.GameBoard.setMyLocation(type);
                        this.ClearObjects();
                        game.SRS.InitGrid(row, col);
                    }
                    else if (action.Substring(0, 1).Equals("C", StringComparison.OrdinalIgnoreCase))
                    {
                        // coms chatter - update the user
                        game.ComsChatter(action.Substring(1));
                        result = "C";
                    }
                    else
                    {
                        // update the SRS map, Add to cell or zero out for Remove/Destroy actions
                        game.SRS.SetShortRangeSensors(row, col, action.Equals("A", StringComparison.OrdinalIgnoreCase) ? type : 0);

                        if (action.Equals("D", StringComparison.OrdinalIgnoreCase))
                        {
                            // We destroyed something, remove it from the board & srs
                            for (int i = 0; i < starObjects.Count(); i++)
                            {
                                // Is this the object?
                                if (row == starObjects.ElementAt(i).Row && col == starObjects.ElementAt(i).Col)
                                {
                                    // Tell us what got destroyed
                                    switch (type)
                                    {
                                        case ASTEROID:
                                            game.ComsChatter("Asteroid destroyed.");
                                            break;
                                        case KLINGON:
                                            game.ComsChatter("Klingon destroyed!");
                                            break;
                                        case STARBASE:
                                            game.ComsChatter("Starfleet is ordering a General Courtmartial!");
                                            break;
                                    }

                                    // Destroyed, so remove it
                                    this.RemoveStarObjectAt(row, col);
                                }
                            }
                        }

                        // Return what we did so we can decide if we want to do 
                        // anything extra, or null if it was the last node
                        result = (moveObjects.Count() > 0 ? " AKSE    T".Substring(type, 1) + action : null);
                    }
                }

                return result;
            }
        }


        /*
         * Process the taking of fire from the enemy
         */
        public void TakingFire()
        {
            lock (pblock)
            {
                int p;
                String resp;
                ObjectMovement m;

                // loop through and take fire from Klingons based on their health
                for (int s = 0; s < starObjects.Count(); s++)
                {
                    p = starObjects.ElementAt(s).Health;

                    if (starObjects.ElementAt(s).Type == KLINGON && p > 0)
                    {
                        // disrupter fire from klingon ship
                        p = GameUtility.HitPower(starObjects.ElementAt(s), game.SRS, (int)p);

                        // create update on hit
                        m = new ObjectMovement(-KLINGON, "F" + p + " point hit from Klingon at (" + (starObjects.ElementAt(s).Row + 1) + "," + (starObjects.ElementAt(s).Col + 1) + ")", starObjects.ElementAt(s).Row, starObjects.ElementAt(s).Col);
                        moveObjects.Add(m);

                        m = new ObjectMovement(-KLINGON, "F", starObjects.ElementAt(s).Row, starObjects.ElementAt(s).Col);
                        moveObjects.Add(m);

                        m = new ObjectMovement(-KLINGON, "F", starObjects.ElementAt(s).Row, starObjects.ElementAt(s).Col);
                        moveObjects.Add(m);

                        m = new ObjectMovement(KLINGON, "F", starObjects.ElementAt(s).Row, starObjects.ElementAt(s).Col);
                        moveObjects.Add(m);

                        resp = game.DamageControl.takingDamage(p);

                        if (resp.Length > 0)
                        {
                            m = new ObjectMovement(0, "C" + resp, -1, -1);
                            moveObjects.Add(m);
                        }
                    }
                }
            }
        }


        /*
         *  Return the list of starObjects in this sector
         */
        public List<StarObject> GetGameObjects()
        {
            return this.starObjects;
        }


        /*
         * Return the object located at row,col
         */
        public StarObject GetGameObject(int row, int col)
        {
            StarObject response = null;

            for (int i = 0; i < starObjects.Count(); i++)
            {
                if (starObjects.ElementAt(i).Row == row && starObjects.ElementAt(i).Col == col)
                {
                    response = starObjects.ElementAt(i);
                    break;
                }
            }

            return response;
        }


        /*
         * Return the objects in the sector
         */
        public StarObject GetGameObject(int i)
        {
            return starObjects.ElementAt(i);
        }

    }
}

