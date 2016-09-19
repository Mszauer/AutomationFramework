//#define debugColors
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
        public Dictionary<string, List<string>> currentQuestion;

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
            currentQuestion = new Dictionary<string, List<string>>();

            browser = new WebBrowser();
            browser.Dock = DockStyle.Fill;
            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnWebpageLoad);

            string targetUrl = "google.com";

            if (System.IO.File.Exists("C:/default.txt")) {
                targetUrl = System.IO.File.ReadAllText("C:/default.txt");
            }
            browser.Navigate(targetUrl);

            mainWindowSplitter = new SplitContainer();
            mainWindowSplitter.IsSplitterFixed = true;
            mainWindowSplitter.Dock = DockStyle.Fill;

            BrowserLayoutSplit(targetUrl);

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
            tabs.SelectedTab.Text = "Browser";


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
            currentPages = new ListBox();
            currentPages.Dock = DockStyle.Fill;
            currentPages.SelectionMode = SelectionMode.One;
            splitListBox.Panel1.Controls.Add(currentPages);
            
            #endregion
            #region Sourcecode View Button
            Button sourceCodeViewButton = new Button();
            sourceCodeViewButton.Text = "View Source Code";
            sourceCodeViewButton.Dock = DockStyle.Fill;
            sourceCodeViewButton.BackColor = Color.LightGreen;
            sourceCodeViewButton.Click += (object sender, EventArgs e) => {
                if (currentPages.SelectedIndex >= 0) {
                    AddTab(sender, e);
                    List<HtmlWindow> currentFrames = RefreshFrames();
                    tabs.SelectedTab.Text = currentFrames[currentPages.SelectedIndex].Url.ToString();
                    AddTextfield(tabs.SelectedTab,currentFrames);
                }
            };

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
            currentPages.Items.Clear();
            List<HtmlWindow> frames = RefreshFrames();
            foreach(HtmlWindow window in frames) {
                try {
                    currentPages.Items.Add(window.Document.Url);
                }
                catch (System.Exception e){
                    
                }
            }
        }
        protected void SwitchTabs(object sender, EventArgs e) {
            //switches depending on selected index of currentPages
            tabs.SelectedIndex = currentPages.SelectedIndex;
        }
        protected void AddTab(object sender, EventArgs e) {
            TabPage defaultTab = new TabPage();
            tabs.TabPages.Add(defaultTab);
            tabs.SelectTab(tabs.TabPages.Count - 1);
        }
        protected void AddTextfield(TabPage tab,List<HtmlWindow> currentFrames) {
            TextBox content = new TextBox();
            content.Multiline = true;
            content.Dock = DockStyle.Fill;
            content.WordWrap = true;
            content.ScrollBars = ScrollBars.Vertical;
            content.AcceptsReturn = true;
            content.AcceptsTab = true;

            string webData = currentFrames[currentPages.SelectedIndex].Document.Body.OuterHtml;
            webData = webData.Replace("\n", "\r\n");

            content.Text = webData;
            tab.Controls.Add(content);
        }
        protected void RemoveTab(object sender, EventArgs e) {
            if (!(tabs.TabPages.Count == 1)){
                tabs.TabPages.Remove(tabs.SelectedTab);
                tabs.SelectTab(tabs.TabPages.Count - 1);
            }
        }
        void BrowserLayoutSplit(string currentPageURL) {
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

            CreateTextbox(urlSplitter,currentPageURL);
        }
        string QuestionExtractor() {
            string question = "";
            List<HtmlWindow> doc = RefreshFrames();
            HtmlElement questionElement;

            HtmlElementCollection htmlElements = doc[2].Document.GetElementsByTagName("td");
            foreach (HtmlElement element in htmlElements) {
                try {
                    string questionClass = "assessment_question_text";
                    string className = element.GetAttribute("className");
                    if (className == questionClass) {
                        questionElement = element;

                        DialogResult userClickedOK = MessageBox.Show("The questions is: \n" + element.InnerText);
                        if (userClickedOK == DialogResult.OK) {
                            AnswerExtractor();
                        }

                        question = element.InnerText;

                        System.Windows.Forms.Clipboard.SetText(element.InnerText);

                        return question;
                    }
                }
                catch (System.Exception e) {

                }
            }
            return question;
        }
        List<string> AnswerExtractor() {
            List<string> answers = new List<string>();
            List<HtmlWindow> doc = RefreshFrames();

            HtmlElementCollection htmlElements = doc[2].Document.GetElementsByTagName("td");
            foreach (HtmlElement element in htmlElements) {
                try {
                    string questionClass = "assessment_multiple_choice";
                    string className = element.GetAttribute("className"); //this is always null for some reason
                    if (className == questionClass) {
                        answers.Add(element.InnerText);
                    }
                }
                catch (System.Exception e) {

                }
            }
            #region Popup
            string result = "The answers are:";
            foreach(string answer in answers) {
                result = result.Insert(result.Length-1,"\n" + answer);
            }

            MessageBox.Show(result);
            #endregion
            return answers;
        }
        void CreateTextbox(SplitContainer rootSplitter,string currentPageURL) {
            rootSplitter.SplitterDistance = (int)(3.5f * (rootSplitter.Size.Width / 4));

#region URLFIELD
            TextBox urlText = new TextBox();
            urlText.Dock = DockStyle.Fill;
            urlText.Multiline = false;
            urlText.Text = currentPageURL;
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
                string textboxURL = urlText.Text; //retrieve textbox text
                if (!string.IsNullOrEmpty(textboxURL)) {
                    browser.Navigate(new Uri(textboxURL));//Load website from url provided
                }
            };
            goDownloadSplit.Panel1.Controls.Add(urlGoTo);

            Button download = new Button();
            download.Dock = DockStyle.Fill;
            download.BackColor = Color.CadetBlue;
            download.Text = "Get Body";
            download.Click +=  (Object sender, EventArgs e) =>
            {
                QuestionExtractor();
            };
            goDownloadSplit.Panel2.Controls.Add(download);
#endregion
        }
        void OnWebpageLoad(object sender, WebBrowserDocumentCompletedEventArgs e) {
            /*browser.Document.GetElementById("lst-ib").SetAttribute("value", "Superman");//insert text for search boxes

            //browser.Document.GetElementById("go").InvokeMember("click"); // tried to load a new web page and it did not have a search button. NRE
            foreach (HtmlElement el in browser.Document.GetElementsByTagName("input")) {
                if (el.Name == "btnK") {
                    el.InvokeMember("sf.chk");
                    el.InvokeMember("onclick");

                }

            }*/
            RefreshListBox();
            
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
        List<HtmlWindow> RefreshFrames() {
            List<HtmlWindow> currentWindows = GetIFrames(browser.Document.Window);

            return currentWindows;
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