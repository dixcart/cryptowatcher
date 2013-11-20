using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Xml.Linq;

namespace CryptoWatcher
{
    class Program
    {
        private static string BaselineHash;
        private static List<FileSystemWatcher> Watchers;
        private static XDocument Config;

        static void Main(string[] args)
        {
            string strConfigFile = "CryptoWatcher.xml";

            if (args.Length > 0) strConfigFile = args[0];

            if (!File.Exists(strConfigFile))
            {

                new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XComment("CryptoWatcher Local Config File"),
                    new XElement("root",
                        new XElement("testmode", true),
                        new XElement("email",
                            new XElement("to", "user@example.com"),
                            new XElement("from", "user@example.com"),
                            new XElement("server", "localhost")
                        ),
                        new XElement("folder",
                            new XElement("path", "c:\\data\\something")
                        )
                    )
                )
                .Save(strConfigFile);
                Console.WriteLine(strConfigFile + " not found, blank one created.");
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
                Environment.Exit(1);
            }

            //We know the config file exists, so open and read values
            Config = XDocument.Load(strConfigFile);

            Watchers = new List<FileSystemWatcher>();

            //Store hash of baseline
            using (var md5 = MD5.Create())
            {
                using (Stream baseline = Assembly.GetExecutingAssembly().GetManifestResourceStream("CryptoWatcher.Resources.Baseline.doc"))
                {
                    BaselineHash = BitConverter.ToString(md5.ComputeHash(baseline)).Replace("-", "").ToLower();
                }
            }

            foreach (var folder in Config.Element("root").Descendants("folder"))
            {
                string strPath = folder.Element("path").Value;
                string strTestPath = strPath + "\\DO NOT EDIT THIS DOCUMENT.doc";
                Console.WriteLine("Setting up path: " + strPath);

                //Check if the resource file is there
                if (!File.Exists(strTestPath))
                {
                    File.WriteAllBytes(strTestPath, CryptoWatcher.Properties.Resources.Baseline);
                    Console.WriteLine("Baseline copied to {0}", strTestPath);
                }

                //Now compare the existing with our stored to make sure baseline is a match
                if (BaselineHash != GetHash(strTestPath))
                {
                    //Files already don't match, quit out and suggest deleting
                    Console.WriteLine("Test file {0} doesn't match baseline, delete and allow to be replaced", strTestPath);
                    Environment.Exit(1);
                }

                //Now set up watcher on this file
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(strTestPath);
                watcher.IncludeSubdirectories = false;
                watcher.Filter = Path.GetFileName(strTestPath);
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.Size | NotifyFilters.Security;
                watcher.Changed += watcher_Changed;
                watcher.EnableRaisingEvents = true;
                Watchers.Add(watcher);
            }

            Console.ReadKey(true);


        }

        static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Change detected in {0}", e.FullPath);
            //Compare hashes
            if (Program.BaselineHash != GetHash(e.FullPath))
            {
                Console.WriteLine("WARNING: FILE CHANGE DETECTED, USING PROTECTION");
                string strOwner = File.GetAccessControl(e.FullPath).GetOwner(typeof(NTAccount)).ToString();
                Console.WriteLine("NEW OWNER: {0}", strOwner);
                if (!Convert.ToBoolean(Program.Config.Element("root").Element("testmode").Value))
                {
                    //Shutdown server service
                    ServiceController svcLanman = new ServiceController("LanmanServer");
                    try
                    {
                        svcLanman.Stop();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("UNABLE TO STOP SERVICE: {0}", ex.Message);
                        SendEmail("UNABLE TO STOP SERVICE", ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("TESTMODE ENABLED, NOT SHUTTING DOWN FILES");
                }

                //Send admin email
                SendEmail(Environment.MachineName + " FILE SERVICES SHUTDOWN", "Potential CryptoLocker infection\nComputer: " + Environment.MachineName + "\nOwner: " + strOwner);
            }
        }

        static string GetHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        static void SendEmail(string strSubject, string strMessage)
        {
            MailMessage message = new MailMessage();
            message.To.Add(Program.Config.Element("root").Element("email").Element("to").Value);
            message.Subject = "[CryptoWatcher] " + strSubject;
            message.From = new MailAddress(Program.Config.Element("root").Element("email").Element("from").Value);
            message.Body = strMessage;
            SmtpClient smtp = new SmtpClient(Program.Config.Element("root").Element("email").Element("server").Value);
            smtp.Send(message);
        }

        private string GetSpecificFileProperties(string file, params int[] indexes)
        {
            string fileName = Path.GetFileName(file);
            string folderName = Path.GetDirectoryName(file);
            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder;
            objFolder = shell.NameSpace(folderName);
            StringBuilder sb = new StringBuilder();
            foreach (Shell32.FolderItem2 item in objFolder.Items())
            {
                if (fileName == item.Name)
                {
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        sb.Append(objFolder.GetDetailsOf(item, indexes[i]) + ",");
                    }
                    break;
                }
            }
            string result = sb.ToString().Trim();
            if (result.Length == 0)
            {
                return string.Empty;
            }
            return result.Substring(0, result.Length - 1);
        }
    }
}
