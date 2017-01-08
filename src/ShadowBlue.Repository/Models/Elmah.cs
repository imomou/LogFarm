﻿using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace ShadowBlue.Repository.Models
{
    [DynamoDBTable("Elmah")]
    public class ElmahError 
    {
        [DynamoDBHashKey]
        public string DateTimeId { get; set; }

        public DateTime TimeUtc { get; set; }

        public string ApplicationName { get; set; }
        public string Host { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
        public string User { get; set; }
        public int StatusCode { get; set; }
        public int Sequence { get; set; }
        public Dictionary<string, string> QueryString { get; set; }
        public Dictionary<string, string> Cookies { get; set; }
        public Dictionary<string, string> ServerVariables { get; set; }
        public Dictionary<string, string> Form { get; set; }
    }
}