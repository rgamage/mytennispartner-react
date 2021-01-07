using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyTennisPartner.Web.Exceptions
{
    public class ApiResponse
    {
        public int StatusCode { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ErrorResponse Errors { get; set; }

        public ApiResponse(int statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return "Resource not found";
                case 500:
                    return "An unhandled error occurred";
                default:
                    return null;
            }
        }
    }

    public class ApiOkResponse : ApiResponse
    {
        public object Data { get; }

        public ApiOkResponse(object result)
            : base(200)
        {
            Data = result;
        }
    }

    /// <summary>
    /// not found response, pass in error message
    /// </summary>
    public class ApiNotFoundResponse : ApiResponse
    {
        public ApiNotFoundResponse(string message): base(404, message)
        {
            Errors = new ErrorResponse
            {
                ErrorMessage = message
            };
        }
    }

    /// <summary>
    /// failed response, to be used for exceptions or failures of any kind, other than bad requests
    /// pass in an error message, or an exception
    /// </summary>
    public class ApiFailedResponse : ApiResponse
    {
        public ApiFailedResponse(string message) : base(500, message)
        {
            Errors = new ErrorResponse
            {
                ErrorMessage = message
            };
        }

        public ApiFailedResponse(Exception exception) : base(500)
        {
            var message = exception?.Message ?? "Unhandled exception";
            if (exception?.InnerException != null)
            {
                message += $".  Inner Exception: {exception.InnerException?.Message ?? "None" }";
            }
            Errors = new ErrorResponse
            {
                ErrorMessage = message
            };
        }
    }
    /// <summary>
    /// bad request response, used mostly for validation errors or post/put that requires a valid model
    /// </summary>
    public class ApiBadRequestResponse : ApiResponse
    {
        /// <summary>
        /// use this method to return invalid modelstate message
        /// </summary>
        /// <param name="modelState"></param>
        public ApiBadRequestResponse(ModelStateDictionary modelState)
            : base(400)
        {
            if (modelState is null)
            {
                throw new ArgumentException("modelState is null");
            }
            if (modelState.IsValid)
            {
                throw new ArgumentException("ModelState must be invalid", nameof(modelState));
            }

            var validationErrors = modelState.Select(a => new BadRequestResponse { Field = a.Key, Messages = a.Value.Errors.Select(e => e.ErrorMessage) });

            Errors = new ErrorResponse
            {
                ValidationErrors = validationErrors,
                ErrorMessage = BuildErrorMessageFromValidationErrors(validationErrors)
            };
        }

        /// <summary>
        /// uese this method to return a customized field/message response for a bad request
        /// </summary>
        /// <param name="field"></param>
        /// <param name="message"></param>
        public ApiBadRequestResponse(string field, string message)
            : base(400)
        {
            var validationErrors = new[] { new BadRequestResponse { Field = field, Messages = new string[] { message } } };
            Errors = new ErrorResponse
            {
                ValidationErrors = validationErrors,
                ErrorMessage = BuildErrorMessageFromValidationErrors(validationErrors)
            };
        }

        /// <summary>
        /// build a readable error message from the list of validation errors
        /// </summary>
        /// <param name="validationErrors"></param>
        /// <returns></returns>
        private string BuildErrorMessageFromValidationErrors(IEnumerable<BadRequestResponse> validationErrors)
        {
            //var message = "Bad Request: " +  string.Join(", ", validationErrors
            //    .Select(v => $"{v.Field}: {v.Messages.Select(m => string.Join(", ", m)).ToList()}").ToList());

            //var xxx = validationErrors
            //    .Select(v => $"{v.Field}: {string.Join(", ", v.Messages)}").ToList();

            var message = "Bad Request: " + string.Join(", ", validationErrors
                .Select(v => $"{v.Field}{(string.IsNullOrEmpty(v.Field) ? string.Empty : ": ")} {string.Join(", ", v.Messages)}").ToList());

            return message;
        }

    }

    /// <summary>
    /// container to hold errors, either a message string or array of validation errors
    /// </summary>
    public class ErrorResponse
    {
        public string ErrorMessage { get; set; }
        public IEnumerable<BadRequestResponse> ValidationErrors { get; set; }
    }

    /// <summary>
    /// data stucture to for a bad request response (for form validation, or any POST or PUT requiring a valid model be sent from client)
    /// </summary>
    public class BadRequestResponse
    {
        public string Field { get; set; }
        public IEnumerable<string> Messages { get; set; }
    }

    /*
     * To use this API Result Class, it's recommended to add the following methods to your API controller base class
     * to make it easier / simpler to return succes/fail responses
     * 
    public IActionResult ApiOk(object model)
    {
        return new OkObjectResult(new ApiOkResponse(model));
    }

    public IActionResult ApiBad(ModelStateDictionary modelState)
    {
        return BadRequest(new ApiBadRequestResponse(ModelState));
    }

    public IActionResult ApiBad(string field, string message)
    {
        return BadRequest(new ApiBadRequestResponse(field, message));
    }

    public IActionResult ApiNotFound(string message)
    {
        return NotFound(new ApiResponse(404, message));
    }
    */
}
