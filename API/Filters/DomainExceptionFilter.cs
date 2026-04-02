using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiNegocioCR.Api.Application.Common;
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

        if (context.Exception is WhatsappNotConfiguredException whatsappEx)
        {
            context.Result = new ObjectResult(new
            {
                error = whatsappEx.Message,
                code = WhatsappNotConfiguredException.ErrorCode
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
            context.ExceptionHandled = true;
        }

        if (context.Exception is EncryptionFailedException encryptEx)
        {
            context.Result = new ObjectResult(new
            {
                error = encryptEx.Message,
                code = EncryptionFailedException.ErrorCode
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is DecryptionFailedException decryptEx)
        {
            context.Result = new ObjectResult(new
            {
                error = decryptEx.Message,
                code = DecryptionFailedException.ErrorCode
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
            context.ExceptionHandled = true;
        }

        if (context.Exception is ArgumentException argEx)
        {
            context.Result = new ObjectResult(new
            {
                error = argEx.Message,
                code = "VALIDATION_ERROR"
            })
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is UnauthorizedAccessException unauthorized &&
            unauthorized.Message == WhatsappReconnectRequired.Code)
        {
            context.Result = new ObjectResult(new
            {
                error = "WhatsApp session expired or revoked. Reconnect WhatsApp in the app.",
                code = WhatsappReconnectRequired.Code
            })
            {
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
            context.ExceptionHandled = true;
            return;
        }
    }
}