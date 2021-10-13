using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Models
{
    public class NewPatient
    {
        public int PatientID { get; set; }
        public string CaseID { get; set; }
        public string Name { get; set; }
        public Nullable<int> Gender { get; set; }
        public Nullable<int> Abnormal { get; set; }
    }
}