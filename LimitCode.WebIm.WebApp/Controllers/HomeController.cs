using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LimitCode.WebIm.WebApp.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var ClientUserId = Request.Cookies["ClientUserId"];
            if (ClientUserId == null || string.IsNullOrWhiteSpace(ClientUserId.Value))
            {
                var cok = new HttpCookie("ClientUserId", Guid.NewGuid().ToString())
                {
                    Expires = DateTime.Now.AddDays(7)
                };
                Response.AppendCookie(cok);
            }
            return View();
        }
    }
}