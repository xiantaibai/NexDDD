using Company.Project.Application.Contracts.Products;
using Company.Project.Domain.Products;
using NexDDD.Provider.Snowflake;
using NexDDD.Core.Events;
using NexDDD.Core.Exceptions;

namespace Company.Project.Application.Products;

/// <summary>
/// 编排创建商品用例，协调唯一性检查、聚合创建和写仓储持久化。
/// </summary>
public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, long>
{
    private readonly IProductRepository _productRepository;
    private readonly ISnowflakeIdGenerator _idGenerator;

    /// <summary>
    /// 创建商品命令处理器。
    /// </summary>
    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ISnowflakeIdGenerator idGenerator)
    {
        _productRepository = productRepository;
        _idGenerator = idGenerator;
    }

    /// <inheritdoc />
    public async Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.FindBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct is not null)
        {
            throw ExceptionHelper.AppFriendly(ProductErrorCodes.SkuExists);
        }

        var product = new Product(
            _idGenerator.NextId(),
            request.Name,
            request.Sku,
            request.Price);

        await _productRepository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
