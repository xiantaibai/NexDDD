using Company.Project.Application.Contracts.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Company.Project.WebApi.Controllers;

/// <summary>
/// 提供商品用例的 HTTP 接入，并将请求转发给 MediatR。
/// </summary>
[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 创建商品接口控制器。
    /// </summary>
    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 创建商品。
    /// </summary>
    [HttpPost]
    public Task<long> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        return _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// 按主键查询商品。
    /// </summary>
    [HttpGet("{id:long}")]
    public Task<ProductDto> Get(long id, CancellationToken cancellationToken)
    {
        return _mediator.Send(new GetProductQuery(id), cancellationToken);
    }
}
