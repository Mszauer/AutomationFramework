﻿//#define debugColors
//#define debugSplitter
using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Collections;
using System.Collections.Generic;

namespace Game {
    public class Program : Form {
        public SplitContainer mainWindowSplitter;
        public WebBrowser browser;
        public ListBox currentPages;
        public TabControl tabs;
        

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
            browser.Dock = DockStyle.Fill;
            //browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DownloadHTML);
            browser.Navigate("google.com");

            mainWindowSplitter = new SplitContainer();
            mainWindowSplitter.IsSplitterFixed = true;
            mainWindowSplitter.Dock = DockStyle.Fill;

            BrowserLayoutSplit();

            this.Controls.Add(mainWindowSplitter);

            mainWindowSplitter.Dock = DockStyle.Fill;

            SplitContainer bodySplitter = new SplitContainer();
            mainWindowSplitter.Panel2.Controls.Add(bodySplitter);
            bodySplitter.Dock = DockStyle.Fill;
            bodySplitter.IsSplitterFixed = true;
            bodySplitter.SplitterDistance = 30;
            //bodySplitter.Panel2.Controls.Add(browser);

            currentPages = new ListBox();
            CreateListBox(bodySplitter);
            //RefreshListBox(); //works but doesn't display items

            tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            bodySplitter.Panel2.Controls.Add(tabs);

            AddTab(this, new EventArgs());
            tabs.SelectedTab.Controls.Add(browser);

