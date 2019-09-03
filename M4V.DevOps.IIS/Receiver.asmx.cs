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
    // [System.Web.Script.Services.ScriptService]
    public class Receiver : System.Web.Services.WebService
    {
        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public string Update(string key)
        {
            Project proj = new Project();
            proj.Key = key;

            // 1 - GET REPO INFO
            proj.GitEmail = ConfigurationManager.AppSettings["Git.Email"];
            proj.GitPassword = ConfigurationManager.AppSettings["Git.Password"];
            proj.GitUsername = ConfigurationManager.AppSettings["Git.Username"];
            proj.PathDEV = ConfigurationManager.AppSettings[$"Project.{proj.Key}.DEV"];
            proj.PathLIVE = ConfigurationManager.AppSettings[$"Project.{proj.Key}.LIVE"];

            // 2 - PULL LATEST
            return proj.Update() ? "Success" : "Failed";
        }
        public class Project
        {
            public string PathDEV { get; set; }
            public string PathLIVE { get; set; }
            public string Key { get; set; }
            public string URL { get; set; }

            public string GitEmail { get; set; }
            public string GitUsername { get; set; }
            public string GitPassword { get; set; }
            private UsernamePasswordCredentials GitCredentials { get; set; }

            public Project()
            {

            }

            public bool Update()
            {
                try
                {
                    // git checkout master
                    // git checkout staging
                    // git fetch
                    // git merge
                    GitCredentials = new UsernamePasswordCredentials { Username = GitUsername, Password = GitPassword };

                    // Updated LIVE
                    using (var repo = new Repository(PathLIVE))
                    {
                        var gitSignature = new Signature(GitUsername, GitEmail, DateTimeOffset.Now);

                        Branch branch = Commands.Checkout(repo, repo.Branches["master"]);

                        // FETCH
                        Commands.Fetch(repo, "origin", new string[0], new FetchOptions { CredentialsProvider = (_url, _user, _cred) => GitCredentials, TagFetchMode = TagFetchMode.All }, null);

                        // MERGE
                        MergeResult result = repo.Merge("master", gitSignature);
                    }

                    // Updated DEV
                    using (var repo = new Repository(PathDEV))
                    {
                        var gitSignature = new Signature(GitUsername, GitEmail, DateTimeOffset.Now);

                        Branch branch = Commands.Checkout(repo, repo.Branches["dev"]);

                        // FETCH
                        Commands.Fetch(repo, "origin", new string[0], new FetchOptions { CredentialsProvider = (_url, _user, _cred) => GitCredentials, TagFetchMode = TagFetchMode.All }, null);

                        // MERGE
                        MergeResult result = repo.Merge("dev", gitSignature);
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
                               
                return true;
            }
        }
    }
}
