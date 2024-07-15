using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MotoRentalService.Api.Attributes
{
    public class ExtensionValidatorFilter(string[] allowedExtensions) : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.SingleOrDefault(o => o.Value is IFormFile).Value is not IFormFile fileParam)
                return;

            var fileExtension = Path.GetExtension(fileParam.FileName);

            if (!allowedExtensions.Contains(fileExtension))
            {
                context.Result = new ContentResult()
                {
                    Content = JsonConvert.SerializeObject("Extension is not supported."),
                    ContentType = "application/json"
                };
                context.HttpContext.Response.StatusCode = 400;
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}