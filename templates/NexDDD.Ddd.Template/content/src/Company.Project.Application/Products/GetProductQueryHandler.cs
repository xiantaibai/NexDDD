using Company.Project.Application.Contracts.Products;
using Company.Project.Domain.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NexDDD.Core.DatabaseAccessor.Repositories;
using NexDDD.Core.Exceptions;

namespace Company.Project.Application.Products;

/// <summary>
/// 使用通用查询仓储投影商品 DTO，保持读模型与领域实体隔离。
/// 负责NexDDD 核心模块中的 GetProductQueryHandler 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
public sealed class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IQueryRepository<Product> _productQueries;

    /// <summary>
    /// 创建商品查询处理器。
    /// </summary>
    public GetProductQueryHandler(IQueryRepository<Product> productQueries)
    {
        _productQueries = productQueries;
    }

    /// <inheritdoc />
    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productQueries
            .AsQueryable(tracking: false)
            .Where(x => x.Id == request.Id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Sku = x.Sku,
                Price = x.Price
            })
            .FirstOrDefaultAsync(cancellationToken);

        return product ?? throw ExceptionHelper.AppFriendly(ProductErrorCodes.NotFound);
    }
}
