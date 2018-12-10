using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Mvc;
using System.Web.Util;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Diagnostics;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirstMVCApp.Controllers
{ 
    public class RITAPIController : Controller
    {
        // Select Degree Program
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> SelectDegree(string degree)
        {
            var returnedJSON = await GetUndergradDegrees(degree);
            if (returnedJSON == null)
            {
                return ThrowJsonError(new Exception(String.Format("No Degree Programs found.")));
            }
            Session["returnedJSON"] = returnedJSON;
            return null;
        }

        public static async Task<Object> GetUndergradDegrees(string degree)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri("http://www.ist.rit.edu/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                
                    if (degree == "under")
                    {
                        HttpResponseMessage response = await client.GetAsync("api/degrees/undergraduate/", HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();
                        var data = await response.Content.ReadAsStringAsync();
                        var rtnResults = JsonConvert.DeserializeObject(data);
                        return rtnResults;
                    }
                    else if (degree == "grad")
                    {
                        HttpResponseMessage response = await client.GetAsync("api/degrees/graduate/", HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();
                        var data = await response.Content.ReadAsStringAsync();
                        var rtnResults = JsonConvert.DeserializeObject(data);
                        return rtnResults;
                    }
                    else return null;

                }
                catch (HttpRequestException hre)
                {
                    var msg = hre.Message;
                    return "HttpRequestException";
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    return "Exception";
                }
            }
        }

        public ActionResult SelectedDegree()
        {
            if (Session["returnedJSON"] != null)
            {
                return View("SelectDegree");
            }
            return RedirectToAction("Index"); 
        }

        public async Task<JsonResult> GetFaculty()
        {
            var returnedJSON = await GetFacultyA();
            if (returnedJSON == null)
            {
                return ThrowJsonError(new Exception(String.Format("No Faculty found.")));
            }
            Session["returnedJSON"] = returnedJSON;
            return null;
        }

        public static async Task<Object> GetFacultyA()
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri("http://www.ist.rit.edu/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api/people", HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                var rtnResults = JsonConvert.DeserializeObject(data);
                return rtnResults;
            }
        }

        public ActionResult SelectedFaculty()
        {
            if (Session["returnedJSON"] != null)
            {
                return View("SelectedFaculty");
            }
            return RedirectToAction("Index");
        }

        public string GetSelectedFaculty(string facultyName)
        {
            var faculty = Session["returnedJSON"].ToString();
            People people = JToken.Parse(faculty).ToObject<People>();

            Faculty selectedFaculty = new Faculty();

            selectedFaculty = people.faculty.Where(a => a.name == facultyName).ToList().FirstOrDefault();


            //foreach (Faculty thisFac in people.faculty)
            //{
            //    if (thisFac.name == facultyName)
            //    {
            //        selectedFaculty = thisFac;
            //        break;
            //    }
            //}
            return JsonConvert.SerializeObject(selectedFaculty);
        }

        public ActionResult All()
        {
            if (Session["returnedJSON"] != null)
            {
                return View("AllFaculty");
            }
            return RedirectToAction("Index");
        }

        public ActionResult SelectedFaculty1(string name)
        {
            if (Session["returnedJSON"] != null)
            {
                var faculty = Session["returnedJSON"].ToString();
                People people = JToken.Parse(faculty).ToObject<People>();
                var facMem = people.faculty.Where(a => a.name == name).Select(a => new { a.imagePath, a.name, a.title, a.username, a.email, a.office, a.phone }).ToList().FirstOrDefault();
                Faculty rtnFac = new Faculty()
                {
                    name = facMem.name,
                    title = facMem.title,
                    username = facMem.username,
                    email = facMem.email,
                    office = facMem.office,
                    phone = facMem.phone,
                    imagePath = facMem.imagePath
                };
                return View(rtnFac);
            }
            return RedirectToAction("Index");

        }


        public JsonResult ThrowJsonError(Exception e)
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            Response.StatusDescription = e.Message;

            return Json(new { Message = e.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}
