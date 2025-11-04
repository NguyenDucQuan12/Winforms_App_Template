// LoadingHelper.cs
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winforms_App_Template.Loading
{
    public static class LoadingHelper
    {
        public static async Task<TResult?> RunFunctionWithLoadingAsync<TResult>(
            Form owner,
            Func<CancellationToken, Task<TResult?>> workMethod,
            string caption = "Đang xử lý...",
            Image? gifOverride = null)
        {
            using var cts = new CancellationTokenSource();
            using var host = ThreadedLoadingHost.Start(owner, cts, caption, gifOverride, disableOwner: true);

            try
            {
                // Đẩy sang background để UI thread rảnh
                return await Task.Run(() => workMethod(cts.Token), cts.Token);
            }
            finally
            {
                host.Dispose(); // đóng popup + mở khoá owner (đã marshal an toàn)
            }
        }

        public static async Task RunFunctionWithLoadingAsync(
            Form owner,
            Func<CancellationToken, Task> workMethod,
            string caption = "Đang xử lý...",
            Image? gifOverride = null)
        {
            using var cts = new CancellationTokenSource();
            using var host = ThreadedLoadingHost.Start(owner, cts, caption, gifOverride, disableOwner: true);
            try
            {
                await Task.Run(() => workMethod(cts.Token), cts.Token);
            }
            finally
            {
                host.Dispose();
            }
        }

        // 1 tham số + có kết quả
        public static async Task<TResult?> RunFunctionWithLoadingAsync<TArg, TResult>(
            Form owner,
            Func<TArg, CancellationToken, Task<TResult?>> workMethod,
            TArg arg,
            string caption = "Đang xử lý...",
            Image? gifOverride = null)
        {
            using var cts = new CancellationTokenSource();
            using var host = ThreadedLoadingHost.Start(owner, cts, caption, gifOverride, disableOwner: true);
            try
            {
                return await Task.Run(() => workMethod(arg, cts.Token), cts.Token);
            }
            finally
            {
                host.Dispose();
            }
        }

        // 1 tham số + không kết quả
        public static async Task RunFunctionWithLoadingAsync<TArg>(
            Form owner,
            Func<TArg, CancellationToken, Task> workMethod,
            TArg arg,
            string caption = "Đang xử lý...",
            Image? gifOverride = null)
        {
            using var cts = new CancellationTokenSource();
            using var host = ThreadedLoadingHost.Start(owner, cts, caption, gifOverride, disableOwner: true);
            try
            {
                await Task.Run(() => workMethod(arg, cts.Token), cts.Token);
            }
            finally
            {
                host.Dispose();
            }
        }

        // ✅ Overload 2 tham số
        public static async Task<TResult?> RunFunctionWithLoadingAsync<T1, T2, TResult>(
            Form owner,
            Func<T1, T2, CancellationToken, Task<TResult?>> workMethod,
            T1 arg1, T2 arg2,
            string caption = "Đang xử lý...",
            Image? gifOverride = null)
        {
            using var cts = new CancellationTokenSource();
            using var host = ThreadedLoadingHost.Start(owner, cts, caption, gifOverride, disableOwner: true);
            try
            {
                return await Task.Run(() => workMethod(arg1, arg2, cts.Token), cts.Token);
            }
            finally
            {
                host.Dispose();
            }
        }

        // ✅ Overload 3 tham số
        public static async Task<TResult?> RunFunctionWithLoadingAsync<T1, T2, T3, TResult>(
            Form owner,
            Func<T1, T2, T3, CancellationToken, Task<TResult?>> workMethod,
            T1 arg1, T2 arg2, T3 arg3,
            string caption = "Đang xử lý...",
            Image? gifOverride = null)
        {
            using var cts = new CancellationTokenSource();
            using var host = ThreadedLoadingHost.Start(owner, cts, caption, gifOverride, disableOwner: true);
            try
            {
                return await Task.Run(() => workMethod(arg1, arg2, arg3, cts.Token), cts.Token);
            }
            finally
            {
                host.Dispose();
            }
        }
    }
}


/// Cách sử dụng:
/// 1. Chạy 1 hàm bất kỳ, không có kết quả trả về, hoặc có kết quả trả về thì thêm TResult và return
/// private async void btnSync_Click(object sender, EventArgs e)
// {
//    await LoadingHelper.RunWithLoadingAsync(this, async ct =>
//    {
//        await _repo.SyncSomethingAsync(ct); // nhớ truyền ct để Cancel có tác dụng
//        ct.ThrowIfCancellationRequested();
//        // ... các bước khác
//    }, caption: "Đang đồng bộ dữ liệu...");
// }
/// 2. Tự mở popup không modal, tự đóng sau
//  private async void btnDoLongJob_Click(object sender, EventArgs e)
// {
//    // Mở popup & khoá form
//    var (dlg, cts) = LoadingHelper.ShowNonModal(
//        owner: this,
//        caption: "Đang chạy tác vụ dài..."
//    );

//    try
//    {
//        // Chạy việc (ví dụ fire-and-forget hoặc tự await)
//        await Task.Run(() => LongWork(cts.Token), cts.Token);
//    }
//    catch (OperationCanceledException)
//    {
//        MessageBox.Show("Đã hủy.");
//    }
//    catch (Exception ex)
//    {
//        MessageBox.Show(ex.Message);
//    }
//    finally
//    {
//        // Đóng popup & mở khoá form
//        Winforms_App_Template.Loading.LoadingHelper.CloseLoading(dlg, this, cts);
//    }
// }
