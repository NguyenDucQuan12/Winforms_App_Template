using Microsoft.Data.SqlClient;           // Dùng provider hiện đại cho SQL Server (thay cho System.Data.SqlClient)

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Factory tạo kết nối SQL theo nguyên tắc:
    /// - "open late": chỉ mở ngay trước khi dùng
    /// - "close early": đóng/Dispose càng sớm càng tốt (để trả về pool)
    /// </summary>
    public static class SqlConnectionFactory
    {
        /// <summary>
        /// Tạo và TRẢ VỀ 1 SqlConnection đã MỞ SẴN.
        /// Lưu ý: người GỌI chịu trách nhiệm Dispose() (thường dùng 'using' / 'await using').
        /// </summary>
        public static async Task<SqlConnection> OpenAsync(string connString, CancellationToken ct)
        {
            // Tạo đối tượng kết nối dựa vào connection string.
            // ADO.NET có connection pooling tự động theo connString: đóng/Dispose không phá kết nối thật ngay,
            // mà đưa nó về pool để tái sử dụng cực nhanh ở lần sau.
            var conn = new SqlConnection(connString);

            // Mở kết nối ở chế độ async để KHÔNG chặn (block) UI thread.
            // Truyền CancellationToken 'ct' để có thể HỦY quá trình mở nếu người dùng bấm Cancel
            /*
             * Ví dụ: mất mạng, SQL Server không phản hồi, v.v.
             * 
             * using var cts = new CancellationTokenSource(); // tạo CTS để hủy (phải bấm Cancel)
             * using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // auto-cancel sau 10s
             * 
             * await SqlConnectionFactory.OpenAsync(connStr, cts.Token); // kết nối tới SQL và truyền token
             * 
             * cts.Cancel(); // --> OpenAsync sẽ ném OperationCanceledException nếu đang chờ (bất cứ khi nào người dùng bấm Cancel)
             */
            await conn.OpenAsync(ct)

                // ConfigureAwait(false): trong THƯ VIỆN (library) nên dùng false để:
                //  - không yêu cầu "quay về" SynchronizationContext ban đầu (UI/ASP.NET)
                //  - tránh deadlock khi người gọi (ở nơi nào đó) lỡ làm .Wait()/.Result
                //  - tiết kiệm chi phí marshal context
                // Ở tầng UI, KHÔNG dùng false nếu cần quay lại đúng UI thread sau await.
                .ConfigureAwait(false);

            // Trả về kết nôi đã mở sẵn
            return conn;
        }
    }
}