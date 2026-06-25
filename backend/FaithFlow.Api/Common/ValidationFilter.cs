using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FaithFlow.Backend.Common
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IValidatorFactory _validatorFactory;

        public ValidationFilter(IValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionArguments = context.ActionArguments;

            foreach (var argument in actionArguments)
            {
                var validator = _validatorFactory.GetValidator(argument.Value?.GetType());
                if (validator != null)
                {
                    var validationResult = await validator.ValidateAsync(new ValidationContext<object>(argument.Value!));

                    if (!validationResult.IsValid)
                    {
                        context.Result = new BadRequestObjectResult(new
                        {
                            status = 400,
                            title = "Validation Error",
                            errors = validationResult.Errors.ToDictionary(
                                e => e.PropertyName,
                                e => new[] { e.ErrorMessage })
                        });
                        return;
                    }
                }
            }

            await next();
        }
    }
}