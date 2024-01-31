using System.Web.Mvc;

namespace TestProject.Controllers
{
    public class DefaultController : Controller
	{
		// GET: Default
		public ActionResult Test()
		{
			return Content("test");
		}
	}
}