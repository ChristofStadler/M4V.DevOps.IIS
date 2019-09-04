using LibGit2Sharp;
using System;
using System.Configuration;
using System.Web.Script.Services;
using System.Web.Services;

namespace M4V.DevOps.IIS
{
    /// <summary>
    /// Summary description for Receiver
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Receiver : System.Web.Services.WebService
    {
        [WebMethod]
        public string Update()
        {
            string key = System.Web.HttpContext.Current.Request.QueryString["key"];

            if (String.IsNullOrEmpty(key)) return "Failed";

            Project project = new Project();
            project.Key = key;

            // 1 - GET REPO INFO
            project.GitEmail = ConfigurationManager.AppSettings["Git.Email"];
            project.GitUsername = ConfigurationManager.AppSettings["Git.Username"];
            project.Path = ConfigurationManager.AppSettings[$"Project.{project.Key}"];

            // 2 - PULL LATEST
            return project.Update();
        }

        public class Project
        {
            public string Path { get; set; }
            public string Key { get; set; }

            public string GitEmail { get; set; }
            public string GitUsername { get; set; }
            private new DefaultCredentials GitCredentials { get; set; }

            public string Update()
            {
                try
                {
                    GitCredentials = new DefaultCredentials(); 

                    using (var repo = new Repository(Path))
                    {
                        var gitSignature = new Signature(GitUsername, GitEmail, DateTimeOffset.Now);

                        // FETCH
                        Commands.Fetch(repo, "origin", new string[0], new FetchOptions { CredentialsProvider = (_url, _user, _cred) => GitCredentials, TagFetchMode = TagFetchMode.All }, null);

                        // MERGE
                        MergeResult result = repo.MergeFetchedRefs(gitSignature, new MergeOptions());
                    }
                }
                catch (Exception e)
                {
                    return System.Web.HttpContext.Current.Request.IsLocal ? e.ToString() : "Failed";
                }        
                
                return "Success";
            }
        }
    }
}
