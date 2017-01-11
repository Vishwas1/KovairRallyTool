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
using log4net;


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
        private static readonly ILog logger = LogManager.GetLogger("logger");
        //List<Thread> threads = new List<Thread>();
        //Thread[] array = new Thread[5];
        int i=0;
        public MainWindow()
        {
            
            //FileInfo fi = new FileInfo("log4net.xml");
            //log4net.Config.XmlConfigurator.Configure(fi);
            //log4net.GlobalContext.Properties["host"] = Environment.MachineName;
            log4net.Config.XmlConfigurator.Configure();
            logger.Debug("MainWindow:: Starts...");
            logger.Debug("MainWindow:: Before InitializeComponent.");
            InitializeComponent();
            logger.Debug("MainWindow:: After InitializeComponent.");
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            Loaded += MainWindow_Loaded;
            //toggleExpanderCollasp
            //portExpander.Expanded += new RoutedEventHandler(toggleExpanderCollasp);
            //delivExpander.Expanded += new RoutedEventHandler(toggleExpanderCollasp);  
            logger.Debug("MainWindow:: Ends.");
        }

        private void toggleExpanderCollasp(object sender, RoutedEventArgs args)
        {
            logger.Debug("toggleExpanderCollasp:: Starts...");
            //Do something when the Expander control collapses
            var senderExpnd = ((System.Windows.Controls.HeaderedContentControl)(sender)).Header.ToString().Contains("Deliv") ? "DelivExpander": "PortExpander";
            switch (senderExpnd) {
                case "DelivExpander": if (portExpander != null) portExpander.IsExpanded = false;
                    break;
                case "PortExpander": if (delivExpander!=null) delivExpander.IsExpanded = false;
                    break;
            }
            logger.Debug("toggleExpanderCollasp:: Ends.");
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Debug("MainWindow_Loaded:: Starts...");
            restApi = new RallyRestApi();
            if (File.Exists("RallyConfig.xml"))
            {
                logger.Debug("MainWindow_Loaded:: RallyConfig.xml exists");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("RallyConfig.xml");
                logger.Debug("MainWindow_Loaded::After loading RallyConfig.xml");
                XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/root");

                serverUrl = urlTxt.Text = nodeList[0].ChildNodes[0].InnerText;
                username = unameTxt.Text = nodeList[0].ChildNodes[1].InnerText;
                password = pwdTxt.Password = nodeList[0].ChildNodes[2].InnerText;
                logger.DebugFormat("MainWindow_Loaded::serverUrl ={0}", serverUrl);
                logger.DebugFormat("MainWindow_Loaded::username ={0}", username);
                logger.DebugFormat("MainWindow_Loaded::password ={0}", password);
            }
            else
            {
                logger.Debug("MainWindow_Loaded:: RallyConfig.xml does not exists");
                urlTxt.Text = "";
                unameTxt.Text = "";
                pwdTxt.Password = "";
            }
            logger.Debug("MainWindow_Loaded:: Ends.");
        }

        private void saveConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.DebugFormat("saveConfigBtn_Click:: Starts...");
            if (!String.IsNullOrEmpty(urlTxt.Text) && !String.IsNullOrEmpty(unameTxt.Text) && !String.IsNullOrEmpty(pwdTxt.Password))
            {
                logger.DebugFormat("saveConfigBtn_Click:: Inside if");
                username = unameTxt.Text;
                password = pwdTxt.Password;
                serverUrl = urlTxt.Text;

                logger.DebugFormat("saveConfigBtn_Click:: New thread configured");
                Thread thread = new Thread(() =>
                {
                    bool isConfigCorrect = false;
                    logger.DebugFormat("saveConfigBtn_Click:: Inside IF : Before calling restApi.Authenticate(). username = {0}, password = {1}, serverUrl= {2}",username,password,serverUrl);
                    isConfigCorrect = restApi.Authenticate(username, password, serverUrl, proxy: null, allowSSO: false).Equals(RallyRestApi.AuthenticationResult.Authenticated);
                    logger.DebugFormat("saveConfigBtn_Click:: Inside IF : After calling restApi.Authenticate()");
                    Action action = () =>
                    {
                        if (isConfigCorrect)
                        {
                            logger.DebugFormat("saveConfigBtn_Click:: Inside IF : isConfigCorrect = {0}", isConfigCorrect);
                            if (File.Exists("RallyConfig.xml"))
                                File.Delete("RallyConfig.xml");

                            string rallyConfigXml = "<root><url>" + serverUrl + "</url><username>" + username + "</username><password>" + password + "</password></root>";
                            logger.DebugFormat("saveConfigBtn_Click:: Inside IF : rallyConfigXml ={0}", rallyConfigXml);
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(rallyConfigXml);
                            doc.Save("RallyConfig.xml");
                            logger.DebugFormat("saveConfigBtn_Click:: Inside IF :After saving rallyConfigXml into  RallyConfig.xml");
                            mandFldLbl.Visibility = System.Windows.Visibility.Visible;
                            mandFldLbl.Content = "Configuration is successfully saved!";
                            logger.DebugFormat("saveConfigBtn_Click:: Inside IF :Configuration is successfully saved!");
                        }
                        else
                        {
                            logger.DebugFormat("saveConfigBtn_Click:: Inside Else : isConfigCorrect = {0}", isConfigCorrect);
                            mandFldLbl.Visibility = System.Windows.Visibility.Visible;
                            mandFldLbl.Content = "Invalid rally configuration!";
                            logger.DebugFormat("saveConfigBtn_Click:: Inside Else :Invalid rally configuration!!");
                        }
                        clrallBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        saveConfigBtn.Background = Brushes.Silver;
                        saveConfigBtn.Foreground = Brushes.Black;
                        saveConfigBtn.Content = "Save Configuration";

                    };
                    Dispatcher.BeginInvoke(action);

                });
                thread.Start();
                logger.DebugFormat("saveConfigBtn_Click:: New thread starts");
                saveConfigBtn.Content = "Processing...";
                saveConfigBtn.Background = Brushes.Green;
                saveConfigBtn.Foreground = Brushes.White;


            }
            else
            {
                logger.DebugFormat("saveConfigBtn_Click:: Inside else");
                mandFldLbl.Visibility = System.Windows.Visibility.Visible;
                mandFldLbl.Content = "*All fields are mandatory.";
            }
            logger.DebugFormat("saveConfigBtn_Click:: Ends.");
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
            logger.DebugFormat("getallBtn_Click:: Starts...");
            MessageBoxResult result = MessageBox.Show("Fetching all artifacts count is a long running process. It may take some time. Do you wish to continue?", "KovairRally", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.OK:
                    logger.DebugFormat("getallBtn_Click:: Inside Case MessageBoxResult.OK:: Starts...");
                    string projectId = prjctidTxt.Text;
                    i = 0;
                    logger.DebugFormat("getallBtn_Click:: Before calling DoConnection :: projectId={0}",projectId);
                    if (DoConnection(projectId))
                    {
                        logger.DebugFormat("getallBtn_Click:: After calling DoConnection :: projectId={0}", projectId);
                        
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=Task");
                        Task task1 = Task.Factory.StartNew(() => startNewThread("Tasks", projectId));
                        logger.DebugFormat("getallBtn_Click:: After calling startNewThread :: artifact=Task");
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=HierarchicalRequirement");
                        Task task2 = Task.Factory.StartNew(() => startNewThread("HierarchicalRequirement", projectId));
                        logger.DebugFormat("getallBtn_Click:: After calling startNewThread :: artifact=HierarchicalRequirement");
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=PortfolioItem/theme");
                        Task task3 = Task.Factory.StartNew(() => startNewThread("PortfolioItem/theme", projectId));
                        logger.DebugFormat("getallBtn_Click:: After calling startNewThread :: artifact=PortfolioItem/theme");
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=PortfolioItem/initiative");
                        Task task4 = Task.Factory.StartNew(() => startNewThread("PortfolioItem/initiative", projectId));
                        logger.DebugFormat("getallBtn_Click:: After calling startNewThread :: artifact=PortfolioItem/initiative");
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=PortfolioItem/feature");
                        Task task5 = Task.Factory.StartNew(() => startNewThread("PortfolioItem/feature", projectId));
                        logger.DebugFormat("getallBtn_Click:: After calling startNewThread :: artifact=PortfolioItem/feature");
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=Defects");
                        Task task6 = Task.Factory.StartNew(() => startNewThread("Defects", projectId));
                        logger.DebugFormat("getallBtn_Click:: After calling startNewThread :: artifact=Defects");
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=TestCases");
                        Task task7 = Task.Factory.StartNew(() => startNewThread("TestCases", projectId));
                        logger.DebugFormat("getallBtn_Click:: Before calling startNewThread :: artifact=TestCases");
                        getallBtn.Content = "Processing...";
                        getallBtn.Background = Brushes.Green;
                        getallBtn.Foreground = Brushes.White;
                        taskLbl.Content = "";
                        storyLbl.Content = "";
                        themeLbl.Content = "";
                        initiativeLbl.Content = "";
                        defectLbl.Content = "";
                        tcaseLbl.Content = "";
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
                    logger.DebugFormat("getallBtn_Click::  Case MessageBoxResult.OK:: End...");
                    break;

                case MessageBoxResult.Cancel:
                        logger.DebugFormat("getallBtn_Click:: Inside Case MessageBoxResult.Cancel:: Starts...");
                    return;


            }

            logger.DebugFormat("getallBtn_Click:: End...");
        }

        private void startNewThread(string artifact, string projectId)
        {
            logger.DebugFormat("startNewThread:: Starts...");
            int count = 0;
            logger.DebugFormat("Before create object:: thread");
            Thread thread = new Thread(() =>
                           {
                               logger.DebugFormat("GetArtifactCount:: Before calling...::projectId={0}" + projectId + "::artifact={1}" + artifact);
                               count = GetArtifactCount(projectId, artifact);
                               logger.DebugFormat("GetArtifactCount:: After Calling...::count={0}"+count);
                               Action action = () =>
                               {                             
                                   switch (artifact)
                                   {
                                       case "Tasks":
                                           logger.DebugFormat("startNewThread:: Inside Case...artifact=Task");
                                           taskLbl.Content = count;
                                           logger.DebugFormat("startNewThread:: End Case...artifact=Task");                                        
                                           break;
                                       case "HierarchicalRequirement":
                                           logger.DebugFormat("startNewThread:: Inside Case... artifact=HierarchicalRequirement");
                                           storyLbl.Content = count;
                                           logger.DebugFormat("startNewThread:: End Case... artifact=HierarchicalRequirement");
                                           break;
                                       case "PortfolioItem/theme":
                                           logger.DebugFormat("startNewThread:: Inside Case... artifact=PortfolioItem/theme");
                                           themeLbl.Content = count;
                                           logger.DebugFormat("startNewThread:: End Case... artifact=PortfolioItem/theme");
                                           break;
                                       case "PortfolioItem/initiative":
                                           logger.DebugFormat("startNewThread:: Inside Case... artifact=PortfolioItem/initiative");
                                           initiativeLbl.Content = count;
                                           logger.DebugFormat("startNewThread:: End Case... artifact=PortfolioItem/initiative");
                                           break;
                                       case "PortfolioItem/feature":
                                           logger.DebugFormat("startNewThread:: Inside Case... artifact=PortfolioItem/feature");
                                           featuresLbl.Content = count;
                                           logger.DebugFormat("startNewThread:: End Case... artifact=PortfolioItem/feature");
                                           break;
                                       case "Defects":
                                           logger.DebugFormat("startNewThread:: Inside Case... artifact=Defects");
                                           defectLbl.Content = count;
                                           logger.DebugFormat("startNewThread:: End Case... artifact=Defects");
                                           break;
                                       case "TestCases":
                                           logger.DebugFormat("startNewThread:: Inside Case... artifact=TestCases");
                                           tcaseLbl.Content = count;
                                           logger.DebugFormat("startNewThread:: End Case... artifact=TestCases");
                                           break;
                                   }

                                   if (i == 7)
                                   {
                                       logger.Debug("startNewThread:: Begin If i==7");
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
                                       logger.Debug("startNewThread:: End If i==7");
                                   }
                                   

                               };
                               Dispatcher.BeginInvoke(action);
                           });
            logger.DebugFormat("After create object:: thread");
            //threads.Add(thread);
            thread.Start();
            logger.DebugFormat("startNewThread:: End...");
            
        }

        #endregion All Artifacts counts

        #region Individual Artifacts counts

        #region Portfolio Artifacts

        private void getThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.DebugFormat("getThemeBtn_Click:: Starts");
            string projectId = prjctidTxt.Text;

            int count = 0;
            Thread thread = new Thread(() =>
            {
                logger.DebugFormat("getThemeBtn_Click:: DoConnection Begin call with projectId={0}", projectId);
                if (DoConnection(projectId))
                {
                    logger.DebugFormat("getThemeBtn_Click:: DoConnection End call with");
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //MessageBox.Show("Invalid Rally Configuration!");
                        validatnLbl.Visibility = System.Windows.Visibility.Hidden;
                        validatnLbl.Content = "";
                    }));
                    logger.DebugFormat("getThemeBtn_Click:: GetArtifactCountc Begin call with artifactName=PortfolioItem/theme");
                    count = GetArtifactCount(projectId, "PortfolioItem/theme");
                    logger.DebugFormat("getThemeBtn_Click:: GetArtifactCountc End call count={0}",count);
                    Action action = () =>
                    {
                        themeLbl.Content = count;
                        
                    };
                    Dispatcher.BeginInvoke(action);
                }
                Action action1 = () =>
                {
                    getThemeBtn.Background = Brushes.Silver;
                    getThemeBtn.Foreground = Brushes.Black;
                    getThemeBtn.Content = "Get Theme";

                };
                Dispatcher.BeginInvoke(action1);
            });
            thread.Start();
            getThemeBtn.Content = "Processing...";
            getThemeBtn.Background = Brushes.Green;
            getThemeBtn.Foreground = Brushes.White;
            themeLbl.Content = "";
            logger.DebugFormat("getThemeBtn_Click:: GetProjectName Begin call with projectId={0}", projectId);
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            logger.DebugFormat("getThemeBtn_Click:: GetProjectName End call taskForProjectName={0}", taskForProjectName);
            taskForProjectName.Start();
            logger.DebugFormat("getThemeBtn_Click:: End");
        }

        private void getInitiativeBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.DebugFormat("getInitiativeBtn_Click:: Start");
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
                logger.DebugFormat("getInitiativeBtn_Click:: DoConnection Before call with projectId={0}", projectId);
                if (DoConnection(projectId))
                {
                    logger.DebugFormat("getInitiativeBtn_Click:: DoConnection End");
                    logger.DebugFormat("getInitiativeBtn_Click:: GetArtifactCount Before call with artifactName= PortfolioItem/initiative");
                    count = GetArtifactCount(projectId, "PortfolioItem/initiative");

                    logger.DebugFormat("getInitiativeBtn_Click:: GetArtifactCount After call count={0}", count);
                    Action action = () =>
                    {
                        initiativeLbl.Content = count;
                        
                    };
                    Dispatcher.BeginInvoke(action);
                } 
                Action action1 = () =>
                {
                    getInitiativeBtn.Background = Brushes.Silver;
                    getInitiativeBtn.Foreground = Brushes.Black;
                    getInitiativeBtn.Content = "Get Initiative";

                };
                Dispatcher.BeginInvoke(action1);
            });
            thread.Start();
            getInitiativeBtn.Content = "Processing...";
            getInitiativeBtn.Background = Brushes.Green;
            getInitiativeBtn.Foreground = Brushes.White;
            initiativeLbl.Content = "";
            logger.DebugFormat("getInitiativeBtn_Click:: GetProjectName Before call with projectId={0}", projectId);
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            logger.DebugFormat("getInitiativeBtn_Click:: GetProjectName End call taskForProjectName={0}", taskForProjectName);
            taskForProjectName.Start();
            logger.DebugFormat("getInitiativeBtn_Click:: End");
        }

        private void getFeaturesBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.DebugFormat("getFeaturesBtn_Click:: Start..");
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
                        
                    };
                    Dispatcher.BeginInvoke(action);
                }
                Action action1 = () =>
                {
                    
                    getFeaturesBtn.Background = Brushes.Silver;
                    getFeaturesBtn.Foreground = Brushes.Black;
                    getFeaturesBtn.Content = "Get Feature";

                };
                Dispatcher.BeginInvoke(action1);
            });
            thread.Start();
            getFeaturesBtn.Content = "Processing...";
            getFeaturesBtn.Background = Brushes.Green;
            getFeaturesBtn.Foreground = Brushes.White;
            featuresLbl.Content = "";
            Task taskForProjectName = new Task(() => GetProjectName(projectId));
            taskForProjectName.Start();
            logger.DebugFormat("getFeaturesBtn_Click:: Ends..");
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
                        taskLbl.Content = count;
                    };
                    Dispatcher.BeginInvoke(action);
                }
                Action action1 = () =>
                {
                    getTaskBtn.Content = "Get Task";
                    getTaskBtn.Background = Brushes.Silver;
                    getTaskBtn.Foreground = Brushes.Black;
                    //taskLbl.Content = count;
                };
                Dispatcher.BeginInvoke(action1);
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
                        
                    };
                    Dispatcher.BeginInvoke(action);
                }

                Action action1 = () =>
                {
                    getStoryBtn.Background = Brushes.Silver;
                    getStoryBtn.Foreground = Brushes.Black;
                    getStoryBtn.Content = "Get Story";
                };
                Dispatcher.BeginInvoke(action1);
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
                Action action1 = () =>
                {
                    
                    getDefectBtn.Background = Brushes.Silver;
                    getDefectBtn.Foreground = Brushes.Black;
                    getDefectBtn.Content = "Get Defect";
                };
                Dispatcher.BeginInvoke(action1);
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
                        
                    };
                    Dispatcher.BeginInvoke(action);
                }
                Action action1 = () =>
                {
                    getTCaseBtn.Background = Brushes.Silver;
                    getTCaseBtn.Foreground = Brushes.Black;
                    getTCaseBtn.Content = "Get Test Case";
                };
                Dispatcher.BeginInvoke(action1);
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
            
            bool isAuthencate = false;
            try
            {
                logger.DebugFormat("DoConnection:: Starts ...");
                if (!String.IsNullOrEmpty(projectId))
                {
                    logger.DebugFormat("DoConnection:: Inside if projectId = {0}", projectId);
                    if (restApi.Authenticate(username, password, serverUrl, proxy: null, allowSSO: false).Equals(RallyRestApi.AuthenticationResult.Authenticated))
                    {
                        logger.DebugFormat("DoConnection:: Inside if: Inside id. After calling  Authenticate");
                        isAuthencate = true;
                    }
                    else
                    {
                        logger.DebugFormat("DoConnection:: Inside if: Inside else. After calling  Authenticate");
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
                    logger.DebugFormat("DoConnection:: Inside else");
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        validatnLbl.Visibility = System.Windows.Visibility.Visible;
                        validatnLbl.Content = "Please enter Rally ProjectId!";
                    }));
                    isAuthencate = false;
                }
                return isAuthencate;
            }
            catch (Exception ex)
            {
                return isAuthencate;
            }
            finally 
            {
                logger.DebugFormat("DoConnection:: Returning isAuthencate = {0}.", isAuthencate);
                logger.DebugFormat("DoConnection:: Ends.");
            }
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