            #region DebugColors
#if debugColors
            mainWindowSplitter.Panel1.BackColor = Color.Yellow;
            mainWindowSplitter.Panel2.BackColor = Color.Green;
            bodySplitter.Panel1.BackColor = Color.Red;
            bodySplitter.Panel2.BackColor = Color.Blue;
#endif
            #endregion


        }
        void CreateListBox(SplitContainer rootSplitter) {
            SplitContainer splitListBox = ListboxSeperate();


            rootSplitter.Panel1.Controls.Add(splitListBox);
            #region Listbox
            ListBox list = new ListBox();
            list.Dock = DockStyle.Fill;
            list.SelectionMode = SelectionMode.One; splitListBox.Panel1.Controls.Add(list);
            #endregion
            #region Sourcecode View Button
            Button sourceCodeViewButton = new Button();
            sourceCodeViewButton.Text = "View Source Code";
            sourceCodeViewButton.Dock = DockStyle.Fill;
            sourceCodeViewButton.BackColor = Color.LightGreen;
            splitListBox.Panel2.Controls.Add(sourceCodeViewButton);
            #endregion
            #region SplitterDistanceDebug
#if debugSplitter
            splitListBox.SplitterMoved += delegate {
                sourceCodeViewButton.Text = ("Splitter Distance: " + splitListBox.SplitterDistance.ToString());
            };
#endif
            #endregion
        }
        protected void RefreshListBox() {
            /*foreach(HtmlWindow window in RefreshFrames()) {
                currentPages.Items.Add(window);
            }*/
            foreach(String test in RefreshFrames()) {
                currentPages.Items.Add(test);
                //currentPages.Controls.Add(test);
            }
        }
        protected void SwitchTabs(object sender, EventArgs e) {
            //switches depending on selected index of currentPages
            //tabs.SelectedTab = currentPages.SelectedIndex ; //change to match with index
        }
        protected void AddTab(object sender, EventArgs e) {
            TabPage defaultTab = new TabPage();
            defaultTab.Name = browser.DocumentTitle;
            tabs.TabPages.Add(defaultTab);
            tabs.SelectTab(tabs.TabPages.Count - 1);
        }
        protected void RemoveTab(object sender, EventArgs e) {
            if (!(tabs.TabPages.Count == 1)){
                tabs.TabPages.Remove(tabs.SelectedTab);
                tabs.SelectTab(tabs.TabPages.Count - 1);
            }
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
        void CreateTextbox(SplitContainer rootSplitter) {
            string textboxURL;
            rootSplitter.SplitterDistance = (int)(3.5f * (rootSplitter.Size.Width / 4));

#region URLFIELD
            TextBox urlText = new TextBox();
            urlText.Dock = DockStyle.Fill;
            urlText.Multiline = false;
            rootSplitter.Panel1.Controls.Add(urlText);
#endregion

#region GO/DOWNLOAD BUTTONS
            SplitContainer goDownloadSplit = new SplitContainer();
            goDownloadSplit.SplitterDistance = rootSplitter.Width / 2;
            goDownloadSplit.IsSplitterFixed = true;
            goDownloadSplit.Dock = DockStyle.Fill;

            rootSplitter.Panel2.Controls.Add(goDownloadSplit);



            Button urlGoTo = new Button();
            urlGoTo.Dock = DockStyle.Fill;
            urlGoTo.BackColor = Color.LightSeaGreen;
            urlGoTo.Text = "Go!";
            urlGoTo.Click += delegate (Object sender, EventArgs e)//setup button click function
            {
                textboxURL = urlText.Text; //retrieve textbox text
                if (!string.IsNullOrEmpty(textboxURL)) {
                    browser.Navigate(new Uri(textboxURL));//Load website from url provided
                }
            };
            goDownloadSplit.Panel1.Controls.Add(urlGoTo);

            Button download = new Button();
            download.Dock = DockStyle.Fill;
            download.BackColor = Color.CadetBlue;
            download.Text = "Get Body";
            download.Click += delegate (Object sender, EventArgs e)//setup button click function
            {
                GetHTMLBody();
            };
            goDownloadSplit.Panel2.Controls.Add(download);
#endregion
        }
        void OnWebpageLoad(object sender, WebBrowserDocumentCompletedEventArgs e) {
            browser.Document.GetElementById("lst-ib").SetAttribute("value", "Superman");//insert text for search boxes

            //browser.Document.GetElementById("go").InvokeMember("click"); // tried to load a new web page and it did not have a search button. NRE
            foreach (HtmlElement el in browser.Document.GetElementsByTagName("input")) {
                if (el.Name == "btnK") {
                    el.InvokeMember("sf.chk");
                    el.InvokeMember("onclick");

                }

            }
            
        }
        void GetHTMLBody() {
            string webData = browser.Document.Body.OuterHtml;
            SaveFileDialog save = new SaveFileDialog();

            // Set filter options and filter index.
            save.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            save.FilterIndex = 1;

            // Call the ShowDialog method to show the dialog box.
            DialogResult userClickedOK = save.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == DialogResult.OK) {
                // Open the selected file to read.
                System.IO.Stream fileStream = save.OpenFile();

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileStream)) {
                    writer.WriteLine(webData);
                }

            }
        }
        SplitContainer ListboxSeperate() {
            SplitContainer listboxSplitter = new SplitContainer();
            listboxSplitter.Dock = DockStyle.Fill;
            listboxSplitter.IsSplitterFixed = true;
            listboxSplitter.Panel2MinSize = 5;
            listboxSplitter.SplitterDistance =  725;
            listboxSplitter.Orientation = Orientation.Horizontal;
#region DebugColors
#if debugColors
            listboxSplitter.Panel1.BackColor = Color.Coral;
            listboxSplitter.Panel2.BackColor = Color.Chocolate;
#endif
#endregion
            return listboxSplitter;
        }
        /*List<HtmlWindow> RefreshFrames() {
            List<HtmlWindow> currentWindows = GetIFrames(browser.Document.Window);

            return currentWindows;
        } //this gets stuck on infinte loop*/
        String[] RefreshFrames() {
            string[] tester = new string[3] { "one", "two", "3" };
            return tester;
        }
        protected List<HtmlWindow> GetIFrames(HtmlWindow rootWindow) {
            List<HtmlWindow> result = new List<HtmlWindow>();

            result.Add(rootWindow);
            foreach(HtmlWindow child in rootWindow.Frames) {
                result.AddRange(GetIFrames(child));//recursively adds all child's windows
            }

            return result;
        }

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }
    }
}