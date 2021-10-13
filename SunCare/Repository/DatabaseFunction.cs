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
            List<RecordDetail> orderTimeRecords = db.RecordDetails.Where(x => x.Record.PatientID == patientID && (x.QuestionID == 1 || x.QuestionID == 5 || x.QuestionID == 8 || x.QuestionID == 14 || x.QuestionID == 17 || x.QuestionID == 21 || x.QuestionID == 24 || x.QuestionID == 27 || x.QuestionID == 30 || x.QuestionID == 36)).OrderByDescending(x => x.TextAnswer).ToList();
            List<AbnormalRecord> abnormalRecords = DatabaseFunction.GetAbnormalRecord();


            for (int i = 0; i < orderTimeRecords.Count; i++)
            {
                var recordIdA = orderTimeRecords[i].RecordID;
                var ansIdA = orderTimeRecords[i].AnswerID;
                var abnStatus = orderTimeRecords[i].Abnormal;

                for (int j = 0; j < abnormalRecords.Count; j++)
                {
                    var recordIdB = abnormalRecords[j].RecordID;
                    var ansIdB = abnormalRecords[j].AnswerID;
                    if (recordIdA.Equals(recordIdB))
                    {
                        orderTimeRecords[i].Abnormal = 1;
                    }
                    else
                    {
                        orderTimeRecords[i].Abnormal = 0;
                    }
                }
                var abnStatus2 = abnStatus;
            }
            return orderTimeRecords;
        }
        public static List<Record> orderbyRecordByTime(List<Record> records)
        {
            List<Record> orderTimeRecords = records.OrderByDescending(y => y.RecordDetails.Where(x => x.QuestionID == 1 || x.QuestionID == 5 || x.QuestionID == 8 || x.QuestionID == 14 || x.QuestionID == 17 || x.QuestionID == 21 || x.QuestionID == 24 || x.QuestionID == 27 || x.QuestionID == 30 || x.QuestionID == 36).FirstOrDefault().TextAnswer).ToList();

            return orderTimeRecords;
        }
        public static List<NewPatient> getPatients(int gender = -1)
        {
            SunCareEntities db = new SunCareEntities();
            List<Patient> patientList = db.Patients.ToList();
            List<PatientThreshold> patientThresholdList = db.PatientThresholds.ToList();
            List<AbnormalRecord> abnormalRecords = DatabaseFunction.GetAbnormalRecord();

            List<NewPatient> newPatients = new List<NewPatient>();
            for (int i = 0; i < patientList.Count; i++)
            {
                var patientId = patientList[i].PatientID;
                var caseId = patientList[i].CaseID;
                var name = patientList[i].Name;
                var sex = patientList[i].Gender;
                var status = 0; // 0 is not-abnormal , 1 is abnormal

                for (int j = 0; j < abnormalRecords.Count; j++)
                {
                    var abnPatientId = abnormalRecords[j].PatientID;
                    if (patientId.Equals(abnPatientId))
                    {
                        status = 1;
                    }
                }

                NewPatient newPatient = new NewPatient();
                newPatient.PatientID = patientId;
                newPatient.CaseID = caseId;
                newPatient.Name = name;
                newPatient.Gender = sex;
                newPatient.Abnormal = status;
                newPatients.Add(newPatient);
            }


            if (gender == -1)
            {
                return newPatients.ToList();
            }
            return newPatients.Where(x => x.Gender == gender).ToList();



            /*
            SunCareEntities db = new SunCareEntities();
            List<Patient> patientList = db.Patients.ToList();
            if (gender == -1)
            {
                return db.Patients.ToList();
            }
            return db.Patients.Where(x => x.Gender == gender).ToList();
            */
        }

        public static List<AbnormalRecord> GetAbnormalRecord()
        {
            SunCareEntities db = new SunCareEntities();
            List<Patient> patientList = db.Patients.ToList();
            List<PatientThreshold> patientThresholdList = db.PatientThresholds.ToList();
            List<AbnormalRecord> abnormalRecords = new List<AbnormalRecord>();

            // --------------- 取得服務項目紀錄資料 ---------------
            var start_time = DateTime.Now.Date;
            var end_time = DateTime.Now.AddDays(1).Date;
            var query = from rd in db.RecordDetails
                        join r in db.Records on rd.RecordID equals r.RecordID into result1
                        from rd_r in result1.DefaultIfEmpty()

                        join rdo in db.RecordDetailsOptions on rd.AnswerID equals rdo.AnswerID into result2
                        from rd_rdo in result2.DefaultIfEmpty()

                        join qo in db.QuestionOptions on rd_rdo.OptionID equals qo.OptionID into result3
                        from rdo_qo in result3.DefaultIfEmpty()

                        where rd_r.CreateTime > start_time && rd_r.CreateTime <= end_time
                        select new
                        {

                            RecordID = rd.RecordID,
                            CreateTime = rd_r.CreateTime,
                            PatientID = rd_r.PatientID,
                            ServiceID = rd_r.ServiceID,
                            QuestionID = rd.QuestionID,
                            AnswerID = rd.AnswerID,
                            OptionID = (rd_rdo == null || rd_rdo.OptionID == null ? -1 : rd_rdo.OptionID),
                            TextAnswer = (rd.TextAnswer == null ? "-1" : rd.TextAnswer),
                        };
            // --------------- 取得服務項目紀錄資料 ---------------

            var index = db.Records.ToList();

            #region ----------- 用藥紀錄 -----------
            List<Medicine> medicines = new List<Medicine>();

            foreach (var id in index)
            {
                var patientId = id.PatientID;
                var recordId = id.RecordID;
                var ansId = -1;
                var period = -1;
                var amount = "null";

                foreach (var record in query)
                {
                    var r = record.RecordID;
                    if (recordId.Equals(r))
                    {
                        var recordQuestionId = record.QuestionID;
                        if (recordQuestionId.Equals(2))
                        {
                            period = record.OptionID;
                        }
                        else if (recordQuestionId.Equals(3))
                        {
                            ansId = record.AnswerID;
                            amount = record.TextAnswer;
                        }
                    }
                }

                if (period != -1 && !amount.Equals(null))
                {
                    Medicine medicine = new Medicine();
                    medicine.PatientID = patientId;
                    medicine.RecordID = recordId;
                    medicine.AnswerID = ansId;
                    medicine.Period = period;
                    medicine.Amount = amount;
                    medicines.Add(medicine);
                }
            }
            #endregion

            #region ----------- 大便紀錄 -----------
            List<Stool> stools = new List<Stool>();
            foreach (var id in index)
            {
                var patientId = id.PatientID;
                var recordId = id.RecordID;
                var ansId = -1;
                var col = "null";
                var value = "null";

                foreach (var record in query)
                {
                    var r = record.RecordID;
                    if (recordId.Equals(r))
                    {
                        var recordQuestionId = record.QuestionID;
                        ansId = record.AnswerID;
                        if (recordQuestionId.Equals(9))
                        {
                            col = "style";
                        }
                        else if (recordQuestionId.Equals(40))
                        {
                            col = "color";
                        }
                        value = record.OptionID.ToString();
                    }
                }



                if (!col.Equals("null") && !value.Equals("null"))
                {
                    Stool stool = new Stool();
                    stool.PatientID = patientId;
                    stool.RecordID = recordId;
                    stool.AnswerID = ansId;
                    stool.Col = col;
                    stool.Value = value;
                    stools.Add(stool);
                }
            }
            #endregion

            #region ----------- 小便紀錄 -----------
            List<Urinate> urinates = new List<Urinate>();
            foreach (var id in index)
            {
                var patientId = id.PatientID;
                var recordId = id.RecordID;
                var ansId = -1;
                var col = "null";
                var value = "null";

                foreach (var record in query)
                {
                    var r = record.RecordID;
                    if (recordId.Equals(r))
                    {
                        var recordQuestionId = record.QuestionID;
                        ansId = record.AnswerID;
                        if (recordQuestionId.Equals(43))
                        {
                            col = "color";
                        }
                        else if (recordQuestionId.Equals(15))
                        {
                            col = "volume";
                        }
                        value = record.OptionID.ToString();
                    }
                }

                if (!col.Equals("null") && !value.Equals("null"))
                {
                    Urinate urinate = new Urinate();
                    urinate.PatientID = id.PatientID;
                    urinate.RecordID = recordId;
                    urinate.AnswerID = ansId;
                    urinate.Col = col;
                    urinate.Value = value;
                    urinates.Add(urinate);
                }
            }
            #endregion

            #region ----------- 量測紀錄 -----------
            List<Messure> messures = new List<Messure>();
            foreach (var id in index)
            {
                var patientId = id.PatientID;
                var recordId = id.RecordID;
                var ansId = -1;
                var time = "null";
                var col = "null";
                var value = "null";

                foreach (var record in query)
                {
                    var r = record.RecordID;
                    if (recordId.Equals(r))
                    {
                        var recordQuestionId = record.QuestionID;
                        ansId = record.AnswerID;
                        if (recordQuestionId.Equals(30))
                        {
                            time = record.TextAnswer;
                        }
                        else if (recordQuestionId.Equals(31))
                        {
                            col = "temperature";
                        }
                        else if (recordQuestionId.Equals(32))
                        {
                            col = "sysBp";
                        }
                        else if (recordQuestionId.Equals(33))
                        {
                            col = "diaBp";
                        }
                        else if (recordQuestionId.Equals(34))
                        {
                            col = "hr";
                        }
                        else if (recordQuestionId.Equals(52))
                        {
                            col = "weight";
                        }
                        value = record.TextAnswer;
                    }

                    if (!col.Equals("null") && !value.Equals("null"))
                    {
                        Messure messure = new Messure();
                        messure.PatientID = patientId;
                        messure.RecordID = recordId;
                        messure.AnswerID = ansId;
                        messure.Col = col;
                        messure.Time = time;
                        messure.Value = value;
                        messures.Add(messure);
                    }
                }

            }
            #endregion


            // 項目紀錄與閾值檢查

            for (int i = 0; i < patientThresholdList.Count; i++)
            {
                var tresholdPatientId = patientThresholdList[i].PatientID;
                var tresholdServiceId = patientThresholdList[i].ServiceID;
                var tresholdQuestionId = patientThresholdList[i].QuestionID;
                var tresholdOptionId = patientThresholdList[i].OptionID;
                var tresholdValue = patientThresholdList[i].ThresholdValue;

                switch (tresholdServiceId)
                {
                    case 2:
                        #region 用藥紀錄檢查
                        var medicinesNum = medicines.Count;
                        if (medicinesNum > 0)
                        {
                            for (int j = 0; j < medicinesNum; j++)
                            {
                                var patientId = medicines[j].PatientID;
                                if (tresholdPatientId.Equals(patientId))
                                {
                                    var period = medicines[j].Period;
                                    if (tresholdOptionId.Equals(period))
                                    {
                                        var value = medicines[j].Amount;
                                        if (tresholdValue.Equals(value))
                                        {
                                            Console.WriteLine("PASS");
                                        }
                                        else
                                        {
                                            AbnormalRecord abnormalRecord = new AbnormalRecord();
                                            abnormalRecord.PatientID = medicines[j].PatientID;
                                            abnormalRecord.RecordID = medicines[j].RecordID;
                                            abnormalRecord.AnswerID = medicines[j].AnswerID;
                                            abnormalRecord.Value = value;
                                            abnormalRecord.Threshold = tresholdValue;
                                            abnormalRecords.Add(abnormalRecord);
                                            Console.WriteLine("No PASS");
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    case 4:
                        #region 大便紀錄檢查
                        var stoolNum = stools.Count;
                        if (stoolNum > 0)
                        {
                            for (int j = 0; j < stoolNum; j++)
                            {
                                var patientId = stools[j].PatientID;
                                if (tresholdPatientId.Equals(patientId))
                                {
                                    if (tresholdQuestionId.Equals(0))
                                    {

                                    }
                                    else if (tresholdQuestionId.Equals(9))
                                    {
                                        if (stools[j].Col.Equals("style"))
                                        {
                                            var style = stools[j].Value;
                                            if (tresholdValue.Equals(style))
                                            {
                                                Console.WriteLine("PASS");
                                            }
                                            else
                                            {
                                                AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                abnormalRecord.PatientID = stools[j].PatientID;
                                                abnormalRecord.RecordID = stools[j].RecordID;
                                                abnormalRecord.AnswerID = stools[j].AnswerID;
                                                abnormalRecord.Value = style.ToString();
                                                abnormalRecord.Threshold = tresholdValue;
                                                abnormalRecords.Add(abnormalRecord);
                                                Console.WriteLine("No PASS");
                                            }
                                        }
                                    }
                                    else if (tresholdQuestionId.Equals(40))
                                    {
                                        if (stools[j].Col.Equals("color"))
                                        {
                                            var color = stools[j].Value;
                                            if (tresholdValue.Equals(color))
                                            {
                                                Console.WriteLine("PASS");
                                            }
                                            else
                                            {
                                                AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                abnormalRecord.PatientID = stools[j].PatientID;
                                                abnormalRecord.RecordID = stools[j].RecordID;
                                                abnormalRecord.AnswerID = stools[j].AnswerID;
                                                abnormalRecord.Value = color.ToString();
                                                abnormalRecord.Threshold = tresholdValue;
                                                abnormalRecords.Add(abnormalRecord);
                                                Console.WriteLine("No PASS");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    case 5:
                        #region 小便紀錄檢查
                        var urinateNum = urinates.Count;
                        if (urinateNum > 0)
                        {
                            for (int j = 0; j < urinateNum; j++)
                            {
                                var patientId = urinates[j].PatientID;
                                if (tresholdPatientId.Equals(patientId))
                                {
                                    if (tresholdQuestionId.Equals(15))
                                    {
                                        if (urinates[j].Col.Equals("volume"))
                                        {
                                            var volume = urinates[j].Value;
                                            if (int.Parse(volume) > int.Parse(tresholdValue.ToString()))
                                            {
                                                Console.WriteLine("PASS");
                                            }
                                            else
                                            {
                                                AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                abnormalRecord.PatientID = urinates[j].PatientID;
                                                abnormalRecord.RecordID = urinates[j].RecordID;
                                                abnormalRecord.AnswerID = urinates[j].AnswerID;
                                                abnormalRecord.Value = volume;
                                                abnormalRecord.Threshold = tresholdValue;
                                                abnormalRecords.Add(abnormalRecord);
                                                Console.WriteLine("No PASS");
                                            }
                                        }
                                    }
                                    else if (tresholdQuestionId.Equals(43))
                                    {
                                        if (urinates[j].Col.Equals("color"))
                                        {
                                            var color = urinates[j].Value;
                                            if (tresholdValue.Equals(color))
                                            {
                                                Console.WriteLine("PASS");
                                            }
                                            else
                                            {
                                                AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                abnormalRecord.PatientID = urinates[j].PatientID;
                                                abnormalRecord.RecordID = urinates[j].RecordID;
                                                abnormalRecord.AnswerID = urinates[j].AnswerID;
                                                abnormalRecord.Value = color.ToString();
                                                abnormalRecord.Threshold = tresholdValue;
                                                abnormalRecords.Add(abnormalRecord);
                                                Console.WriteLine("No PASS");
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        #endregion
                        break;
                    case 10:
                        #region 量測紀錄檢查
                        var messureNum = messures.Count;
                        if (messureNum > 0)
                        {
                            for (int j = 0; j < messureNum; j++)
                            {
                                var patientId = messures[j].PatientID;
                                if (tresholdPatientId.Equals(patientId))
                                {
                                    if (tresholdQuestionId.Equals(30))
                                    {
                                        if (tresholdOptionId.Equals(1))
                                        {
                                            // 開始時間
                                            var time = messures[j].Time;
                                            string[] suntemp1 = time.Split(':');
                                            string[] suntemp2 = tresholdValue.Split(':');
                                            int date1 = int.Parse(suntemp1[0] + suntemp1[1]);
                                            int date2 = int.Parse(suntemp2[0] + suntemp2[1]);
                                            if (date1 > date2)
                                            {
                                                Console.WriteLine("PASS");
                                            }
                                            else
                                            {
                                                AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                abnormalRecord.PatientID = messures[j].PatientID;
                                                abnormalRecord.RecordID = messures[j].RecordID;
                                                abnormalRecord.AnswerID = messures[j].AnswerID;
                                                abnormalRecord.Value = time;
                                                abnormalRecord.Threshold = tresholdValue;
                                                abnormalRecords.Add(abnormalRecord);
                                                Console.WriteLine("No PASS");
                                            }

                                        }
                                        if (tresholdOptionId.Equals(2))
                                        {
                                            // 結束時間
                                            var time = messures[j].Time;
                                            string[] suntemp1 = time.Split(':');
                                            string[] suntemp2 = tresholdValue.Split(':');
                                            int date1 = int.Parse(suntemp1[0] + suntemp1[1]);
                                            int date2 = int.Parse(suntemp2[0] + suntemp2[1]);
                                            if (date1 < date2)
                                            {
                                                Console.WriteLine("PASS");
                                            }
                                            else
                                            {
                                                AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                abnormalRecord.PatientID = messures[j].PatientID;
                                                abnormalRecord.RecordID = messures[j].RecordID;
                                                abnormalRecord.AnswerID = messures[j].AnswerID;
                                                abnormalRecord.Value = time;
                                                abnormalRecord.Threshold = tresholdValue;
                                                abnormalRecords.Add(abnormalRecord);
                                                Console.WriteLine("No PASS");
                                            }
                                        }
                                    }
                                    else if (tresholdQuestionId.Equals(31))
                                    {
                                        // 體溫
                                        if (tresholdOptionId.Equals(1))
                                        {
                                            if (messures[j].Col.Equals("temperature"))
                                            {
                                                int threshold = int.Parse(tresholdValue);
                                                int range = int.Parse(patientThresholdList[i + 1].ThresholdValue);
                                                int value = int.Parse(messures[j].Value);
                                                if (value >= (threshold - range) && value <= (threshold + range))
                                                {
                                                    Console.WriteLine("PASS");
                                                }
                                                else
                                                {
                                                    AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                    abnormalRecord.PatientID = messures[j].PatientID;
                                                    abnormalRecord.RecordID = messures[j].RecordID;
                                                    abnormalRecord.AnswerID = messures[j].AnswerID;
                                                    abnormalRecord.Value = value.ToString();
                                                    abnormalRecord.Threshold = (threshold - range) + "," + (threshold + range);
                                                    abnormalRecords.Add(abnormalRecord);
                                                    Console.WriteLine("No PASS");
                                                }
                                            }
                                        }
                                    }
                                    else if (tresholdQuestionId.Equals(32))
                                    {
                                        // 收縮壓
                                        if (tresholdOptionId.Equals(1))
                                        {
                                            if (messures[j].Col.Equals("sysBp"))
                                            {
                                                int threshold = int.Parse(tresholdValue);
                                                int range = int.Parse(patientThresholdList[i + 1].ThresholdValue);
                                                int value = int.Parse(messures[j].Value);
                                                if (value >= (threshold - range) && value <= (threshold + range))
                                                {
                                                    Console.WriteLine("PASS");
                                                }
                                                else
                                                {
                                                    AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                    abnormalRecord.PatientID = messures[j].PatientID;
                                                    abnormalRecord.RecordID = messures[j].RecordID;
                                                    abnormalRecord.AnswerID = messures[j].AnswerID;
                                                    abnormalRecord.Value = value.ToString();
                                                    abnormalRecord.Threshold = (threshold - range) + "," + (threshold + range);
                                                    abnormalRecords.Add(abnormalRecord);
                                                    Console.WriteLine("No PASS");
                                                }
                                            }
                                        }
                                    }
                                    else if (tresholdQuestionId.Equals(33))
                                    {
                                        // 舒張壓
                                        if (tresholdOptionId.Equals(1))
                                        {
                                            if (messures[j].Col.Equals("diaBp"))
                                            {
                                                int threshold = int.Parse(tresholdValue);
                                                int range = int.Parse(patientThresholdList[i + 1].ThresholdValue);
                                                int value = int.Parse(messures[j].Value);
                                                if (value >= (threshold - range) && value <= (threshold + range))
                                                {
                                                    Console.WriteLine("PASS");
                                                }
                                                else
                                                {
                                                    AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                    abnormalRecord.PatientID = messures[j].PatientID;
                                                    abnormalRecord.RecordID = messures[j].RecordID;
                                                    abnormalRecord.AnswerID = messures[j].AnswerID;
                                                    abnormalRecord.Value = value.ToString();
                                                    abnormalRecord.Threshold = (threshold - range) + "," + (threshold + range);
                                                    abnormalRecords.Add(abnormalRecord);
                                                    Console.WriteLine("No PASS");
                                                }
                                            }
                                        }
                                    }
                                    else if (tresholdQuestionId.Equals(34))
                                    {
                                        // 心率
                                        if (tresholdOptionId.Equals(1))
                                        {
                                            if (messures[j].Col.Equals("hr"))
                                            {
                                                int threshold = int.Parse(tresholdValue);
                                                int range = int.Parse(patientThresholdList[i + 1].ThresholdValue);
                                                int value = int.Parse(messures[j].Value);
                                                if (value >= (threshold - range) && value <= (threshold + range))
                                                {
                                                    Console.WriteLine("PASS");
                                                }
                                                else
                                                {
                                                    AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                    abnormalRecord.PatientID = messures[j].PatientID;
                                                    abnormalRecord.RecordID = messures[j].RecordID;
                                                    abnormalRecord.AnswerID = messures[j].AnswerID;
                                                    abnormalRecord.Value = value.ToString();
                                                    abnormalRecord.Threshold = (threshold - range) + "," + (threshold + range);
                                                    abnormalRecords.Add(abnormalRecord);
                                                    Console.WriteLine("No PASS");
                                                }
                                            }
                                        }
                                    }
                                    else if (tresholdQuestionId.Equals(52))
                                    {
                                        // 體重
                                        if (tresholdOptionId.Equals(1))
                                        {
                                            if (messures[j].Col.Equals("weight"))
                                            {
                                                int threshold = int.Parse(tresholdValue);
                                                int range = int.Parse(patientThresholdList[i + 1].ThresholdValue);
                                                int value = int.Parse(messures[j].Value);
                                                if (value >= (threshold - range) && value <= (threshold + range))
                                                {
                                                    Console.WriteLine("PASS");
                                                }
                                                else
                                                {
                                                    AbnormalRecord abnormalRecord = new AbnormalRecord();
                                                    abnormalRecord.PatientID = messures[j].PatientID;
                                                    abnormalRecord.RecordID = messures[j].RecordID;
                                                    abnormalRecord.AnswerID = messures[j].AnswerID;
                                                    abnormalRecord.Value = value.ToString();
                                                    abnormalRecord.Threshold = (threshold - range) + "," + (threshold + range);
                                                    abnormalRecords.Add(abnormalRecord);
                                                    Console.WriteLine("No PASS");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                }
            }

            return abnormalRecords.ToList();
        }

        public static List<NewPatient> getThreshold(int abnormal = -1)
        {
            SunCareEntities db = new SunCareEntities();
            List<Patient> patientList = db.Patients.ToList();
            List<PatientThreshold> patientThresholdList = db.PatientThresholds.ToList();
            List<AbnormalRecord> abnormalRecords = DatabaseFunction.GetAbnormalRecord();

            List<NewPatient> newPatients = new List<NewPatient>();
            for (int i = 0; i < patientList.Count; i++)
            {
                var patientId = patientList[i].PatientID;
                var caseId = patientList[i].CaseID;
                var name = patientList[i].Name;
                var gender = patientList[i].Gender;
                var status = 0; // 0 is not-abnormal , 1 is abnormal

                for (int j = 0; j < abnormalRecords.Count; j++)
                {
                    var abnPatientId = abnormalRecords[j].PatientID;
                    if (patientId.Equals(abnPatientId))
                    {
                        status = 1;
                    }
                }

                NewPatient newPatient = new NewPatient();
                newPatient.PatientID = patientId;
                newPatient.CaseID = caseId;
                newPatient.Name = name;
                newPatient.Gender = gender;
                newPatient.Abnormal = status;
                newPatients.Add(newPatient);
            }



            if (abnormal == -1)
            {
                return newPatients.ToList();
            }
            return newPatients.Where(x => x.Abnormal == abnormal).ToList();
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