using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace schoolproj
{
        public class passcode : TableEntity
        {
            public string code { get; set; }
        }
    public class presence : TableEntity
    {
        public string arrived { get; set; }
        public string left { get; set; }

    }
    public class schedule : TableEntity
    {
        public string startTime { get; set; }
        public string finishTime { get; set; }
    }
    public class students : TableEntity
    {
        public string classID { get; set; }
        public string name { get; set; }
        public string parentID { get; set; }
        public string password { get; set; }

    }
    public class parents : TableEntity
    {
        public string name { get; set; }
        public string password { get; set; }

    }

}
