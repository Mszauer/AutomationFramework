using System;
using System.Windows.Forms;
using System.Drawing;

namespace Game {
    public class Program : Form {
        public SplitContainer mainWindowSplitter;
        public WebBrowser browser;

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public Program() {
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();

            this.Name = "EmbeddedBrowser";
            this.Text = "EmbeddedBrowser";
            this.Load += new EventHandler(OnLoad);
            this.ResumeLayout(false);
        }

        void OnLoad(object sender, EventArgs a) {
            browser = new WebBrowser();
            this.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
            browser.Navigate("Google.com");

            mainWindowSplitter = new SplitContainer();
            mainWindowSplitter.IsSplitterFixed = true;
            BrowserLayoutSplit();

            this.Controls.Add(mainWindowSplitter);
            mainWindowSplitter.Panel2.Controls.Add(browser);

            browser.Dock = DockStyle.Fill;
            mainWindowSplitter.Dock = DockStyle.Fill;
        }

        void BrowserLayoutSplit() {
            mainWindowSplitter.Name = "Main Window";

            mainWindowSplitter.IsSplitterFixed = true;
            mainWindowSplitter.Orientation = Orientation.Horizontal;

            mainWindowSplitter.Panel1MinSize = 1;//Set minimum top size
            mainWindowSplitter.SplitterDistance = 30;//mimicks url browser size
            mainWindowSplitter.FixedPanel = FixedPanel.Panel1;

            SplitContainer urlSplitter = new SplitContainer();
            urlSplitter.Name = "URL Splitter";
            mainWindowSplitter.Panel1.Controls.Add(urlSplitter);
            urlSplitter.Dock = DockStyle.Fill;
            urlSplitter.IsSplitterFixed = true;

            CreateTextbox(urlSplitter);
        }

        void CreateTextbox(SplitContainer urlSplit) {
            string textboxURL;
            urlSplit.SplitterDistance = (int)(3.5f * (urlSplit.Size.Width / 4));

            #region URLFIELD
            TextBox urlText = new TextBox();
            urlText.Dock = DockStyle.Fill;
            urlText.Multiline = false;
            urlSplit.Panel1.Controls.Add(urlText);
            #endregion

            #region GOBUTTONFIELD
            Button urlGoTo = new Button();
            urlGoTo.Dock = DockStyle.Fill;
            urlGoTo.BackColor = Color.Green;
            urlGoTo.Text = "Go!";
            urlGoTo.Click += delegate (Object sender, EventArgs e)//setup button click function
            {
                textboxURL = urlText.Text; //retrieve textbox text
                browser.Navigate(new Uri(textboxURL));//Load website from url provided
            };

            urlSplit.Panel2.Controls.Add(urlGoTo);
            #endregion
        }

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }
    }
}