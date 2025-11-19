using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace FileManagementService
{
    public class clsLogging
    {
        private static string logDirectory;

        private static string logFilePath;

        public static string EventViewerSource = "File Management Service";
        
        public static void Initialize()
        {
            logDirectory = ConfigurationManager.AppSettings["LogDirectory"];


            // Validate and create directory if it doesn't exist
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ConfigurationErrorsException("LogDirectory is not specified in the configuration file.");
            }

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            logFilePath = Path.Combine(logDirectory, "ServiceStateLog.txt");

            if (!EventLog.SourceExists(EventViewerSource))
            {
                EventLog.CreateEventSource(EventViewerSource, "Application");
                //Console.WriteLine("Event source created.");
            }



        }

        public static string LogServiceEvent(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(logFilePath, logMessage);

            // Write to console if running interactively
            if (Environment.UserInteractive)
            {
                Console.WriteLine(logMessage);
            }
            return logMessage;
        }



    }

    public class FileMonitor
    {

        public FileSystemWatcher watcher1 = new FileSystemWatcher(ConfigurationManager.AppSettings["SourceFolder"], "*.txt");
        public FileMonitor()
        {

                watcher1.NotifyFilter = NotifyFilters.Attributes
                                     | NotifyFilters.CreationTime
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.FileName
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.Security
                                     | NotifyFilters.Size;
                /*
            //watcher.Changed += OnChanged;
            //watcher1.Created += OnCreated;
            //watcher.Deleted += OnDeleted;
            //watcher.Renamed += OnRenamed;
            //watcher.Error += OnError;
            */

                watcher1.Filter = "*.txt";
                watcher1.IncludeSubdirectories = true;
                watcher1.EnableRaisingEvents = true;
            

        }

    }

    public class FileProcessor
    {
        StringBuilder strBLogMessage = new StringBuilder();
        public void Subscribe(FileMonitor fileMonitor)
        {
            
            fileMonitor.watcher1.Created += ProcessNewFileOnCreated;
        }

        private void ProcessNewFileOnCreated(object sender, FileSystemEventArgs e)
        {

            //Logging that a new file is detected
            strBLogMessage.AppendLine(clsLogging.LogServiceEvent($"File Detected at {e.FullPath}: named {e.Name} was added"));

            EventLog.WriteEntry(clsLogging.EventViewerSource, strBLogMessage.ToString(), EventLogEntryType.Information);


            //this method is moving and renaming the new file so we don't need to delete the file
            MoveNewFileToDestinationFolder(e);

            
            //DeleteFromSourceFolder(e);
        }

        private void MoveNewFileToDestinationFolder(FileSystemEventArgs e)
        {
            DirectoryInfo DestinationdirectoryInfo = new DirectoryInfo(ConfigurationManager.AppSettings["DestinationFolder"]);

            Guid NewFileNameInGuid = Guid.NewGuid();

            if (!Directory.Exists(ConfigurationManager.AppSettings["DestinationFolder"]))
            {
                DestinationdirectoryInfo = Directory.CreateDirectory(ConfigurationManager.AppSettings["DestinationFolder"]);
            }



            try
            {
                File.Move(e.FullPath, DestinationdirectoryInfo.FullName + "\\" + NewFileNameInGuid + Path.GetExtension(e.FullPath));

                strBLogMessage.AppendLine(clsLogging.LogServiceEvent($"File moved: {e.FullPath} --> " +
                    $"{DestinationdirectoryInfo.FullName + $"\\{NewFileNameInGuid}" + Path.GetExtension(e.FullPath)}"));

                EventLog.WriteEntry(clsLogging.EventViewerSource, strBLogMessage.ToString(), EventLogEntryType.Information);


            }
            catch (Exception ex)
            {
                strBLogMessage.AppendLine(clsLogging.LogServiceEvent(ex.Message));
                EventLog.WriteEntry(clsLogging.EventViewerSource, strBLogMessage.ToString(), EventLogEntryType.Error);

            }




        }

        private void DeleteFromSourceFolder(FileSystemEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.FullPath))
            {
                throw new ConfigurationErrorsException("File path is null or white space.");
            }

            try
            {
                File.Delete(e.FullPath);
                clsLogging.LogServiceEvent($"File deleted from source path {e.FullPath}");

            }
            catch (Exception ex)
            {
                clsLogging.LogServiceEvent(ex.Message);
                throw;
            }

            
        }

        
    }

    
    public partial class FileManagementService : ServiceBase
    {
        FileMonitor fileMonitor = new FileMonitor();
        FileProcessor fileProcessor = new FileProcessor();

        string LogMessage;

        public FileManagementService()
        {
            InitializeComponent();
            clsLogging.Initialize();
            fileProcessor.Subscribe(fileMonitor);
        }

        
        protected override void OnStart(string[] args)
        {
            LogMessage = clsLogging.LogServiceEvent("Service Started");
            EventLog.WriteEntry(clsLogging.EventViewerSource, LogMessage, EventLogEntryType.Information);

            


        }

        protected override void OnStop()
        {
            LogMessage = clsLogging.LogServiceEvent("Service Stopped");
            EventLog.WriteEntry(clsLogging.EventViewerSource, LogMessage, EventLogEntryType.Information);
        }

        public void StartInConsole()
        {
            Console.WriteLine("Service Started in Console Mode");
            OnStart(null);
            Console.WriteLine("Press any key to stop service.........");
            Console.ReadLine();
            OnStop();

        }





    }







}
