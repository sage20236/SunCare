using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Models
{
    public class Medicine
    {
        public int PatientID { get; set; }
        public int RecordID { get; set; }
        public int AnswerID { get; set; }
        public int Period { get; set; }
        public string Amount { get; set; }
    }
}