using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string LanguageCode => CultureInfo.CurrentCulture.Name;
    }
}
