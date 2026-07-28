using MediatR;

namespace Company.Project.Application.Contracts.Products;

/// <summary>
/// 表达按主键查询商品详情的读取意图。
/// 承载NexDDD 核心流程中的不可变数据，供处理器和基础设施组件安全传递。
/// </summary>
public sealed record GetProductQuery(long Id) : IRequest<ProductDto>;
