using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Models
{
    public class AbnormalRecord
    {
        public int PatientID { get; set; }
        public int RecordID { get; set; }
        public int AnswerID { get; set; }
        public string Value { get; set; }
        public string Threshold { get; set; }
    }
}