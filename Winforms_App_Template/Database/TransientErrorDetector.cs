using Microsoft.Data.SqlClient;

namespace Winforms_App_Template.Database
{
    /// <summary>
    /// Xác định lỗi SQL "transient" (có thể tự khỏi) để Retry.
    /// Danh sách gồm: deadlock (1205), connection reset/network (10053/10054/10060),
    /// login/server busy (4060/40197/40501) – bao trùm cả on-prem & Azure.
    /// </summary>
    public static class TransientErrorDetector
    {
        // Các mã lỗi hay gặp (không đầy đủ tuyệt đối, đủ dùng thực tế)
        private static readonly int[] TransientNumbers = new[]
        {
            1205,   // Deadlock
            4060,   // Cannot open database requested by the login
            40197,  // The service has encountered an error processing your request (Azure)
            40501,  // The service is currently busy (Azure)
            40613,  // Database not currently available (Azure)
            10928, 10929, // Resource limits (Azure)
            49918, 49919, 49920, // Cannot process request (Azure)
            10053, 10054, 10060, // Net/connection aborted/reset/timed out
            233,    // Connection init error / pipe error
        };

        /// <summary>
        /// Trả true nếu nên retry.
        /// </summary>
        public static bool IsTransient(SqlException ex)
        {
            foreach (SqlError err in ex.Errors)
                if (Array.IndexOf(TransientNumbers, err.Number) >= 0)
                    return true;
            return false;
        }
    }
}