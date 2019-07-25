using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

/*
 * Basic dialogs for user interaction
 * 
 */
namespace WPFTrek.Utilities
{
    static class Dialogs
    {
        /*
         * Simple input dialog that allows for multi-line prompts.  If we allow the text
         * to wrap by itself, the text will overwrite the controls.
         * 
         * TODO - fix the height calculation issue for word wrapping
         * 
         */
        public static string BasicInputDialog(String title, String prompt)
        {
            string result = null;

            Form ibForm = new Form();
            Label ibLabel = new Label();
            TextBox ibTextBox = new TextBox();
            Button ibOK = new Button();
            Button ibCancel = new Button();

            ibForm.Text = title;
            ibLabel.Text = prompt;

            ibOK.Text = "OK";
            ibCancel.Text = "Cancel";
            ibOK.DialogResult = DialogResult.OK;
            ibCancel.DialogResult = DialogResult.Cancel;

            ibLabel.SetBounds(9, 20, 372, 13);

            ibLabel.AutoSize = true;
            ibTextBox.Anchor = ibTextBox.Anchor | AnchorStyles.Right;

            int h = ibLabel.Height * ibLabel.Text.Count(x => x == '\n');

            ibTextBox.SetBounds(12, 36+h, 372, 20);
            ibOK.SetBounds(228, 72+h, 75, 23);
            ibCancel.SetBounds(309, 72+h, 75, 23);
            ibOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ibCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ibForm.ClientSize = new System.Drawing.Size(396, 107+h);
            ibForm.Controls.AddRange(new Control[] { ibLabel, ibTextBox, ibOK, ibCancel });
            ibForm.ClientSize = new Size(Math.Max(300, ibLabel.Right + 10), ibForm.ClientSize.Height);
            ibForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            ibForm.StartPosition = FormStartPosition.CenterParent;
            ibForm.MinimizeBox = false;
            ibForm.MaximizeBox = false;
            ibForm.AcceptButton = ibOK;
            ibForm.CancelButton = ibCancel;

            if(ibForm.ShowDialog()==DialogResult.OK)
            {
                // Cancel was not hit, so return the value of the textbox
                result = ibTextBox.Text.TrimEnd();
            }
            
            return result;
        }


        public static DialogResult YesNoDialog(String title, String prompt)
        {
            Form ibForm = new Form();
            Label ibLabel = new Label();
            Button ibOK = new Button();
            Button ibCancel = new Button();

            ibForm.Text = title;
            ibLabel.Text = prompt;

            ibOK.Text = "Yes";
            ibCancel.Text = "No";
            ibOK.DialogResult = DialogResult.Yes;
            ibCancel.DialogResult = DialogResult.No;

            ibLabel.SetBounds(9, 20, 372, 13);
            ibLabel.AutoSize = true;

            int h = ibLabel.Height * prompt.Count(x => x == '\n');

            ibOK.SetBounds(160, 55 + h, 75, 23);
            ibCancel.SetBounds(255, 55 + h, 75, 23);
            ibOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ibCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ibForm.ClientSize = new System.Drawing.Size(396, 90 + h);
            ibForm.Controls.AddRange(new Control[] { ibLabel, ibOK, ibCancel });
            ibForm.ClientSize = new Size(Math.Max(300, ibLabel.Right + 10), ibForm.ClientSize.Height);
            ibForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            ibForm.StartPosition = FormStartPosition.CenterParent;
            ibForm.MinimizeBox = false;
            ibForm.MaximizeBox = false;
            ibForm.AcceptButton = ibOK;
            ibForm.CancelButton = ibCancel;

            DialogResult response = ibForm.ShowDialog();
            return response;
        }


        public static DialogResult OKDialog(String title, String prompt)
        {
            Form ibForm = new Form();
            WebBrowser wbText = new WebBrowser();
            Button ibOK = new Button();

            ibForm.Text = title;
            wbText.DocumentText = prompt;

            ibOK.Text = "OK";
            ibOK.DialogResult = DialogResult.Yes;

            wbText.SetBounds(9, 20, 422, 335);
            ibOK.SetBounds(185, 365, 75, 23);
            ibOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ibForm.ClientSize = new System.Drawing.Size(446, 400);
            ibForm.Controls.AddRange(new Control[] { wbText, ibOK });
            ibForm.ClientSize = new Size(450,400);
            ibForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            ibForm.StartPosition = FormStartPosition.CenterParent;
            ibForm.MinimizeBox = false;
            ibForm.MaximizeBox = false;
            ibForm.AcceptButton = ibOK;

            DialogResult response = ibForm.ShowDialog();
            return response;
        }

        public static double GetValue(string title, string prompt, double min, double max)
        {
            double result = -1;
            string resultStr;
            string crew = "";

            while (true)
            {
                resultStr = BasicInputDialog(title, crew+prompt+ "\r\n\r\nEnter amount ("+min.ToString()+" - " + max.ToString() + ") ?");

                if (resultStr == null)
                    break;

                if (double.TryParse(resultStr, out result))
                {
                    if (result >=0 && result <= max)
                        break;
                }

                crew = "Mr. Spock raised an eyebrow and said 'Captain, are you well?'\n\n";
            }

            return result;
        }

        public static int DurationDialog(string title, int max)
        {
            int result = -1;
            string resultStr;
            string crew = "";

            while (true)
            {
                resultStr = BasicInputDialog(title, crew + "Duration (1-"+max.ToString()+") ?");

                if (resultStr == null)
                    break;

                if (int.TryParse(resultStr, out result))
                {
                    if (result >= 1 && result < 13)
                        break;
                }

                if (title.Equals("warp", StringComparison.OrdinalIgnoreCase))
                {
                    crew = "Mr. Sule said, 'Captain, I did not understand your command.'\n\n";
                }
                else
                {
                    crew = "Mr. Spock raised an eyebrow and said 'Captian, I did not understand your last command'\n\n";
                }
            }

            return result;
        }

        public static double CourseDialog(string title)
        {
            string crew = "";
            string resultStr;
            double test;
            Double resultVal = -1;

            String prompt = 
                " 8    1    2\n" +
                "   \\    |    /\n" +
                "     \\  |   /\n" +
                "7 ---+--- 3\n" +
                "    /   |  \\\n" +
                "   /    |   \\\n" +
                " 6    5    4\n\n" +
                "Course value to set (1-8.9)?";

            while (true)
            {
                resultStr = BasicInputDialog(title, crew+prompt);

                if (resultStr == null)
                    break;

                double.TryParse(resultStr, out test);

                if (!(double.IsNaN(test) || double.IsInfinity(test)))
                {
                    if (test >= 1 && test < 9)
                    {
                        resultVal = Math.Round(test, 1, MidpointRounding.AwayFromZero);
                        break;
                    }
                }

                if (title.Equals("warp", StringComparison.OrdinalIgnoreCase))
                {
                    crew = "Mr. Sulu said, 'Captain, I did not understand your command.'\n\n";
                }
                else
                {
                    crew = "Mr. Checkov said, 'Kiptan, I did not understand your command.'\n\n";
                }
            }

            return resultVal;
        }
        
    }
}
