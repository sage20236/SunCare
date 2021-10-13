using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Models
{
    public class Urinate
    {
        public int PatientID { get; set; }
        public int RecordID { get; set; }
        public int AnswerID { get; set; }
        public string Col { get; set; }
        public string Value { get; set; }
    }
}