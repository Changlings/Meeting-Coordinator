using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MeetingCoordinator.Controllers
{
    public class HomeController : Controller
    {
//        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

//        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

//        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public void Schedule()
        {
            String title;
            String description;
            String startTime;
            String endTime;

            try
            {
                title = Request.Form["title"];
                description = Request.Form["description"];
                startTime = Request.Form["start-time"];
                endTime = Request.Form["end-time"];
            }
            catch (Exception e)
            {
                //exception encountered
            }
        }
    }
}