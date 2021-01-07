using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Models.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Exceptions
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ApiResponse response;
            // check if the un-handled exception is one our custom exception types, thrown from a db access manager or other sub-component
            if (exception is BadRequestException)
            {
                response = new ApiBadRequestResponse("", exception.Message);
            }
            else if (exception is NotFoundException)
            {
                response = new ApiNotFoundResponse(exception.Message);
            }
            else
            {
                // we don't know what kind it is, so just respond with a 500 internal server error
                response = new ApiFailedResponse(exception);
            }
            var result = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.StatusCode;
            return context.Response.WriteAsync(result);
        }
    }
}
