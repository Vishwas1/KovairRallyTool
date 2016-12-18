using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Rally.RestApi;
using Rally.RestApi.Response;
using System.Threading;


namespace KovairRallyWPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Request request = null;
        static RallyRestApi restApi = null;
        static string username = string.Empty;
        static string password = string.Empty;
        static string serverUrl = string.Empty;
        //List<Thread> threads = new List<Thread>();
        //Thread[] array = new Thread[5];
        int i=0;
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            Loaded += MainWindow_Loaded;
            //toggleExpanderCollasp
            //portExpander.Expanded += new RoutedEventHandler(toggleExpanderCollasp);
            //delivExpander.Expanded += new RoutedEventHandler(toggleExpanderCollasp);   
        }

        private void toggleExpanderCollasp(object sender, RoutedEventArgs args)
        {
            //Do something when the Expander control collapses
            var senderExpnd = ((System.Windows.Controls.HeaderedContentControl)(sender)).Header.ToString().Contains("Deliv") ? "DelivExpander": "PortExpander";
            switch (senderExpnd) {
                case "DelivExpander": if (portExpander != null) portExpander.IsExpanded = false;
                    break;
                case "PortExpander": if (delivExpander!=null) delivExpander.IsExpanded = false;
                    break;
            }
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            restApi = new RallyRestApi();
            if (File.Exists("RallyConfig.xml"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("RallyConfig.xml");
                XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/root");

                serverUrl = urlTxt.Text = nodeList[0].ChildNodes[0].InnerText;
                username = unameTxt.Text = nodeList[0].ChildNodes[1].InnerText;
                password = pwdTxt.Password = nodeList[0].ChildNodes[2].InnerText;
            }
            else
            {
                urlTxt.Text = "";
                unameTxt.Text = "";
                pwdTxt.Password = "";
            }
        }

        private void saveConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(urlTxt.Text) && !String.IsNullOrEmpty(unameTxt.Text) && !String.IsNullOrEmpty(pwdTxt.Password))
            {
                username = unameTxt.Text;
                password = pwdTxt.Password;
                serverUrl = urlTxt.Text;
                Thread thread = new Thread(() =>
                {
                    bool isConfigCorrect = false;
                    isConfigCorrect = restApi.Authenticate(username, password, serverUrl, proxy: null, allowSSO: false).Equals(RallyRestApi.AuthenticationResult.Authenticated);
                    Action action = () =>
                    {
                        if (isConfigCorrect)
                        {
                            if (File.Exists("RallyConfig.xml"))
                                File.Delete("RallyConfig.xml");

                            string rallyConfigXml = "<root><url>" + serverUrl + "</url><username>" + username + "</username><password>" + password + "</password></root>";
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(rallyConfigXml);
                            doc.Save("RallyConfig.xml");
                            mandFldLbl.Visibility = System.Windows.Visibility.Visible;
                            mandFldLbl.Content = "Configuration is successfully saved!";
                        }
                        else
                        {
                            mandFldLbl.Visibility = System.Windows.Visibility.Visible;
                            mandFldLbl.Content = "Invalid rally configuration!";
                        }
                        clrallBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        saveConfigBtn.Background = Brushes.Silver;
                        saveConfigBtn.Foreground = Brushes.Black;
                        saveConfigBtn.Content = "Save Configuration";

                    };
                    Dispatcher.BeginInvoke(action);

                });
                thread.Start();
                saveConfigBtn.Content = "Processing...";
                saveConfigBtn.Background = Brushes.Green;
                saveConfigBtn.Foreground = Brushes.White;


            }
            else
            {
                mandFldLbl.Visibility = System.Windows.Visibility.Visible;
                mandFldLbl.Content = "*All fields are mandatory.";
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("RallyConfig.xml"))
                File.Delete("RallyConfig.xml");

            urlTxt.Text = "";
            unameTxt.Text = "";
            pwdTxt.Password = "";
            mandFldLbl.Visibility = System.Windows.Visibility.Hidden;

        }

        #region All Artifacts counts

        //private void getallBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBoxResult result = MessageBox.Show("Fetching all artifacts count is a long running process. It may take some time. Do you wish to continue?", "KovairRally", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        //    switch (result)
        //    {
        //        case MessageBoxResult.OK:
        //            string[] artifacts = { "Tasks", "HierarchicalRequirement", "PortfolioItem/theme", "PortfolioItem/initiative", "PortfolioItem/feature" };
        //            Thread thread = null;
        //            int count = 0; string projectId = prjctidTxt.Text;
        //            if (DoConnection())
        //            {
        //                List<Thread> runningThreads = new List<Thread>();
        //                foreach (string artifact in artifacts)
        //                {
        //                    string artFct = artifact;
        //                    thread = new Thread(() =>
        //                       {
        //                           count = GetArtifactCount(projectId, artFct);
        //                           Action action = () =>
        //                           {
        //                               switch (artifact)
        //                               {
        //                                   case "Tasks": taskLbl.Content = count;
        //                                       break;
        //                                   case "HierarchicalRequirement": storyLbl.Content = count;
        //                                       break;
        //                                   case "PortfolioItem/theme": themeLbl.Content = count;
        //                                       break;
        //                                   case "PortfolioItem/initiative": initiativeLbl.Content = count;
        //                                       break;
        //                                   case "PortfolioItem/feature": featuresLbl.Content = count;
        //                                       break;
        //                               }
        //                               getallBtn.Content = "Get All Artifacts Count";
        //                               getallBtn.Background = Brushes.Red;
        //                               getallBtn.Foreground = Brushes.White;
        //                               getStoryBtn.IsEnabled = true;
        //                               getTaskBtn.IsEnabled = true;
        //                               getThemeBtn.IsEnabled = true;
        //                               getInitiativeBtn.IsEnabled = true;
        //                               getFeaturesBtn.IsEnabled = true;
        //                               clrallBtn.IsEnabled = true;

        //                           };
        //                           Dispatcher.BeginInvoke(action);
        //                       });
        //                    thread.Start();
        //                    runningThreads.Add(thread);
        //                    getallBtn.Content = "Processing...";
        //                    getallBtn.Background = Brushes.Green;
        //                    getallBtn.Foreground = Brushes.White;
        //                    taskLbl.Content = "";
        //                    storyLbl.Content = "";
        //                    themeLbl.Content = "";
        //                    initiativeLbl.Content = "";
        //                    featuresLbl.Content = "";

        //                    getStoryBtn.IsEnabled = false;
        //                    getTaskBtn.IsEnabled = false;
        //                    getThemeBtn.IsEnabled = false;
        //                    getInitiativeBtn.IsEnabled = false;
        //                    getFeaturesBtn.IsEnabled = false;
        //                    clrallBtn.IsEnabled = false;

        //                    validatnLbl.Visibility = System.Windows.Visibility.Hidden;
        //                    validatnLbl.Content = "";
        //                }

        //                foreach (Thread t in runningThreads)
        //                {
        //                    t.Join();
        //                }

        //            }
        //            break;
        //        case MessageBoxResult.Cancel:
        //            return;

        //    }


        //}

        private void getallBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Fetching all artifacts count is a long running process. It may take some time. Do you wish to continue?", "KovairRally", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.OK:
                    string projectId = prjctidTxt.Text;
                    i = 0;
                    if (DoConnection(projectId))
                    {
                        Task task1 = Task.Factory.StartNew(() => startNewThread("Tasks", projectId));
                        Task task2 = Task.Factory.StartNew(() => startNewThread("HierarchicalRequirement", projectId));
                        Task task3 = Task.Factory.StartNew(() => startNewThread("PortfolioItem/theme", projectId));
                        Task task4 = Task.Factory.StartNew(() => startNewThread("PortfolioItem/initiative", projectId));
                        Task task5 = Task.Factory.StartNew(() => startNewThread("PortfolioItem/feature", projectId));
                        Task task6 = Task.Factory.StartNew(() => startNewThread("Defects", projectId));
                        Task task7 = Task.Factory.StartNew(() => startNewThread("TestCases", projectId));

                        getallBtn.Content = "Processing...";
                        getallBtn.Background = Brushes.Green;
                        getallBtn.Foreground = Brushes.White;
                        taskLbl.Content = "";
                        storyLbl.Content = "";
                        themeLbl.Content = "";
                        initiativeLbl.Content = "";
                        featuresLbl.Content = "";
                        Task taskForProjectName = Task.Factory.StartNew(() => GetProjectName(projectId));

                        getStoryBtn.IsEnabled = false;
                        getTaskBtn.IsEnabled = false;
                        getThemeBtn.IsEnabled = false;
                        getInitiativeBtn.IsEnabled = false;
                        getFeaturesBtn.IsEnabled = false;
                        getDefectBtn.IsEnabled = false;
                        getTCaseBtn.IsEnabled = false;
                        clrallBtn.IsEnabled = false;
                        validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                        validatnLbl.Content = "";
                        //Task.WaitAll(task1, task2, task3, task4, task5);
                        //foreach(var thread in threads){
                        //    thread.Join();

                        //}
                        //MessageBox.Show("All task cmpleted");
                            

                        //getallBtn.Content = "Get All Artifacts Count";
                        //getallBtn.Background = Brushes.Red;
                        //getallBtn.Foreground = Brushes.White;
                        //getStoryBtn.IsEnabled = true;
                        //getTaskBtn.IsEnabled = true;
                        //getThemeBtn.IsEnabled = true;
                        //getInitiativeBtn.IsEnabled = true;
                        //getFeaturesBtn.IsEnabled = true;
                        //clrallBtn.IsEnabled = true;
                    }

                    break;
                case MessageBoxResult.Cancel:
                    return;

            }


        }

        private void startNewThread(string artifact, string projectId)
        {
            int count = 0;
            Thread thread = new Thread(() =>
                           {
                               
                               count = GetArtifactCount(projectId, artifact);
                               Action action = () =>
                               {
                                   switch (artifact)
                                   {
                                       case "Tasks": taskLbl.Content = count;
                                           break;
                                       case "HierarchicalRequirement": storyLbl.Content = count;
                                           break;
                                       case "PortfolioItem/theme": themeLbl.Content = count;
                                           break;
                                       case "PortfolioItem/initiative": initiativeLbl.Content = count;
                                           break;
                                       case "PortfolioItem/feature": featuresLbl.Content = count;
                                           break;
                                       case "Defects": defectLbl.Content = count;
                                           break;
                                       case "TestCases": tcaseLbl.Content = count;
                                           break;
                                   }
                                   if (i == 7)
                                   {
                                       getallBtn.Content = "Get All Artifacts Count";
                                       getallBtn.Background = Brushes.Red;
                                       getallBtn.Foreground = Brushes.White;
                                       getStoryBtn.IsEnabled = true;
                                       getTaskBtn.IsEnabled = true;
                                       getThemeBtn.IsEnabled = true;
                                       getInitiativeBtn.IsEnabled = true;
                                       getFeaturesBtn.IsEnabled = true;
                                       clrallBtn.IsEnabled = true;
                                       getDefectBtn.IsEnabled = true;
                                       getTCaseBtn.IsEnabled = true;
                                   }
                                   

                               };
                               Dispatcher.BeginInvoke(action);
                           });
            //threads.Add(thread);
            thread.Start();
            
            
        }

        #endregion All Artifacts counts

        #region Individual Artifacts counts

        #region Portfolio Artifacts

        private void getThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            string projectId = prjctidTxt.Text;

            int count = 0;
            Thread thread = new Thread(() =>
            {
                if (DoConnection(projectId))
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //MessageBox.Show("Invalid Rally Configuration!");
                        validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                        validatnLbl.Content = "";
                    }));
                    count = GetArtifactCount(projectId, "PortfolioItem/theme");
                    Action action = () =>
                    {
                        themeLbl.Content = count;
                        getThemeBtn.Background = Brushes.Silver;
                        getThemeBtn.Foreground = Brushes.Black;
                        getThemeBtn.Content = "Get Theme";

                    };
                    Dispatcher.BeginInvoke(action);
                }
            });
            thread.Start();
            getThemeBtn.Content = "Processing...";
            getThemeBtn.Background = Brushes.Green;
            getThemeBtn.Foreground = Brushes.White;
            themeLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();

        }

        private void getInitiativeBtn_Click(object sender, RoutedEventArgs e)
        {
            string projectId = prjctidTxt.Text;
            int count = 0;
            Thread thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //MessageBox.Show("Invalid Rally Configuration!");
                    validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                    validatnLbl.Content = "";
                }));
                if (DoConnection(projectId))
                {
                    count = GetArtifactCount(projectId, "PortfolioItem/initiative");
                    Action action = () =>
                    {
                        initiativeLbl.Content = count;
                        getInitiativeBtn.Background = Brushes.Silver;
                        getInitiativeBtn.Foreground = Brushes.Black;
                        getInitiativeBtn.Content = "Get Initiative";

                    };
                    Dispatcher.BeginInvoke(action);
                }
            });
            thread.Start();
            getInitiativeBtn.Content = "Processing...";
            getInitiativeBtn.Background = Brushes.Green;
            getInitiativeBtn.Foreground = Brushes.White;
            initiativeLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();
        }

        private void getFeaturesBtn_Click(object sender, RoutedEventArgs e)
        {
            string projectId = prjctidTxt.Text;

            int count = 0;
            //string projectId = prjctidTxt.Text;
            Thread thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //MessageBox.Show("Invalid Rally Configuration!");
                    validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                    validatnLbl.Content = "";
                }));
                if (DoConnection(projectId))
                {
                    count = GetArtifactCount(projectId, "PortfolioItem/feature");
                    Action action = () =>
                    {
                        featuresLbl.Content = count;
                        getFeaturesBtn.Background = Brushes.Silver;
                        getFeaturesBtn.Foreground = Brushes.Black;
                        getFeaturesBtn.Content = "Get Feature";

                    };
                    Dispatcher.BeginInvoke(action);
                }
            });
            thread.Start();
            getFeaturesBtn.Content = "Processing...";
            getFeaturesBtn.Background = Brushes.Green;
            getFeaturesBtn.Foreground = Brushes.White;
            featuresLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();
        }

        #endregion Portfolio Artifacts

        private void getTaskBtn_Click(object sender, RoutedEventArgs e)
        {

            int count = 0;
            string projectId = prjctidTxt.Text;
            Thread thread = new Thread(() =>
            {

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //MessageBox.Show("Invalid Rally Configuration!");
                    validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                    validatnLbl.Content = "";
                }));
                if (DoConnection(projectId))
                {
                    count = GetArtifactCount(projectId, "Tasks"); //Task
                    Action action = () =>
                    {
                        getTaskBtn.Content = "Get Task";
                        getTaskBtn.Background = Brushes.Silver;
                        getTaskBtn.Foreground = Brushes.Black;
                        taskLbl.Content = count;
                    };
                    Dispatcher.BeginInvoke(action);
                }
            });
            thread.Start();
            getTaskBtn.Foreground = Brushes.White;
            getTaskBtn.Background = Brushes.Green;
            getTaskBtn.Content = "Processing...";
            taskLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();

        }

        private void getStoryBtn_Click(object sender, RoutedEventArgs e)
        {
            //Task<int> task = new Task<int>(GetArtifactCount(prjctidTxt.Text, "HierarchicalRequirement"));
            //Task<int> task = new Task<int>(() => GetArtifactCount(prjctidTxt.Text, "HierarchicalRequirement"));
            //task.Start();
            //processing starts...
            //storyLbl.Content = await task;  
            //processing ends .

            int count = 0;
            string projectId = prjctidTxt.Text;
            Thread thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //MessageBox.Show("Invalid Rally Configuration!");
                    validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                    validatnLbl.Content = "";
                }));
                if (DoConnection(projectId))
                {
                    count = GetArtifactCount(projectId, "HierarchicalRequirement");
                    Action action = () =>
                    {
                        storyLbl.Content = count;
                        getStoryBtn.Background = Brushes.Silver;
                        getStoryBtn.Foreground = Brushes.Black;
                        getStoryBtn.Content = "Get Story";
                    };
                    Dispatcher.BeginInvoke(action);
                }
            });
            thread.Start();
            getStoryBtn.Content = "Processing...";
            getStoryBtn.Background = Brushes.Green;
            getStoryBtn.Foreground = Brushes.White;
            storyLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();

            //getStoryBtn.Content = FindResource("Play");
            //Thread thread = new Thread(() => { count = GetArtifactCount(prjctidTxt.Text, "HierarchicalRequirement"); });
            //thread.Start();
            ////processing ...
            //thread.Join();
            //storyLbl.Content = count;
            //processing endss
        }

        private void getDefectBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            string projectId = prjctidTxt.Text;
            Thread thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //MessageBox.Show("Invalid Rally Configuration!");
                    validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                    validatnLbl.Content = "";
                }));
                if (DoConnection(projectId))
                {
                    count = GetArtifactCount(projectId, "Defects");
                    Action action = () =>
                    {
                        defectLbl.Content = count;
                        getDefectBtn.Background = Brushes.Silver;
                        getDefectBtn.Foreground = Brushes.Black;
                        getDefectBtn.Content = "Get Defect";
                    };
                    Dispatcher.BeginInvoke(action);
                }
            });
            thread.Start();
            getDefectBtn.Content = "Processing...";
            getDefectBtn.Background = Brushes.Green;
            getDefectBtn.Foreground = Brushes.White;
            defectLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();
        }

        private void getTCaseBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            string projectId = prjctidTxt.Text;
            Thread thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //MessageBox.Show("Invalid Rally Configuration!");
                    validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                    validatnLbl.Content = "";
                }));
                if (DoConnection(projectId))
                {
                    count = GetArtifactCount(projectId, "TestCases");
                    Action action = () =>
                    {
                        tcaseLbl.Content = count;
                        getTCaseBtn.Background = Brushes.Silver;
                        getTCaseBtn.Foreground = Brushes.Black;
                        getTCaseBtn.Content = "Get Test Case";
                    };
                    Dispatcher.BeginInvoke(action);
                }
            });
            thread.Start();
            getTCaseBtn.Content = "Processing...";
            getTCaseBtn.Background = Brushes.Green;
            getTCaseBtn.Foreground = Brushes.White;
            tcaseLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();
        }

        #endregion Individual Artifacts counts

        private int GetArtifactCount(string projectId, string artifactName)
        {
            try
            {
                lock (this)
                {
                    i = i + 1;
                    request = new Request(artifactName);
                    request.Project = String.Format("/project/{0}", projectId);
                    request.Fetch = new List<string>() { "FormattedID", "Name", "Project" };
                    return restApi.Query(request) != null ? restApi.Query(request).TotalResultCount : 0;
                }
            }
            catch (Exception ex) 
            {
                return 0;
            }
            

        }

        private void GetProjectName(string projectId)
        {
            try
            {
                lock (this)
                {
                    if (!String.IsNullOrEmpty(projectId))
                    {
                        request = new Request("Projects");
                        request.Fetch = new List<string>() { "Name" };
                        var query = restApi.Query(request);
                        foreach (var result in query.Results)
                        {
                            string referUrl = result["_ref"].ToString();
                            if (referUrl.Contains(projectId))
                            {
                                Action action =  new Action ( () => prjctName.Content = result["Name"]);
                                Dispatcher.BeginInvoke(action);
                                break;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            { 

            }
        }

        private bool DoConnection(string projectId)
        {
            // Initialize the REST API. You can specify a web service version if needed in the constructor.
            bool isAuthencate = false;
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //string projectid = prjctidTxt.Text;
            if (!String.IsNullOrEmpty(projectId))
            {
                //restApi = new RallyRestApi();
                if (restApi.Authenticate(username, password, serverUrl, proxy: null, allowSSO: false).Equals(RallyRestApi.AuthenticationResult.Authenticated))
                {
                    isAuthencate = true;
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //MessageBox.Show("Invalid Rally Configuration!");
                        validatnLbl.Visibility = System.Windows.Visibility.Visible;
                        validatnLbl.Content = "Invalid Rally Configuration!";
                    }));
                    isAuthencate = false;
                }

            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //MessageBox.Show("Please enter Rally ProjectId!");
                    validatnLbl.Visibility = System.Windows.Visibility.Visible;
                    validatnLbl.Content = "Please enter Rally ProjectId!";
                }));
                isAuthencate = false;
            }
            //}));
            return isAuthencate;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            taskLbl.Content = "";
            storyLbl.Content = "";
            themeLbl.Content = "";
            initiativeLbl.Content = "";
            featuresLbl.Content = "";
            prjctidTxt.Text = "";
            prjctName.Content = "";
            defectLbl.Content = "";
            tcaseLbl.Content = "";
        }




    }
}
