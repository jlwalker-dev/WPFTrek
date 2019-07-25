using System;
using System.Collections.Generic;
using System.Linq;

/* 
 * Course calculation for creating an animation track
 * and interacting with other objects on the SRS grid
 * 
 */
namespace WPFTrek.Utilities
{
    class Course
    {
        /*
         * Logic to create the track list was pulled from the QuickBASIC source code
         * located on Bob's Games page www.bobsoremweb.com/startrek.html
         * 
         * Thanks Bob!
         */
        public static List<Track> CreateTrackList(int row, int col, double course)
        {
            List<Track> track = new List<Track>();
            double y1 = row;
            double x1 = col;
            double c = (course - 1D) * .785398;
            double y0 = Math.Cos(c);
            double x0 = -Math.Sin(c);

            int x2, y2;

            // go to 48 in case we've got a 24x24 board in play
            while (track.Count() < 48)
            {
                y1 = y1 - y0;
                x1 = x1 - x0;

                // rounding occurs here instead up in the init of y1,x2 because
                // testing found that it creates a cleaner track.  Don't know if
                // the original code had a bug or if the math routines worked a 
                // bit different in the QuickBASIC version
                y2 = (int)(y1 + 0.5D);
                x2 = (int)(x1 + 0.5D);

                // only record a track if the coordinates changed... sometimes you
                // get a repeat of the coordinates due to rounding issues.
                // We don't care if coordinates go out of grid confines, that's taken
                // care of later in the process
                if (track.Count() == 0 || track.ElementAt(track.Count() - 1).Col != x2 || track.ElementAt(track.Count() - 1).Row != y2)
                {
                    Track t = new Track();
                    t.Col = x2;
                    t.Row = y2;
                    track.Add(t);
                }
            }

            return track;
        }



        /* 
         * Calculate course to target.  Uses supplied position and you
         * give it a target location (row,col).  
         * 
         */
        public static String Calculation(string title, int size, int row, int col)
        {
            double r1 = row;
            double c1 = col;

            double dir = 0;
            double r2 = 0D;
            double c2 = 0D;

            String results = null;
            String[] coords = null;

            // request the coordinates of the target.  If the user
            // exits without putting in a value, a null is returned
            // and we exit the routine sending back a blank string
            while (results == null)
            {
                results = Dialogs.BasicInputDialog(title,"Enter coordinates of target position (row,col)");

                if (results == null) break;

                // they should have entered coordinates in format of row,col
                coords = results.Split(',');

                if (coords.Count() == 2)
                {
                    // see if we can recover values from the input
                    double.TryParse(coords[0].Trim(), out r2);
                    double.TryParse(coords[1].Trim(), out c2);

                    r2--;
                    c2--;

                    if (! (double.IsNaN(r2) || double.IsInfinity(r2) || double.IsNaN(c2) || double.IsInfinity(c2)))
                    {
                        // make sure we have valid values and if not then we'll loop around
                        if (r2 >= 0 && r2 < size && c2 >= 0 && c2 < size)
                        {
                            // we're good!
                            break;
                        }

                        // didn't have valid coordinates
                        results = null;
                    }
                }
            }


            if (results != null)
            {
                // Get the degrees, correct for the 90 degree skew + 360 to make
                // sure we get a positive angle, then get the mod of 360 which
                // is the correct degrees to target (per our perspective).  Now 
                // convert it to the 1 - 8.9 scale by multiplying it by 0.222223
                // (which is 8/360) and adding 1 to get the course to target.
                dir = 0.0222223 * ((toDegrees(Math.Atan2(r2 - r1, c2 - c1)) + 450) % 360D) + 1;

                dir = Math.Round(dir,1);

                // now get a track to retrieve the distance, which is
                // the number of track iterations to the target
                int dist = 0;
                int row2 = (int)r2;
                int col2 = (int)c2;
                List<Track> track = CreateTrackList(row, col, dir);
                for (int i = 0; i < track.Count(); i++)
                {
                    if (row2 == track.ElementAt(i).Row && col2 == track.ElementAt(i).Col)
                    {
                        dist = i + 1;
                    }
                }

                // return what the computer will say
                results = string.Format("course {0:0.0}, distance {1}", dir, dist);
            }

            return results;
        }


        /*
         * Does the Java Math.toDegrees funcitonality
         * Code derived from https://stackoverflow.com/questions/1311049/how-to-map-atan2-to-degrees-0-360
         * 
         */
        public static double toDegrees(double atan2) 
        {
            double result = atan2 * (180.0 / Math.PI);
            return (result > 0.0 ? result : (360.0 + result));
        }
    }
}
