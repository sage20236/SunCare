using SunCare.Models;
using SunCare.Repository;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SunCare.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(String account, String password)
        {
            Employee ee = DatabaseFunction.checkLogin(account, password);
            String msg = "";
            if (ee == null)
            {
                msg = "查無資料";
            }
            else
            {
                FormsAuthentication.SetAuthCookie(ee.EmployeeID.ToString() + "|" + ee.Name, true);
                return RedirectToAction("Index");
            }
            ViewData["msg"] = msg;
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult gaku()
        {

            return View();
        }
        [Authorize]
        public ActionResult Index()
        {
            ViewData["allItems"] = DatabaseFunction.getAllServices();
            return View();
        }



        #region 個案管理
        [Authorize]
        public ActionResult CaseInfo()
        {
            ViewData["allItems"] = DatabaseFunction.getAllServices();
            //ViewData["patients"] = DatabaseFunction.getPatients();
            return View();
        }

        public ActionResult CasePartialPatient(int gender = -1)
        {
            return PartialView(DatabaseFunction.getPatients(gender));
        }

        public ActionResult CaseAbnormalPatient(int abnormal = -1)
        {
            return PartialView(DatabaseFunction.getThreshold(abnormal));
        }

        [Authorize]
        public ActionResult CaseEachInfo(String patientID, int orderby = 1)
        {
            if (patientID == null || patientID == "")
            {
                return RedirectToAction("CaseInfo");
            }
            int patientIDInt = 0;
            if (!int.TryParse(RegularFunction.decrypt(patientID), out patientIDInt))
            {
                return RedirectToAction("CaseInfo");
            }
            //0=time  1=category
            ViewData["orderby"] = orderby;
            ViewData["patientID"] = patientIDInt;
            ViewData["allItems"] = DatabaseFunction.getAllServices();
            ViewData["recordDetails"] = DatabaseFunction.getAllRecordsOrderTime(patientIDInt);
            ViewData["patients"] = DatabaseFunction.getPatients();
            return View();
        }
        public ActionResult InsertRecord(Record record)
        {
            record.CreateTime = DateTime.Now;
            DatabaseFunction.insertRecord(record);
            return Json(new { });
        }
        public ActionResult EditRecord(Record record)
        {
            record.CreateTime = DateTime.Now;
            DatabaseFunction.updateRecord(record);

            return Json(new { });
        }
        public ActionResult DeleteRecord(int recordID = -1)
        {
            if (recordID == -1)
            {
                return Json(new { });
            }
            DatabaseFunction.deleteRecord(recordID);
            return Json(new { });
        }
        #endregion
    }
}