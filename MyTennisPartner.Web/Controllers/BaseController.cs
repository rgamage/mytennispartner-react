using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyTennisPartner.Web.Exceptions;
using Microsoft.AspNetCore.Http;
using MyTennisPartner.Data.Context;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Web.Services;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Controllers
{

    /// <summary>
    /// base class for our web api controllers
    /// contains methods for standardizing our api responses, success and exception cases
    /// and other useful common functions
    /// </summary>
    public class BaseController: Controller
    {

        #region members
        protected TennisContext Context { get; }
        protected ILogger Logger { get; }
        protected NotificationService NotificationService { get; }
        #endregion

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public BaseController(ILogger<BaseController> logger, NotificationService notificationService, TennisContext context = null)
        {
            Logger = logger;
            Context = context;
            NotificationService = notificationService;
        }
        #endregion

        #region ApiResult Helpers
        /// <summary>
        /// returns http status code 200 with object content
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [NonAction]
        public static IActionResult ApiOk(object model)
        {
            return new OkObjectResult(new ApiOkResponse(model));
        }

        /// <summary>
        /// empty ok response
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [NonAction]
        public static IActionResult ApiOk()
        {
            return new OkObjectResult(new ApiOkResponse(null));
        }

        /// <summary>
        /// returns http code 400 with model state errors
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public IActionResult ApiBad()
        {
            return BadRequest(new ApiBadRequestResponse(ModelState));
        }

        /// <summary>
        /// returns http code 400 with custom validation message
        /// </summary>
        /// <param name="field"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult ApiBad(string field, string message)
        {
            return BadRequest(new ApiBadRequestResponse(field, message));
        }

        /// <summary>
        /// returns http code 404 with message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult ApiNotFound(string message)
        {
            return NotFound(new ApiNotFoundResponse(message));
        }

        /// <summary>
        /// returns http code 500 with message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult ApiFailed(string message)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiFailedResponse(message));
        }

        /// <summary>
        /// returns http code 500 with message from exception passed in
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult ApiFailed(Exception exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiFailedResponse(exception));
        }
        #endregion
    }
}
