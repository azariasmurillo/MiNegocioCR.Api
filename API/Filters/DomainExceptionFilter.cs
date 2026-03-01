using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.API.Filters;

public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is InvalidStatusTransitionException ex)
        {
            context.Result = new ObjectResult(new
            {
                error = ex.Message,
                code = InvalidStatusTransitionException.ErrorCode,
                currentStatus = ex.CurrentStatus,
                requestedStatus = ex.RequestedStatus
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is NotFoundException notFound)
        {
            context.Result = new ObjectResult(new
            {
                error = notFound.Message,
                code = NotFoundException.ErrorCode,
                resource = notFound.Resource
            })
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
            context.ExceptionHandled = true;
        }
    }
}