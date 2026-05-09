using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Idempotency
{
    /// <summary>
    /// 幂等返回结果。
    /// </summary>
    public sealed class IdempotencyBeginResult
    {
        public bool Success { get; init; }

        public bool IsCompleted { get; init; }

        public bool IsProcessing { get; init; }

        public string? RequestId { get; init; }

        /// <summary>
        /// 新增成功
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static IdempotencyBeginResult SuccessResult(
            string requestId)
        {
            return new()
            {
                Success = true,
                RequestId = requestId
            };
        }

        /// <summary>
        /// 已完成
        /// </summary>
        /// <returns></returns>
        public static IdempotencyBeginResult CompletedResult()
        {
            return new()
            {
                IsCompleted = true
            };
        }

        /// <summary>
        /// 进行中
        /// </summary>
        /// <returns></returns>
        public static IdempotencyBeginResult ProcessingResult()
        {
            return new()
            {
                IsProcessing = true
            };
        }
    }
}
