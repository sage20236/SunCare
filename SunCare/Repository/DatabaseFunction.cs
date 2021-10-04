using SunCare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Repository
{
    public class DatabaseFunction
    {

        public static Employee checkLogin(String account, String password) 
        {
            SunCareEntities db = new SunCareEntities();
            return db.Employees.Where(x => x.Account == account.Trim() && x.Password == password.Trim()).FirstOrDefault();
        }

        /*public static Employee checkLogin2(int realID, String name)
       {

           SunCareEntities db = new SunCareEntities();
           Employee findEmployee = db.Employees.Where(x => x.id == realID).FirstOrDefault();
           //==null return null
           //!=null 
           findEmployee.Name = name;

           db.SaveChanges();
           return findEmployee;
          
    } */

        public static List<ServiceItem> getAllServices()
        {
            SunCareEntities db = new SunCareEntities();
            return db.ServiceItems.ToList(); 
        }
        public static List<RecordDetail> getAllRecordsOrderTime(int patientID)
        {
            SunCareEntities db = new SunCareEntities();
            return db.RecordDetails.Where(x => x.Record.PatientID == patientID  && (x.QuestionID == 1 || x.QuestionID == 5 || x.QuestionID == 8 || x.QuestionID == 14 || x.QuestionID == 17 || x.QuestionID == 21 || x.QuestionID == 24 || x.QuestionID == 27 || x.QuestionID == 30 || x.QuestionID == 36)).OrderByDescending(x=>x.TextAnswer).ToList();
        }
        public static List<Record> orderbyRecordByTime(List<Record> records)
        {
            return records.OrderByDescending(y=>y.RecordDetails.Where(x=>x.QuestionID == 1 || x.QuestionID == 5 || x.QuestionID == 8 || x.QuestionID == 14 || x.QuestionID == 17 || x.QuestionID == 21 || x.QuestionID == 24 || x.QuestionID == 27 || x.QuestionID == 30 || x.QuestionID == 36).FirstOrDefault().TextAnswer).ToList();
        }
        public static List<Patient> getPatients(int gender=-1)
        {
            SunCareEntities db = new SunCareEntities();
            if (gender == -1)
            {
                return db.Patients.ToList();
            }
            return db.Patients.Where(x => x.Gender == gender).ToList();
        }

        public static List<PatientThreshold> getAllAbnormalByPatient()
        {
            SunCareEntities db = new SunCareEntities();
            return db.PatientThresholds.ToList();
            //return db.RecordDetails.Where(x => x.Record.PatientID == patientID && (x.QuestionID == 1 || x.QuestionID == 5 || x.QuestionID == 8 || x.QuestionID == 14 || x.QuestionID == 17 || x.QuestionID == 21 || x.QuestionID == 24 || x.QuestionID == 27 || x.QuestionID == 30 || x.QuestionID == 36)).OrderByDescending(x => x.TextAnswer).ToList();
        }

        public static bool insertRecord(Record record)
        {
            bool isSuccess = true;
            SunCareEntities db = new SunCareEntities();
            db.Records.Add(record);
            db.SaveChanges();
            return isSuccess;
        }
        public static void updateRecord(Record record)
        {
            deleteRecord(record.RecordID);
            insertRecord(record);
        }
        public static void deleteRecord(int recordID)
        {
            SunCareEntities db = new SunCareEntities();
            Record orignalRecord = db.Records.Where(x => x.RecordID == recordID).FirstOrDefault();
            foreach (RecordDetail eachDetails in orignalRecord.RecordDetails.ToList())
            {
                foreach (RecordDetailsOption eachOption in eachDetails.RecordDetailsOptions.ToList())
                {
                    db.RecordDetailsOptions.Remove(eachOption);
                }
                db.RecordDetails.Remove(eachDetails);
            }
            db.Records.Remove(orignalRecord);
            db.SaveChanges();
        }
    }
}