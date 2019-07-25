using System;
using System.Windows.Forms;

/*
 * End of game conditions come here for the appropriate dialog
 * and choice on whether to play again
 * 
 */
namespace WPFTrek.Utilities
{
    static class GameEnd
    {
        /*
         * End of game dialog box
         * 
         */
        public static bool ForTheWin()
        {
            string dialog = "Congratulations on a job well done!\n\n"
                            + "You defeated the Klingon empire's plans and kept the\n'" 
                            + "Federation free for another day.  As a reward, you\n"
                            + "will be promoted to admiral and given a desk job at\n"
                            + "Starfleet headquarters!\n\n"
                            + "Do you wish to play again?";

            return Dialogs.YesNoDialog("You Won!",dialog) == DialogResult.Yes;
        }


        public static bool OutOfTime()
        {
            string dialog = "Sensing weakness, the Klingon empire has committed\n"
                            + "to all out war.  It's doubtful the Federation will\n"
                            + "be able to survive!\n\n"
                            + "Do you wish to play again?";

            return Dialogs.YesNoDialog("You Ran Out Of Time!", dialog) == DialogResult.Yes;
        }

        public static bool OutOfEnergy()
        {
            string dialog = "As the Enterprise slowly drifts through space, you get\n"
                            + "to think about how the Klingon empire will overrun\n"
                            + "the federation!\n\n"
                            + "Do you wish to play again?";

            return Dialogs.YesNoDialog("You Ran Out Of Energy!", dialog) == DialogResult.Yes;
        }

        public static bool Destroyed()
        {
            string dialog = "As you drift though space in an escape pod, you realize\n"
                            + "that you have now opened the Federation up to further\n"
                            + "attacks and eventually all out war.\n\n"
                            + "Do you wish to play again?";

            return Dialogs.YesNoDialog("The Enterprise was Destroyed!", dialog) == DialogResult.Yes;
        }
    }
}
