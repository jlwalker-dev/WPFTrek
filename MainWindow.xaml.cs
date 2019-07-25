using System;
using System.Windows;
using System.Windows.Controls;
using System.Timers;


using WPFTrek.Utilities;
using WPFTrek.Controllers;
using WPFTrek.Game;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace WPFTrek
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly object pblock = new object();

        // part of the hot key setup
        public static RoutedCommand rcImpulse = new RoutedCommand();
        public static RoutedCommand rcWarp = new RoutedCommand();
        public static RoutedCommand rcTorpedoes = new RoutedCommand();
        public static RoutedCommand rcPhasers = new RoutedCommand();
        public static RoutedCommand rcShields = new RoutedCommand();


        // colors for buttons on the game display
        private readonly SolidColorBrush REDISH=new SolidColorBrush(Color.FromRgb(255,200,200));
        private readonly SolidColorBrush GREENISH =new SolidColorBrush(Color.FromRgb(180, 255,180));
        private readonly SolidColorBrush LTGREEN =new SolidColorBrush(Color.FromRgb(230, 255,230));
        private readonly SolidColorBrush LTYELLOW =new SolidColorBrush(Color.FromRgb(240, 245,200));
        private readonly bool USE_ANIMATION = true;

        // timer control
        Timer timer;
        private int tickTock = 0;
        private int timerStep = 0;
        //private bool inTimer = false;

        // alert status
        public const int GREENALERT = 1;
        public const int YELLOWALERT = 2;
        public const int REDALERT = 3;

        // status flags
        private int alertLevel = 0;
        private bool docked = false;
        private bool debug = false;
        private double energyLevel = 100.0;

        // debug var
        long failInfo;

        // sub class references
        internal GameBoard GameBoard { get; set; }
        internal GameMap GameMap { get; set; }
        internal SRSController SRS { get; set; }
        internal LRSController LRS { get; set; }
        internal WarpController Warp { get; set; }
        internal TorpedoController Torpedoes { get; set; }
        internal PhaserController Phasers { get; set; }
        internal ShieldController Shields { get; set; }
        internal GameObjects GameObjects { get; set; }
        internal ImpulseController Impulse { get; set; }
        internal DamageControl DamageControl { get; set; }
        internal StarBaseController StarBases { get; set; }
        internal ProbeController Probes { get; set; }


        /*
         * 
         */
        public MainWindow()
        {
            InitializeComponent();

            // start of custom code
            WriteToLog.write("---- System Start ----", string.Empty, true);
            debug = Config.ReadElement("Debug", "False", true).Equals("true", StringComparison.OrdinalIgnoreCase);

            // initialize the sub classes
            GameBoard = new GameBoard(this);
            GameMap = new GameMap(this);
            SRS = new SRSController(this);
            LRS = new LRSController(this);
            Warp = new WarpController(this);
            Torpedoes = new TorpedoController(this);
            Phasers = new PhaserController(this);
            Shields = new ShieldController(this);
            Impulse = new ImpulseController(this);
            GameObjects = new GameObjects(this);
            DamageControl = new DamageControl(this);
            StarBases = new StarBaseController(this);
            Probes = new ProbeController(this);

            // set up a new board and start the timer
            NewGame(8);
            CreateTimer();

            WriteToLog.write("Debug = " + debug.ToString());
            WriteToLog.ClearOldLogs();

            // set up the hotkeys
            rcImpulse.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rcImpulse, ImpulseCommand));

            rcWarp.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rcWarp, WarpCommand));

            rcTorpedoes.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rcTorpedoes, TorpedoCommand));

            rcPhasers.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rcPhasers, PhaserCommand));

            rcShields.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(rcShields, ShieldCommand));
        }


        /*
         * Routine to set everything up for a new game
         */
        public void NewGame(int gridSize)
        {
            WriteToLog.write("---- Game Start ----");

            // initialize the other classes
            GameBoard.NewGame(gridSize);
            GameMap.NewGame();
            SRS.NewGame();
            LRS.NewGame();
            Warp.NewGame();
            Torpedoes.NewGame();
            Phasers.NewGame();
            Shields.NewGame();
            Impulse.NewGame();
            DamageControl.NewGame();
            StarBases.NewGame();
            Probes.NewGame();

            ComsClear();

            // Register starbases
            for (int i = 0; i < GameBoard.GetSize() * GameBoard.GetSize(); i++)
            {
                if (GameBoard.GetGameBoard(i) > 99)
                {
                    // Register this starbase
                    StarBases.AddStarbase(i);
                    WriteToLog.write("Added starbase to loc " + i.ToString());
                }
            }

            // locate the enterprise and play the game
            GameBoard.RandomEnterpriseLocation();
            WriteToLog.write("Enteprise is in sector "+GameBoard.GetLocation());

            SRS.Execute();
            SetCondition();
        }


        /*
         * Getter for alert level 
         */
        public int GetAlertLevel()
        {
            return alertLevel;
        }


        /*
         * Set the status board, update ship functions, and other clean up after
         * each turn is complete.
         */
        private void SetCondition()
        {
            // On occassion, a pblock was causing issues
            // so I need a way to fail and keep trying
            // instead of waiting forever
            var obj = new object();
            
             while (true)
             {
                 using (var tryLock = new TryLock(obj))
                 {
                     if (tryLock.HasLock)
                     {
                         SetConditionExecute();
                         endGameCheck();
                         break;
                     }
                     else
                    {
                        failInfo++;
                        if (failInfo > 10000)
                        {
                            WriteToLog.write("***** SETCONDITION FAILURE");
                            break;
                        }
                    }
                 }
             }
        }


        private void SetConditionExecute()
        {
            WriteToLog.write("SetCondition - starDate=" + GameBoard.CurrentStarDate() + "  energy=" + this.energyLevel);

            // set the alert level
            if (GameBoard.GetGameBoard() % 100 / 10 > 0)
            {
                this.alertLevel = REDALERT;
            }
            else
            {
                //energy <15% or or shields up is yellow alert
                if (this.energyLevel < 15 || Shields.AreUp())
                {
                    this.alertLevel = YELLOWALERT;
                }
                else
                {
                    this.alertLevel = GREENALERT;
                }
            }

            Debug("Alert Level = " + alertLevel);

            // update the form
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                Debug("Updating Status");

                switch (this.alertLevel)
                {
                    case REDALERT:
                        lblAlert.Content = "RED";
                        break;
                    case YELLOWALERT:
                        this.lblAlert.Content = "YELLOW";
                        break;
                    default:
                        this.lblAlert.Content = "GREEN";
                        break;
                }

                // update labels - the updatecontent method sends
                // the info to the log file when in debug mode
                UpdateContent(lblStarDate, String.Format("{0:0.0} ({1:0.0})", this.GameBoard.CurrentStarDate(), GameBoard.TimeLeft()));
                UpdateContent(lblEnergy, String.Format("{0:0.0}%", this.energyLevel));
                UpdateContent(lblKlingons, GameBoard.getKlingonCount().ToString());

                lblImpulse.Content = Impulse.HealthPercent() + "%";
                lblWarp.Content = Warp.HealthPercent() + "%";
                lblPhasers.Content = Phasers.HealthPercent() + "%";
                lblTorpedoes.Content = Torpedoes.HealthPercent() + "%";
                lblTorpedoCount.Content = "(" + Torpedoes.getCurrentCount() + " Remaining)";
                lblShields.Content = Shields.HealthPercent() + "%";
                lblLRS.Content = LRS.HealthPercent() + "%";
                lblShieldPowerRemaining.Content = Shields.levelPercent() + "%";

                string[] loc = GameBoard.GetLocationInfo();
                lblQuadrant.Content = loc[0];
                lblSector.Content = loc[1];
            }));


            // set button colors 
            Debug("Set buttons");
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                // update button background
                btnImpulse.Background = ((Impulse.IsHealthy() ? LTGREEN : REDISH));
                btnWarp.Background = ((Warp.IsHealthy() ? LTGREEN : REDISH));
                btnTorpedoes.Background = ((Torpedoes.IsHealthy() ? (Torpedoes.getCurrentCount() > 2 ? LTGREEN : LTYELLOW) : REDISH));
                btnPhasers.Background = ((Phasers.IsHealthy() ? LTGREEN : REDISH));
                btnLRS.Background = ((LRS.IsHealthy() ? GREENISH : REDISH));
                btnRepairs.Background = (DamageControl.HealthPercent() > 50 ? GREENISH : (DamageControl.IsHealthy() ? LTYELLOW : REDISH));
                btnDivert.Background = (Shields.levelPercent() > 80 ? GREENISH : (Shields.levelPercent() < 40 ? REDISH : LTYELLOW));

                btnDivert.IsEnabled = !IsDocked();
                btnShields.IsEnabled = !IsDocked();

                if (this.alertLevel == REDALERT)
                {
                    if (Shields.HealthPercent() > 60)
                        btnShields.Background = (Shields.AreUp() ? GREENISH : REDISH);
                    else
                        btnShields.Background = ((Shields.IsHealthy() && Shields.AreUp() ? LTYELLOW : REDISH));
                }
                else
                {
                    if (Shields.HealthPercent() > 30)
                        btnShields.Background = (Shields.AreUp() ? GREENISH : LTGREEN);
                    else
                        btnShields.Background = ((Shields.IsHealthy() ? (Shields.AreUp() ? LTGREEN : LTYELLOW) : REDISH));
                }
            }));


            // are we docked?
            Debug("Docked");
            SetDocked(SRS.AreWeDocked());

            // look for starbase updates
            Debug("Starbases");
            StarBases.Execute();

            // execute damage control and LRS updates
            Debug("DC");
            DamageControl.Execute();

            // update the Long range sensors
            Debug("LRS");
            LRS.Execute();

            Debug("Exiting setcondition");
        }


        /*
         * Check for end of game conditions
         * and make sure we only process one
         * end game condition
         * 
         */
        private void endGameCheck()
        {
            Debug("Endgame check");
            int endGameAction = 0;

            if (GameBoard.getKlingonCount() < 1)
            {
                Debug("YOU WON!");
                endGameAction = (GameEnd.ForTheWin() ? 1 : 2);
            }
            else if (GameBoard.TimeLeft() <= 0.0)
            {
                Debug("Out of Time");
                endGameAction = (GameEnd.OutOfTime() ? 1 : 2);
            }
            else if (this.energyLevel < .1)
            {
                Debug("Out of Energy");
                endGameAction = (GameEnd.OutOfEnergy() ? 1 : 2);
            }
            else if (this.alertLevel == REDALERT &&
                (!(Impulse.IsHealthy() || Warp.IsHealthy() || Phasers.IsHealthy()
                || (Torpedoes.IsHealthy() && Torpedoes.getCurrentCount() > 0))))
            {
                Debug("No longer able to fight");
                endGameAction = (GameEnd.Destroyed() ? 1 : 2);
            }

            // take end game action if > 0
            switch (endGameAction)
            {
                case 1:
                    Debug("Starting new game");
                    // TODO - add 16 grid support.  If I decide to add multiplayer
                    // player support (not likely) then I'll also add 24 & 32 grid sizes
                    NewGame(8);
                    break;

                case 2:
                    Debug("Normal exit to game");
                    System.Windows.Forms.Application.ExitThread();
                    this.Close();
                    break;
            }

        }


        /*
         * Simple debug output for a label control
         * prints out the label name and value
         * to the application debug file
         * 
         */
        private void UpdateContent(Label lbl, string txt)
        {
            Debug(lbl.Name + "=" + txt);
            lbl.Content = txt;
        }


        /*
         * Set the docked status of the ship
         */
        public void SetDocked(bool docked)
        {
            
            if (!this.docked && docked)
            {
                this.energyLevel = 100;
                ComsChatter("Commander Scot reports reports, 'Docking is complete, Captain.'");
            }

            if(this.docked && !docked)
            {
                ComsChatter("Lt Sulu reports, 'We're clear for maneuvering, Captain.'");
            }

            DamageControl.Docked(docked);
            Impulse.Docked(docked);
            LRS.Docked(docked);
            Phasers.Docked(docked);
            Probes.Docked(docked);
            Shields.Docked(docked);
            SRS.Docked(docked);
            Torpedoes.Docked(docked);
            Warp.Docked(docked);

            this.docked = docked;
        }


        /*
         * Getter for docked status
         */
        public bool IsDocked()
        {
            return docked;
        }


        /*
         * Setter for the ship's energy level
         */
        public void AdjustEnergy(double e)
        {
            energyLevel =(energyLevel+e>100?100:energyLevel+e);
        }


        /*
         * Getter for ship's energy level
         */
        public double GetEnergyLevel()
        {
            return energyLevel;
        }


        /*
         *  Button event handler
         *  
         *  We could simply disable/enable the buttons and menus during
         *  timer execution, but that would cause serious flashing and
         *  further delays to play.  The current soultion is to simply
         *  ignore button/menu selection during animation sequences.
         */
        void ButtonControl(object sender, RoutedEventArgs e)
        {
            // prevent button press during timer execution
            if (timerStep==0)
            {
                String btn = (sender as Button).Content.ToString();
                ButtonCommand(btn);
            }
        }



        /*
         * Button execution was broken out of the button handler so the
         * short cut keys can access it in a straight forward way
         */
        private void ButtonCommand(string btn) {
            bool executed = false;

            // prevent button hot keys during timer execution
            if (timerStep == 0)
            {
                Debug(btn + " pressed");

                switch (btn.ToUpper())
                {
                    case "IMPL":
                        executed = Impulse.Execute();
                        break;

                    case "WARP":
                        executed = Warp.Execute();

                        if (executed)
                            SRS.Execute();
                        break;

                    case "SHLD":
                        Shields.Execute();
                        timerStep = 4;
                        timer.Enabled = true;
                        //SetCondition();
                        break;

                    case "POWER":
                        Shields.divertPower();
                        timerStep = 4;
                        timer.Enabled = true;
                        //SetCondition();
                        break;

                    case "TORP":
                        executed = Torpedoes.Execute();
                        break;

                    case "PHAS":
                        executed = Phasers.Execute();
                        break;

                    case "DMG":
                        DamageControl.FixAllDamage();
                        timerStep = 4;
                        timer.Enabled = true;
                        //SetCondition();
                        break;
                }

                if (executed)
                {
                    // update status before animation starts
                    SetCondition();

                    if (IsUsingAnimation())
                    {
                        timerStep = 1;
                        timer.Enabled = true;
                    }
                    else
                    {
                        Debug("No Animation Executing");
                        GameObjects.MoveWithNoAnimation();
                        GameObjects.TakingFire();
                        GameObjects.MoveWithNoAnimation();
                    }
                }
            }
        }


        /*
         * Menu event handler
         */
        void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // prevent menu selection during timer execution
            if (timerStep == 0)
            {
                // can't let them in while the timer is busy
                MenuItem item = sender as MenuItem;
                MenuControl(item.Header.ToString());
            }
        }

        void MenuControl(string item)
        {
            string info;

            switch (item.ToLower().Substring(0, 3))
            {
                case "tar": // calculate torpedo course
                    info = Course.Calculation("Torpedo Course Calculation",10, SRS.GetMyRow(), SRS.GetMyCol());
                    if (info != null) ComsChatter("Computer returned " + info);
                    break;

                case "war": // calculate warp course
                    info = Course.Calculation("Ship Course Calculation", GameBoard.GetSize(), GameBoard.getMyLocation() / GameBoard.GetSize(), GameBoard.getMyLocation() % GameBoard.GetSize());
                    if (info != null) ComsChatter("Computer returned " + info);
                    break;

                case "lau": // launch probe to sector
                    Probes.Execute();
                    break;

                case "new": // new game
                    if (Dialogs.YesNoDialog("New Game", "Do you wish to start a new game?") == System.Windows.Forms.DialogResult.Yes)
                    { 
                        NewGame(8);
                    }
                    break;

                case "qui": // quit
                    if (Dialogs.YesNoDialog("Exit Game", "Do you wish to quit?") == System.Windows.Forms.DialogResult.Yes)
                    {
                        Debug("User exited through menu - normal exit");
                        System.Windows.Forms.Application.ExitThread();
                        this.Close();
                    }
                    break;

                case "ins": // instructions
                    info = "Instructions.txt was not found";

                    try
                    {
                        info = File.ReadAllText(@"Instructions.html");
                        info = info.Replace("{K}", GameBoard.getKlingonCount().ToString());
                    }
                    catch (Exception)
                    {
                        // don't do anything
                    }

                    Dialogs.OKDialog("Instructions", info);
                    break;

                case "abo": // about
                    info = "<html><H1><i>STAR TREK</i></H1><br>"
                        + "Released freely to the public domain.<br><br>"
                        + "A modern version styled after the 1970's console based game "
                        + "developed by Mike Mayfield and recreated, largely from memory, by Jon Walker.<br><br>"
                        + "The purpose of this game is to help others learn basic C# coding by example. "
                        + "All of the concepts here are very straight forward and will be building blocks "
                        + "for many WPF projects.<br><br>"
                        + "Written using Visual Studio Community 2019<br><html>";

                    Dialogs.OKDialog("About",info);
                    break;

                case "cre": // credits
                    info = "<html><H1>References</H1>"
                        + "Wikipedia<br>&nbsp;&nbsp;&nbsp;&nbsp;<u>en.wikipedia.org/wiki/Star_Trek_(1971_video_game)</u><br><br>"
                        + "Bob's Games<br>&nbsp;&nbsp;&nbsp;&nbsp;<u>www.bobsoremweb.com/startrek.html</u><br><br>"
                        + "Tom Almy's Super StarTrek<br>&nbsp;&nbsp;&nbsp;&nbsp;<u>www.almy.us/sst.html</u><br><br><br>"
                        + "StackOverflow<br>&nbsp;&nbsp;&nbsp;&nbsp;<u>www.stackoverflow.com</u></html>";

                    Dialogs.OKDialog("About", info);
                    break;

            }
        }


        /*
         * Clear the coms text area for a new game. 
         */
        public void ComsClear()
        {
            this.Dispatcher.Invoke(() =>
            {
                txtComs.Text = "Ship shows green across the board, Captain!";
                WriteToLog.write("ComsClear");
            });
        }


        /*
         * Display information to the coms textbox making sure to 
         * advance to the bottom of the text in the box
         */
        public void ComsChatter(string txt)
        {
            this.Dispatcher.Invoke(() =>
            {
                txtComs.Text += "\r\n" + txt;
                WriteToLog.write("ComsChatter ==> "+txt);

                txtComs.SelectionStart = txtComs.Text.Length;
                txtComs.ScrollToEnd();
            });
        }


        /*
         * I wanted to add some animation to the game for firing of a torpedo 
         * or moving the Enterprise.   I coded the creation and execution of a list 
         * of animation commands in the Java version first.  It was incredibly easy
         * and works well.
         * 
         * However, I've always used encapsulated threads in my C# experience.  
         * Essentially the threads I used were seperae proceses that were 
         * kicked off by the main thread and didn't have reason to interact directly
         * back with the main thread until completed.  In the Java version, there was
         * almost no issues to get it to work as Java seems to have a lot of thread 
         * safety built into the system.
         * 
         * In C#, having one that interacts with the main thread was a whole different 
         * level of attention and we need to make sure we don't have a conflicts with 
         * the main thread.   Luckily, the VS 2019 IDE helps you catch problems early on.
         * 
         * The big thing is to identify where the locations are that have potential for 
         * conflict and lock those routines as appropriate.  Some of them could require 
         * more than a simple lock, like the WriteToLog.  On a slower processor, you
         * can lose information in Writelog because the lock just skips the process. Thus
         * I was forced to use a lock manually with a try/catch.
         * 
         * Another issue is form screen updates.  You have to use the form's 
         * Dispatcher.BeginInvoke (the Dispatcher.Invoke doesn't seem to work as well)
         * to make sure there are no conflicts between the main and timer thread during
         * form updates.  This process seems to work flawlessly and made my life a lot
         * easier.  I'm going to be experimenting more with having threads access the
         * main and each other in upcoming projects and I'll save some of what I discover
         * to my public Git repository.
         * 
         */
        public void CreateTimer()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(TimerTask);
            timer.Interval = 150;
            timer.Start();
            Debug("Timer created");
        }


        /*
         * Timer task that is called per interval.
         * Slower processors will have problems getting the
         * current step done within the interval, so disable
         * the timer as soon as you enter to create a
         * protection to prevent overrun.
         * 
         */
        private void TimerTask(object source, ElapsedEventArgs args)
        {
            timer.Enabled = false;

            tickTock++; // allows me to keep track of which iteration
            Debug("Entering Timer - " + tickTock);

            //if (! inTimer)
            //{
            //    int thisTimer = tickTock;
            //    inTimer = true;

                // Steps of Animation processing
                // 1 - Animate Enterprise movement until complete
                // 2 - Calculate hits from enemy
                // 3 - Animate hits from enemy until complete
                // 4 - Animation done, update current conditions, check for end of game
                switch (timerStep)
                {
                    // -------------------------------------------------
                    // Enterprise movement animation
                    // -------------------------------------------------
                    case 1:
                        Debug("Enterprise Move - " + tickTock);
                        if (GameObjects.ShowObjectMovement() == null) timerStep++;
                        break;

                    // -------------------------------------------------
                    // Calculate hits from enemy
                    // -------------------------------------------------
                    case 2:
                        Debug("Post Enterprise Move - Taking Fire" + tickTock);
                        GameObjects.TakingFire();
                        timerStep++;
                        break;

                    // -------------------------------------------------
                    // Show enemy movement
                    // -------------------------------------------------
                    case 3:
                        Debug("Enemy Move " + tickTock);
                        if (GameObjects.ShowObjectMovement() == null) timerStep++;
                        break;

                    // -------------------------------------------------
                    // break us out, we're done
                    // -------------------------------------------------
                    case 4:
                        Debug("Animation complete " + tickTock);
                        timerStep++;
                        SetCondition();
                        break;

                    // -------------------------------------------------
                    // should never get here
                    // -------------------------------------------------
                    default:
                        Debug("Unknown timer step "+timerStep+" in timer " + tickTock);
                        timerStep = 0;
                        break;
                }

                Debug("Timer done " + tickTock + " step=" + timerStep);

                // When done, disable the timer until we need it again
                // otherwise things can get hinky with timing
                timer.Enabled = (timerStep > 0 && timerStep < 5);
                timerStep = (timerStep > 4 ? 0 : timerStep);

            //    inTimer = false;
            //}
            //else
            //{
                // Slower processors could end up here
            //    Debug((inTimer?tickTock.ToString()+" IN TIMER":"Exiting Timer "+tickTock.ToString()));
            //}
        }


        /*
         * Report on whether we're using animation support.  I added
         * this when I first stated adding animation support so that
         * I could switch back and forth.
         * 
         */
        public bool IsUsingAnimation()
        {
            return USE_ANIMATION;
        }


        /*
         * Very simple debug routine.  Just print out to the log
         * if something is sent and return debug mode setting.
         * 
         */
        public bool Debug(string txt)
        {
            if(debug)
            {
                if (txt.Length > 0)
                {
                    WriteToLog.write(txt);
                }
            }

            return debug;
        }


        /*
         * Tie in the hot keys to the button commands
         */
        private void ImpulseCommand(object sender, ExecutedRoutedEventArgs e) { ButtonCommand("IMPL"); }
        private void WarpCommand(object sender, ExecutedRoutedEventArgs e) { ButtonCommand("WARP"); }
        private void TorpedoCommand(object sender, ExecutedRoutedEventArgs e) { ButtonCommand("TORP"); }
        private void PhaserCommand(object sender, ExecutedRoutedEventArgs e) { ButtonCommand("PHAS"); }
        private void ShieldCommand(object sender, ExecutedRoutedEventArgs e) { ButtonCommand("SHLD"); }
    }
}
