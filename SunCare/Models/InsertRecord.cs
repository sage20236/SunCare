using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Models
{
    public class InsertRecord
    {
        public int serviceID { get; set; }
        public int employeeID{ get; set; }
        public int patientID { get; set; }
        public List<QuestionAnswer> questionAnswer { get; set; }
    }
    public class QuestionAnswer
    {
        public int questionID { get; set; }
        public String answer { get; set; }
        public List<int> options{ get; set; }
        
    }
}