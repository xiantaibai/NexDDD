using Microsoft.AspNetCore.Mvc;
using YSCore.Base.Exceptions;
using YSCore.Base.Exceptions.Extensions;

namespace ExceptionDemoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExceptionDemoController : ControllerBase
{
    [HttpGet("success")]
    public object Success()
    {
        return new
        {
            message = "success",
            traceId = HttpContext.TraceIdentifier,
            serverTime = DateTimeOffset.UtcNow
        };
    }

    [HttpGet("business")]
    public object Business()
    {
        throw ExceptionHelper.AppFriendly("demo.business")
            .WithErrorCode("demo.business")
            .StatusCode(StatusCodes.Status500InternalServerError)
            .WithData(new { scene = "business" });
    }

    [HttpGet("validation")]
    public object Validation()
    {
        throw ExceptionHelper.Bah("demo.validation")
            .WithErrorCode("demo.validation")
            .WithData(new { field = "name" });
    }

    [HttpGet("internal-public")]
    public object InternalPublic()
    {
        try
        {
            throw new InvalidOperationException("inner detail from internal system");
        }
        catch (Exception ex)
        {
            throw ExceptionHelper.AppFriendly("demo.internal.public", ex)
                .WithErrorCode("demo.internal.public")
                .StatusCode(StatusCodes.Status500InternalServerError)
                .WithExposeInnerException(true)
                .WithData(new { scene = "internal-public" });
        }
    }

    [HttpGet("internal-private")]
    public object InternalPrivate()
    {
        try
        {
            throw new InvalidOperationException("inner detail from internal system");
        }
        catch (Exception ex)
        {
            throw ExceptionHelper.AppFriendly("demo.internal.private", ex)
                .WithErrorCode("demo.internal.private")
                .StatusCode(StatusCodes.Status500InternalServerError)
                .WithExposeInnerException(false)
                .WithData(new { scene = "internal-private" });
        }
    }

    [HttpGet("error-code")]
    public object ErrorCode(int type)
    {
        if (type == 0)
            throw ExceptionHelper.Bah(ErrorCodeConst.Payment.OrderNotFound, type);
        if(type == 1)
            throw ExceptionHelper.Bah(ErrorCodeConst.System.ServerError);

        return type;
    }
}
