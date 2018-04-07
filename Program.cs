using System;
//using System.Xml.Linq;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Configuration;


namespace Jenkinsjobs
{
    class Program
    {
        static void Main(string[] args)
        {
            string type = args[0];
            string xmlfile = args[1].ToString();
            string BuildNumber = "";
            string ProjectName = "";
            string BuildStatus = "";
            DateTime BuildDate = DateTime.MinValue;           
            string Duration = "";
            string BuildCreator = "";            
            string BuildId = "";
            
            string PromJobName ="";
            string PromProjectName = "";
            string PromotionEnv = "";
            string PromBuildNumber = "";
            string PromotionNbr = "";
            DateTime PromotionDate = DateTime.MinValue;
            string PromotionStatus = "";
            string PromDuration = "";
            string PromotionCreator = "";

            XmlDocument builddoc = new XmlDocument();
            builddoc.Load(xmlfile);

            if (type == "Build")
            {
                XmlNodeList displayNames = builddoc.SelectNodes("//freeStyleBuild/displayName");

                foreach (XmlElement displayName in displayNames)
                {
                    BuildNumber = displayName.InnerXml;
                }

                XmlNodeList ids = builddoc.SelectNodes("//freeStyleBuild/id");

                foreach (XmlElement id in ids)
                {
                    BuildId = id.InnerXml;
                }

                XmlNodeList projnames = builddoc.SelectNodes("//freeStyleBuild/url");
                foreach (XmlElement projname in projnames)
                {
                    string s = projname.InnerXml;
                    var startTag = "job/";
                    int startIndex = s.IndexOf(startTag) + startTag.Length;
                    int endIndex = s.IndexOf("/" + BuildId, startIndex);
                    ProjectName = s.Substring(startIndex, endIndex - startIndex);
                }

                XmlNodeList results = builddoc.SelectNodes("//freeStyleBuild/result");
                foreach (XmlElement result in results)
                {
                    BuildStatus = result.InnerXml;
                }

                XmlNodeList timestamps = builddoc.SelectNodes("//freeStyleBuild/timestamp");
                foreach (XmlElement timestamp in timestamps)
                {
                    Int64 miliseconds = Convert.ToInt64(timestamp.InnerXml);
                    BuildDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(miliseconds / 1000d)).ToLocalTime();              

                }

                XmlNodeList durations = builddoc.SelectNodes("//freeStyleBuild/duration");
                foreach (XmlElement duration in durations)
                {
                    Int64 miliseconds = Convert.ToInt64(duration.InnerXml);
                    TimeSpan t = TimeSpan.FromMilliseconds(miliseconds);
                    Duration = string.Format("{0:D2}h:{1:D2}m",t.Hours,t.Minutes);                
                }

                XmlNodeList userids = builddoc.SelectNodes("//freeStyleBuild/action/cause/userName");
                foreach (XmlElement userid in userids)
                {
                    BuildCreator = userid.InnerXml;
                }
                var connectionString = ConfigurationManager.ConnectionStrings["Jenkins"].ConnectionString;
                SqlConnection sqlConn = new SqlConnection(connectionString);
                SqlCommand sqlComm = new SqlCommand();
                sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = "INSERT INTO Jobs (ProjectName, BuildNumber, BuildDate, Duration, BuildStatus, BuildCreator) VALUES (@ProjectName, @BuildNumber, @BuildDate, @Duration, @BuildStatus, @BuildCreator)";
                sqlComm.Parameters.AddWithValue("@ProjectName", ProjectName);
                sqlComm.Parameters.AddWithValue("@BuildNumber", BuildNumber);
                sqlComm.Parameters.AddWithValue("@BuildDate", BuildDate);
                sqlComm.Parameters.AddWithValue("@Duration", Duration);
                sqlComm.Parameters.AddWithValue("@BuildStatus", BuildStatus);
                sqlComm.Parameters.AddWithValue("@BuildCreator", BuildCreator);
                sqlConn.Open();
                sqlComm.ExecuteNonQuery();
                sqlConn.Close();
            }

            if (type == "Pramotion")
            {
                string JobName = args[2];
                PromJobName = JobName;
                var res = PromJobName.Split('/');
                PromProjectName = res[0];
                PromotionEnv = res[2];
                PromBuildNumber = args[3];

                XmlNodeList results = builddoc.SelectNodes("//promotion/result");
                foreach (XmlElement result in results)
                {
                    PromotionStatus = result.InnerXml;
                }

                XmlNodeList timestamps = builddoc.SelectNodes("//promotion/timestamp");
                foreach (XmlElement timestamp in timestamps)
                {
                    Int64 miliseconds = Convert.ToInt64(timestamp.InnerXml);
                    PromotionDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(miliseconds / 1000d)).ToLocalTime();

                }

                XmlNodeList durations = builddoc.SelectNodes("//promotion/duration");
                foreach (XmlElement duration in durations)
                {
                    Int64 miliseconds = Convert.ToInt64(duration.InnerXml);
                    TimeSpan t = TimeSpan.FromMilliseconds(miliseconds);
                    PromDuration = string.Format("{0:D2}h:{1:D2}m", t.Hours, t.Minutes);
                }

                XmlNodeList userids = builddoc.SelectNodes("//promotion/action/cause/userName");
                foreach (XmlElement userid in userids)
                {
                    PromotionCreator = userid.InnerXml;
                }

                XmlNodeList ids = builddoc.SelectNodes("//promotion/id");
                foreach (XmlElement id in ids)
                {
                    PromotionNbr = id.InnerXml;
                }
                var connectionString = ConfigurationManager.ConnectionStrings["Jenkins"].ConnectionString;
                SqlConnection sqlConn = new SqlConnection(connectionString);
                SqlCommand sqlComm = new SqlCommand();
                sqlComm = sqlConn.CreateCommand();
                sqlComm.CommandText = "INSERT INTO Promotions (ProjectName, PromotionEnv, BuildNumber, PromotionNbr, PromotionDate, PromotionStatus,Duration, PromotionCreator) VALUES (@ProjectName, @PromotionEnv, @BuildNumber, @PromotionNbr, @PromotionDate, @PromotionStatus, @Duration, @PromotionCreator)";
                sqlComm.Parameters.AddWithValue("@ProjectName", PromProjectName);
                sqlComm.Parameters.AddWithValue("@PromotionEnv", PromotionEnv);
                sqlComm.Parameters.AddWithValue("@BuildNumber", PromBuildNumber);
                sqlComm.Parameters.AddWithValue("@PromotionNbr", PromotionNbr);
                sqlComm.Parameters.AddWithValue("@PromotionDate", PromotionDate);
                sqlComm.Parameters.AddWithValue("@PromotionStatus", PromotionStatus);
                sqlComm.Parameters.AddWithValue("@Duration", PromDuration);
                sqlComm.Parameters.AddWithValue("@PromotionCreator", PromotionCreator);
                sqlConn.Open();
                sqlComm.ExecuteNonQuery();
                sqlConn.Close();
            }   
        }
    }
}