using MediatR;

namespace Company.Project.Application.Contracts.Products;

/// <summary>
/// 表达按主键查询商品详情的读取意图。
/// </summary>
public sealed record GetProductQuery(long Id) : IRequest<ProductDto>;
