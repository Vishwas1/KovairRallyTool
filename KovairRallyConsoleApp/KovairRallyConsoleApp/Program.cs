using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rally.RestApi;
using Rally.RestApi.Response;
namespace KovairRallyConsoleApp
{
    class Program
    {
        private static Request request = null;
        private QueryResult queryResult = null;
        static RallyRestApi restApi = null;

        static void Main(string[] args)
        {
            
            #region other queries 
            ////Create an item
            //DynamicJsonObject toCreate = new DynamicJsonObject();
            //toCreate["Name"] = "My Defect";
            //CreateResult createResult = restApi.Create("defect", toCreate);

            ////Update the item
            //DynamicJsonObject toUpdate = new DynamicJsonObject();
            //toUpdate["Description"] = "This is my defect.";
            //OperationResult updateResult = restApi.Update(createResult.Reference, toUpdate);

            ////Get the item
            //DynamicJsonObject item = restApi.GetByReference(createResult.Reference);

            //Query for items
            //Request request = new Request("HierarchicalRequirement");
            ////request.Workspace = "/workspace/77953801696";
            //request.Project = "/project/28252068177";
            //request.Fetch = new List<string>() { "FormattedID", "Name", "Tasks", "Release", "Project", "Owner" };
            //request.Fetch = new List<string>() { "Name", "FormattedID"};
            //request.Query = new Query("Name", Query.Operator.Equals, "My Defect");
            #endregion other queries

            if (DoConnection())
            {
                //GetArtifactCount("56479390676", "Tasks"); //Task

                //GetArtifactCount("56479390676", "Defect"); //Defect

                //GetArtifactCount("56479390676", "HierarchicalRequirement"); // User Story

                //GetArtifactCount("56479390676", "PortfolioItem/theme"); //Theme

                //GetArtifactCount("56479390676", "PortfolioItem/initiative"); //Initiative

                //GetArtifactCount("56479390676", "PortfolioItem/feature");  //Feature

                //GetArtifactCount("56479390676", "TestCases");  //TestCases
                
                string[] artifacts = { "Tasks", "Defect", "HierarchicalRequirement", "PortfolioItem/theme", "PortfolioItem/initiative", "PortfolioItem/feature" };
                
                //string[] artifacts = { "Tasks", "Defect"};
                
                //var tasks = artifacts.Select(t => Task<int>.Factory.StartNew(() => GetArtifactCount("56479390676", t))).ToArray();

                //Task.WaitAll(tasks);

                //var r = tasks.Select(task => task.Result).ToList();

                //Task<int>[] taskArray = { Task<int>.Factory.StartNew(() => GetArtifactCount("56479390676",artifacts[0])),
                //                     Task<int>.Factory.StartNew(() => GetArtifactCount("56479390676",artifacts[1])), 
                //                     Task<int>.Factory.StartNew(() => GetArtifactCount("56479390676",artifacts[2])) };


                Task<int>[] taskArray = { Task<int>.Factory.StartNew(() => GetArtifactCount("28249260968", artifacts[4])) };
                Task.WaitAll(taskArray);
                var results = new Int32[taskArray.Length];

            }
            else
            {
                // not authenticated
            }
            
        }

        private static bool DoConnection()
        {
            string username = "vb807t@att.com ";
            string password = "KovairRally1@";
            string serverUrl = "https://rally1.rallydev.com/";
            // Initialize the REST API. You can specify a web service version if needed in the constructor.
            restApi = new RallyRestApi();
            return restApi.Authenticate(username, password, serverUrl, proxy: null, allowSSO: false).Equals(RallyRestApi.AuthenticationResult.Authenticated) ? true : false;
        }

        private static int GetArtifactCount(string projectId, string artifactName) 
        {
            if (!String.IsNullOrEmpty(projectId) && !String.IsNullOrEmpty(artifactName))
            {
                request = new Request("Projects");
                request.Project = String.Format("/project/{0}", projectId);
                request.Fetch =  new List<string>() { "Name" };
                var query = restApi.Query(request).Results;
               // var dd = query.AsEnumerable().Any(o => o.Fields["_ref"].ToString().indexOf(projectId)>0);
                //var rr = query.Any(o => o["_ref"].ToString().contains(projectId));
                //var project = query["Project"];
                //var projectname = project["Name"];
                //Console.WriteLine("Name of project is::  {0}", projectname);
                foreach (var result in query)
                {
                    //Request projectsRequest = new Request(result["Projects"]);
                    var referUrl = result["_ref"].ToString();
                    if (referUrl.Contains(projectId))
                    {
                        var projectname = result["Name"];
                        Console.WriteLine("Name of project is::  {0}", projectname);
                        break;
                    }
                        
                }
                return restApi.Query(request) != null ? restApi.Query(request).TotalResultCount : 0;
            }
            else
            {
                return 0;
            }
        }

        #region Individual Artifact Count
        //private static int GetDefectCount(string projectId) 
        //{
        //    if (!String.IsNullOrEmpty(projectId))
        //    {
        //        request = new Request("Defect");
        //        request.Project = String.Format("/project/{0}", projectId);
        //        request.Fetch = new List<string>() { "FormattedID", "Name" };
        //        return restApi.Query(request)!=null ? restApi.Query(request).TotalResultCount : 0;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
            
        //}

        //private static int GetTaskCount(string projectId) 
        //{
        //    return 0;
        //}

        //private static int GetUserStoryCount(string projectId) 
        //{
        //    if (!String.IsNullOrEmpty(projectId))
        //    { 
            
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //    request = new Request("HierarchicalRequirement");
        //    request.Project = String.Format("/project/{0}", projectId);
        //    request.Fetch = new List<string>() { "FormattedID", "Name" };
        //    return restApi.Query(request) != null ? restApi.Query(request).TotalResultCount : 0;
        //}

        //private static int GetTheameCount(string projectId) 
        //{
        //    if (!String.IsNullOrEmpty(projectId))
        //    {

        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //    request = new Request("PortfolioItem/theme");
        //    request.Project = String.Format("/project/{0}", projectId);
        //    request.Fetch = new List<string>() { "FormattedID", "Name" };
        //    return restApi.Query(request) != null ? restApi.Query(request).TotalResultCount : 0;
        //}

        //private static int GetInitiativeCount(string projectId) 
        //{
        //    if (!String.IsNullOrEmpty(projectId))
        //    {

        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //    request = new Request("PortfolioItem/initiative");
        //    request.Project = String.Format("/project/{0}", projectId);
        //    request.Fetch = new List<string>() { "FormattedID", "Name" };
        //    return restApi.Query(request) != null ? restApi.Query(request).TotalResultCount : 0;
        //}

        //private static int GetFeatureCount(string projectId) 
        //{
        //    if (!String.IsNullOrEmpty(projectId))
        //    {

        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //    request = new Request("PortfolioItem/feature");
        //    request.Project = String.Format("/project/{0}", projectId);
        //    request.Fetch = new List<string>() { "FormattedID", "Name" };
        //    return restApi.Query(request) != null ? restApi.Query(request).TotalResultCount : 0;
        //}

        #endregion Individual Artifact Count
    }
}
