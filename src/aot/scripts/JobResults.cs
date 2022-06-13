using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelixResults
{

    public class TopLevelResult
    {
        public ResultDetails[] Results { get; set; }
    }

    public class ResultDetails
    {
        public string DetailsUrl { get; set; }
        public string Job { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
    }

    //public class JobResults
    //{
    //    public DateTime Queued { get; set; }
    //    public DateTime Started { get; set; }
    //    public DateTime Finished { get; set; }
    //    public string Delay { get; set; }
    //    public string Duration { get; set; }
    //    public string Id { get; set; }
    //    public int ExitCode { get; set; }
    //    public object[] Errors { get; set; }
    //    public object[] Warnings { get; set; }
    //    public object[] Logs { get; set; }
    //    public object[] Files { get; set; }
    //    public string Job { get; set; }
    //    public string Name { get; set; }
    //    public string State { get; set; }
    //}


    public class JobResults
    {
        public DateTime Queued { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public string Delay { get; set; }
        public string Duration { get; set; }
        public string Id { get; set; }
        public string MachineName { get; set; }
        public int ExitCode { get; set; }
        public string ConsoleOutputUri { get; set; }
        public object[] Errors { get; set; }
        public object[] Warnings { get; set; }
        public Log_Result[] Logs { get; set; }
        public File_Result[] Files { get; set; }
        public string Job { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
    }

    public class Log_Result
    {
        public string Log_Module { get; set; }
        public string Log_Uri { get; set; }
    }

    public class File_Result
    {
        public string File_FileName { get; set; }
        public string File_Uri { get; set; }
    }


    //public class ResultFileInfo
    //{
    //    public string Result_FileName { get; set; }
    //    public string Result_Uri { get; set; }
    //}


}
