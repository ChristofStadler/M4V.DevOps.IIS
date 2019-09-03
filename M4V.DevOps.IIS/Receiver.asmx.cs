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
            Project project = new Project();
            project.Key = key;

            // 1 - GET REPO INFO
            project.GitEmail = ConfigurationManager.AppSettings["Git.Email"];
            project.GitPassword = ConfigurationManager.AppSettings["Git.Password"];
            project.GitUsername = ConfigurationManager.AppSettings["Git.Username"];
            project.PathDEV = ConfigurationManager.AppSettings[$"Project.{project.Key}.DEV"];
            project.PathLIVE = ConfigurationManager.AppSettings[$"Project.{project.Key}.LIVE"];

            // 2 - PULL LATEST
            return project.Update() ? "Success" : "Failed";
        }

        public class Project
        {
            public string PathDEV { get; set; }
            public string PathLIVE { get; set; }
            public string Key { get; set; }

            public string GitEmail { get; set; }
            public string GitUsername { get; set; }
            public string GitPassword { get; set; }
            private UsernamePasswordCredentials GitCredentials { get; set; }

            public bool Update()
            {
                try
                {
                    // git checkout live
                    // git checkout dev
                    // git fetch
                    // git merge
                    GitCredentials = new UsernamePasswordCredentials { Username = GitUsername, Password = GitPassword };

                    // Updated LIVE
                    using (var repo = new Repository(PathLIVE))
                    {
                        var gitSignature = new Signature(GitUsername, GitEmail, DateTimeOffset.Now);

                        // CHECKOUT
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

                        // CHECKOUT
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
