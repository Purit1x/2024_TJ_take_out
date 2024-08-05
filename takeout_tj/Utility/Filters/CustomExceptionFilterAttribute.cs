using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace takeout_tj.Utility
{
    public class CustomExceptionFilterAttribute : Attribute, IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilterAttribute> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public CustomExceptionFilterAttribute(ILogger<CustomExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 当有异常发生的时候，触发这个方法
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            _logger.LogInformation(context.Exception.Message);

            //在这里就应该处理异常
            if (context.ExceptionHandled == false) //如果没有被处理
            {
                context.Result = new JsonResult(new ApiResult<object>()
                {
                    Success = false,
                    Message = context.Exception.Message
                });
                //就在这里处理
                context.ExceptionHandled = true;//异常已经被处理过了；
            }
        }
    }
}
