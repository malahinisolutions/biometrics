using apidemo.Services;
using AtenApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace apidemo.Controllers
{
    public class CompareController : ApiController
    {
        public HttpResponseMessage Post(Data data)
        {
            JObject response = new JObject();
            string logPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Log/");
            LogUtility ut = new LogUtility(logPath);
            try
            {
                //if(string.IsNullOrWhiteSpace(data.id) && (string.IsNullOrWhiteSpace(data.photo_one) || string.IsNullOrWhiteSpace(data.photo_two)))
                //{
                //    throw new Exception("id is required");
                //}
                //else 
                if(!("iris".Equals(data.type) || "face".Equals(data.type)))
                {
                    throw new Exception("type is required and it should be iris or face.");
                }
                else if(string.IsNullOrWhiteSpace(data.photo_one) || string.IsNullOrWhiteSpace(data.photo_two))
                {
                    throw new Exception("photo_one and photo_two are required");
                }
                //else if("face".Equals(data.type) && string.IsNullOrWhiteSpace(data.photo_one))
                //{
                //    throw new Exception("photo_one is required if you are verifying face");
                //}
                if ("iris".Equals(data.type))
                {
                    Iris iris = new Iris();
                    iris.PhotoOne = data.photo_one;
                    iris.PhotoTwo = data.photo_two;
                    float score = iris.Compare();
                    response.Add("score", score);
                }
                else if ("face".Equals(data.type))
                {
                    Face face = new Face();
                    face.PhotoOne = data.photo_one;
                   face.PhotoTwo = data.photo_two;
                    float score = face.Compare();
                    response.Add("score", score);
                }
                string s = response.ToString(Newtonsoft.Json.Formatting.None, null);
                ut.Write(data.type+" | Success", s, "Post", "/api/v1/compare","OK");
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                response.Add("error", ex.Message);
               string s= response.ToString(Newtonsoft.Json.Formatting.None, null);
                ut.Write(data.type+" | Error",s, "Post", "/api/v1/compare","Bad Request");
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }
    }
}
