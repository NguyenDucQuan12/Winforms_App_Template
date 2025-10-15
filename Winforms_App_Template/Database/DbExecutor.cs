using System.Data;
using Dapper;                           // Dapper micro-ORM
using Polly;                            // ResiliencePipeline

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Thực thi truy vấn SQL (Dapper) qua Polly v8 ResiliencePipeline:
    /// - Có Retry + Circuit Breaker thông qua SQLPolicies 
    /// - Có Timeout (CommandDefinition) + CancellationToken tránh treo vô hạn
    /// - Kết nối ngắn: open late / close early
    /// </summary>
    public class DbExecutor
    {
        private readonly string _connString;           // chuỗi kết nối, không cho phép thay đổi
        private readonly ResiliencePipeline _pipeline; // pipeline resilience dùng chung

        public DbExecutor(string? connString = null, ResiliencePipeline? pipeline = null)
        {
            _connString = DbConfig.GetConnectionString(connString);        // Lấy chuỗi kết kết nối
            _pipeline = pipeline ?? SqlPolicies.CreatePipeline();        // Tạo pipeline mặc định như đã cấu hình trong SQLPilicies
        }

        /// <summary>
        /// SELECT trả IEnumerable&lt;T&gt; (Dapper).
        /// Dùng .AsTask() vì ExecuteAsync (v8) trả ValueTask&lt;T&gt;.
        /// </summary>
        /// Hàm này bất đồng bộ (Task) trả về một tập hợp IEnumerable<T>
        /// Trong đó T (Generic) là kiểu mà ta tự định nghĩa khi trả về kết quả
        public Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? param = null,
            int commandTimeoutSeconds = 30,
            CommandType commandType = CommandType.Text,
            CancellationToken ct = default)
        => _pipeline.ExecuteAsync(async token =>                 // => là cú pháp rút gọn của return
        {
            // Mỗi lần gọi mở 1 kết nối mới (pooling tái sử dụng ngầm), và tự Dispose để trả connection về pool
            using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);

            // Dapper QueryAsync có CommandDefinition → set timeout + cancel
            var result = await conn.QueryAsync<T>(new CommandDefinition(
                commandText: sql,
                parameters: param,
                commandType: commandType,
                commandTimeout: commandTimeoutSeconds,
                cancellationToken: token)).ConfigureAwait(false);

            return result; // IEnumerable<T>
        }, ct).AsTask();  // <— CHUYỂN ValueTask<IEnumerable<T>> → Task<IEnumerable<T>>

        /// <summary>
        /// INSERT/UPDATE/DELETE trả số dòng ảnh hưởng.
        /// </summary>
        public Task<int> ExecuteAsync(
            string sql,
            object? param = null,
            int commandTimeoutSeconds = 30,
            CommandType commandType = CommandType.Text,
            CancellationToken ct = default)
        => _pipeline.ExecuteAsync(async token =>
        {
            using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);

            var affected = await conn.ExecuteAsync(new CommandDefinition(
                commandText: sql,
                parameters: param,
                commandType: commandType,
                commandTimeout: commandTimeoutSeconds,
                cancellationToken: token)).ConfigureAwait(false);

            return affected; // int
        }, ct).AsTask(); // <— CHUYỂN ValueTask<int> → Task<int>

        /// <summary>
        /// Giao dịch nhiều lệnh phụ thuộc nhau (retry toàn khối nếu transient).  
        /// Nhận kết quả trả về
        /// </summary>
        public Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<IDbConnection, IDbTransaction, CancellationToken, Task<TResult>> body,
            IsolationLevel isolation = IsolationLevel.ReadCommitted,
            CancellationToken ct = default)
        => _pipeline.ExecuteAsync(async token =>
        {
            // Tạo connect và IDTransaction
            // Mọi lệnh SQL muốn nằm trong cùng một giao dịch phải được truyền transaction: tx (và dùng đúng conn đã tạo tx)
            using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);
            using var tx = conn.BeginTransaction(isolation);

            try
            {
                // Cho phần thân tự thực thi các lệnh Dapper; đảm bảo truyền conn, tx để cùng nằm trong 1 giao dịch
                var result = await body(conn, tx, token).ConfigureAwait(false);

                // Nếu tất cả lệnh thành công thì commit để lưu lại
                tx.Commit();
                return result; // dummy int để khớp generic; sẽ bị bỏ qua
            }
            catch
            {
                // Tiến hành rollback nếu 1 giao dịch lỗi
                try { tx.Rollback(); } catch { /* ignore rollback error */ }
                throw;
            }
        }, ct).AsTask();

        /// <summary>
        /// Giao dịch nhiều lệnh phụ thuộc nhau (retry toàn khối nếu transient).  
        /// Không nhận kết quả trả về
        /// </summary>
        //public Task ExecuteInTransactionAsync(
        //    Func<IDbConnection, IDbTransaction, CancellationToken, Task> body,
        //    IsolationLevel isolation = IsolationLevel.ReadCommitted,
        //    CancellationToken ct = default)
        //=> _pipeline.ExecuteAsync(async token =>
        //{
        //    // Tạo connect và IDTransaction
        //    // Mọi lệnh SQL muốn nằm trong cùng một giao dịch phải được truyền transaction: tx (và dùng đúng conn đã tạo tx)
        //    using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);
        //    using var tx = conn.BeginTransaction(isolation);

        //    try
        //    {
        //        // Cho phần thân tự thực thi các lệnh Dapper; đảm bảo truyền conn, tx để cùng nằm trong 1 giao dịch
        //        await body(conn, tx, token).ConfigureAwait(false);

        //        // Nếu tất cả lệnh thành công thì commit để lưu lại
        //        tx.Commit();
        //        return 0; // dummy int để khớp generic; sẽ bị bỏ qua
        //    }
        //    catch
        //    {
        //        // Tiến hành rollback nếu 1 giao dịch lỗi
        //        try { tx.Rollback(); } catch { /* ignore rollback error */ }
        //        throw;
        //    }
        //}, ct).AsTask(); // <— CHUYỂN ValueTask<int> → Task
    }
}