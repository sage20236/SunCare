using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Models
{
    public class AbnormalPatient
    {

        public string PatientName { get; set; }
        public int PatientID { get; set; }
        public int ThresholdTag { get; set; }
        public string ThresholdValue { get; set; }
    }
}