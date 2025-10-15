using System;
using System.Linq;                             // dùng Select(...) để gom mã lỗi SQL
using Microsoft.Data.SqlClient;                // SqlException
using Polly;                                   // ResiliencePipeline, PredicateBuilder, builder
using Polly.Retry;                             // RetryStrategyOptions, DelayBackoffType
using Polly.CircuitBreaker;                    // CircuitBreakerStrategyOptions
using Winforms_App_Template.Utils;                              // LogEx (Serilog wrapper)

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Tạo Resilience Pipeline: Retry + Circuit Breaker
    /// + Gắn log (LogEx) cho sự kiện retry / breaker open / close / half-open.
    /// </summary>
    public static class SqlPolicies
    {
        public static ResiliencePipeline CreatePipeline(
            int maxRetryAttempts = 3,                 // số lần thử lại (không tính lần đầu)
            TimeSpan? baseDelay = null,               // delay cơ sở cho backoff
            int minThroughput = 20,                   // Tối thiểu số request trong “cửa sổ” để breaker đánh giá
            double failureRatio = 0.5,                // tỷ lệ lỗi để mở mạch (50%)
            TimeSpan? breakDuration = null)           // thời gian “mở mạch”
        {
            baseDelay ??= TimeSpan.FromMilliseconds(200);  // mặc định 200ms nếu không truyền vào
            breakDuration ??= TimeSpan.FromSeconds(20);     // mặc định 20s nếu không truyền vào

            // 1) shouldHandle: quy định LOẠI lỗi mà Retry/Breaker sẽ can thiệp
            //    - .Handle<SqlException>(predicate): chỉ nhận các SqlException "transient" (tạm thời)
            //    - .Handle<TimeoutException>(): cộng thêm lỗi Timeout tự định nghĩa
            var shouldHandle = new PredicateBuilder()
                .Handle<SqlException>(TransientErrorDetector.IsTransient)  // chỉ lỗi SQL “transient”
                .Handle<TimeoutException>();                               // + timeout

            // 2) Chiến lược Retry 
            var retry = new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,         // tối đa N lần thử lại
                Delay = baseDelay.Value,                     // delay cơ sở
                BackoffType = DelayBackoffType.Exponential,  // exponential backoff
                UseJitter = true,                            // thêm jitter tránh đồng loạt retry cùng lúc
                ShouldHandle = shouldHandle,                 // chỉ retry các lỗi định nghĩa ở trên

                // Callback khi RETRY diễn ra
                // Chú ý: OnRetryArguments KHÔNG có Delay → không log DelayMs ở đây.
                OnRetry = args =>
                {
                    var ex = args.Outcome.Exception; // Exception gây retry (nếu có)
                    var sql = ex as SqlException ?? ex?.InnerException as SqlException;
                    // Gom các mã lỗi SQL (nếu có) thành chuỗi, ví dụ: "1205,4060"
                    var codes = sql == null
                        ? "n/a"
                        : string.Join(",", sql.Errors.Cast<SqlError>().Select(e => e.Number));

                    // Ghi log cảnh báo (warning)
                    LogEx.Warning(
                        "Thử lại truy vấn SQL lần thứ {Attempt}, sqlErrors=[{Codes}]",
                        args.AttemptNumber, codes);

                    return default; // ValueTask.CompletedTask
                }
            };

            // 3) Circuit Breaker : khi lỗi dồn dập, tạm "mở mạch" để fail nhanh thay vì dồn thêm tải
            var breaker = new CircuitBreakerStrategyOptions
            {
                ShouldHandle = shouldHandle,                 // Cùng tiêu chí lỗi như Retry, lỗi nào được tính (SqlException transient, TimeoutException)
                FailureRatio = failureRatio,                 // Ngưỡng tỷ lệ lỗi trong SamplingDuration (vd 0.5 = 50%)
                MinimumThroughput = minThroughput,           // Số lượng tối thiểu trong cửa sổ để breaker đánh giá
                SamplingDuration = TimeSpan.FromSeconds(30), // Cửa sổ đo quan sát (30s)
                BreakDuration = breakDuration.Value,         // Khi mở mạch, giữ nguyên trong bao lâu

                // Khi BREAKER chuyển sang trạng thái mở mạch (open)
                OnOpened = args =>
                {
                    var ex = args.Outcome.Exception;
                    var sql = ex as SqlException ?? ex?.InnerException as SqlException;
                    var codes = sql == null
                        ? "n/a"
                        : string.Join(",", sql.Errors.Cast<SqlError>().Select(e => e.Number));

                    LogEx.Error(
                        "SQL CIRCUIT đã chuyển sang trạng thái OPEN trong {BreakMs}ms, sqlErrors=[{Codes}]. Tất cả kết nối sẽ tạm thời thất bại.",
                        (int)args.BreakDuration.TotalMilliseconds, codes);

                    return default;
                },

                // Khi BREAKER khép lại (Reset → Closed) sau thời gian nghỉ
                OnClosed = args =>
                {
                    LogEx.Information("SQL CIRCUIT đã đóng, tất cả kết nối tới DB được mở lại");
                    return default;
                },

                // Khi breaker Half-Open (nửa mở): Polly cho qua vài request “thăm dò”
                OnHalfOpened = args =>
                {
                    LogEx.Information("SQL CIRCUIT chuyển sang chế độ HALF-OPEN, cho phép một số yêu cầu tới DB.");
                    return default;
                }
            };

            // 4) Dựng pipeline theo thứ tự: Retry → CircuitBreaker
            //    - Request đi vào tầng Retry trước (có thể lặp lại)
            //    - Nếu lỗi dồn dập, Breaker sẽ mở để các request tiếp theo fail nhanh
            var pipeline = new ResiliencePipelineBuilder()
                .AddRetry(retry)               // tầng Retry
                .AddCircuitBreaker(breaker)    // tầng Circuit Breaker
                .Build();                      // tạo pipeline

            return pipeline;
        }
    }
}