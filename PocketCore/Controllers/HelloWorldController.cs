using Microsoft.AspNetCore.Mvc;
namespace Pocket.Controllers
{
    public class HelloWorldController : Controller
    {
        //
        // GET: /HelloWorld/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Welcome(string name, int id = 1)
        {
            ViewBag.Message = name + " Your Welcome Page";
            return View();
        }
	}
}