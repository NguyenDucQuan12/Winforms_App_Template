using DevExpress.XtraReports.UI;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Table;
using Winforms_App_Template.Utils;

public sealed class ReportLayoutStore
{
    private readonly string _reportName;       // Tên report cần thao tác
    private readonly string _updatedBy;        // user hiện tại (để audit)

    public ReportLayoutStore(string reportName, string updatedBy)
    {
        _reportName = reportName;
        _updatedBy = updatedBy;
    }

    /// <summary>
    /// Nén dữ liệu bằng GZip trong bộ nhớ RAM
    /// </summary>
    /// <param name="raw">Tệp tin report của DevExpress</param>
    /// <returns></returns>
    private static byte[] Gzip(byte[] raw)
    {
        // Khởi tạo lớp cho phép lưu trữ dữ liệu dạng nhị phân ngay trong bộ nhớ RAM
        using var msOut = new MemoryStream();
        // Khởi tạo GZipStream, dữ liệu sẽ được nén vào msOut
        using (var gz = new GZipStream(msOut, CompressionLevel.Optimal, leaveOpen: true))
            gz.Write(raw, 0, raw.Length); // Ghi toàn bộ dữ liệu vào gz và GZipStream sẽ nén dữ liệu rồi ghi vào msOut

        return msOut.ToArray(); // Chuyển toàn bộ dữ liệu đã nén trong msOut thành mảng byte, rồi trả về.
    }

    /// <summary>
    /// Giải nén byte[] GZip
    /// </summary>
    /// <param name="gzBytes"></param>
    /// <returns></returns>
    private static byte[] Gunzip(byte[] gzBytes)
    {
        using var msIn = new MemoryStream(gzBytes);
        // Tạo 1 GZipStream để đọc và giải nén dữ liệu từ msIn
        using var gz = new GZipStream(msIn, CompressionMode.Decompress);
        // Tạo MemoryStream để chứa dữ liệu sau khi giải nén.(Lưu trên RAM)
        using var msOut = new MemoryStream();
        // Dữ liệu giải nén được copy vào msOut
        gz.CopyTo(msOut);
        return msOut.ToArray();
    }

    /// <summary>
    /// Tính SHA256 của byte[]
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static byte[] Sha256Of(byte[] data)
    {
        // Kiểm tra đầu vào
        if (data == null || data.Length == 0)
            return Array.Empty<byte>();
        
        // Tính toán sha và trả về
        using var sha = SHA256.Create();
        return sha.ComputeHash(data);
    }

    // <summary>
    /// Lấy "key" duy nhất cho report, ưu tiên DisplayName (nếu được set), fallback sang tên lớp report (GetType().Name).
    /// Đảm bảo không trả về null hoặc chuỗi rỗng.
    /// </summary>
    public static string GetKey(XtraReport rpt)
    {
        if (rpt == null)
            throw new ArgumentNullException(nameof(rpt), "Report không được null.");

        // Ưu tiên DisplayName nếu hợp lệ
        var name = rpt.DisplayName;
        if (!string.IsNullOrWhiteSpace(name))
            return name.Trim();

        // Fallback sang tên kiểu (class name)
        var typeName = rpt.GetType().Name;
        if (!string.IsNullOrWhiteSpace(typeName))
            return typeName.Trim();

        // Trường hợp hiếm khi cả hai đều rỗng
        return "UnnamedReport_" + Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Lưu layout của report vào DB, trả về Version mới.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public async Task<int> SaveAsync(XtraReport report, CancellationToken ct = default)
    {
        // Kiểm tra đầu vào
        if (report == null)
            throw new ArgumentNullException(nameof(report), "Report không được null.");

        if (string.IsNullOrWhiteSpace(_reportName))
            throw new InvalidOperationException("Tên report (_reportName) chưa được thiết lập.");

        // Ghi XML của layout vào MemoryStream (ms)
        byte[] xml;
        using (var ms = new MemoryStream())
        {
            // Lưu tệp tin repx sang định dạng xml
            try
            {
                report.SaveLayoutToXml(ms);
                xml = ms.ToArray();

                if (xml.Length == 0)
                {
                    LogEx.Error($"Tệp tin {_reportName} trống hoặc không hợp lệ để lưu dưới dạng xml");
                    throw new InvalidDataException("Report layout trống hoặc không hợp lệ.");
                }
            }
            catch (Exception ex)
            {
                LogEx.Error($"Không thể lưu tệp tin {_reportName} sang định dạng xml. Lỗi: {ex}");
                throw new InvalidOperationException("Không thể xuất layout của report.", ex);
            }
        }

        // Nén + checksum
        byte[] gz;
        byte[] hash;
        try
        {
            gz = Gzip(xml);        // Nén xml thành byte để giảm kích thước trước khi lưu DB (tránh phình DB/băng thông).
            hash = Sha256Of(xml);  // Tạo checksum SHA-256 trên XML gốc (không phải trên dữ liệu nén) để kiểm toàn vẹn sau này
        }
        catch (Exception ex)
        {
            LogEx.Error($"Không thể nén hoặc tạo checksum cho tệp tin {_reportName}. Lỗi: {ex}");
            throw new InvalidOperationException("Không thể nén hoặc tạo checksum cho report.", ex);
        }

        // Gọi Store Procedure lưu vào DB
        try
        {
            var repo = new ReportLayoutVersions_Table(new DbExecutor());

            int newVersion = await repo.AddVersionAsync(
                reportName: _reportName,
                updatedBy: Environment.UserName ?? "UnknownUser",
                contentGz: gz,
                sha256: hash,
                ct: ct
            ).ConfigureAwait(false);

            return newVersion;
        }
        catch (OperationCanceledException)
        {
            // Nếu người dùng hủy qua CancellationToken
            throw;
        }
        catch (Exception ex)
        {
            LogEx.Error($"Lỗi khi lưu phiên bản report {_reportName} vào cơ sở dữ liệu. Lỗi: {ex}");
            throw new InvalidOperationException("Lỗi khi lưu phiên bản report vào cơ sở dữ liệu.", ex);
        }
    }

    /// <summary>
    /// Đọc layout phiên bản mới nhất từ DB và load vào report.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public async Task<bool> TryLoadAsync(XtraReport report, int commandTimeoutSeconds = 30, CancellationToken ct = default)
    {
        // Kiểm tra đầu vào
        if (report == null)
            throw new ArgumentNullException(nameof(report), "Report không được null.");

        if (string.IsNullOrWhiteSpace(_reportName))
            throw new InvalidOperationException("Tên report (_reportName) chưa được thiết lập.");

        try
        {
            //Truy vấn phiên bản mới nhất từ DB
            var repo = new ReportLayoutVersions_Table(new DbExecutor());
            var row = await repo.GetLatestAsync(reportName: _reportName, ct: ct).ConfigureAwait(false);
            if (row == null)
                return false;

            // Giải nén tệp tin
            byte[] gz = row.ContentGz;  // GÓI DỮ LIỆU NÉN: GZip của XML .repx
            byte[] xml = Gunzip(gz);    // GIẢI NÉN → bytes XML thuần của layout thành tệp .repx
            byte[] sha = (byte[])row.Sha256;   // CHỮ KÝ/Checksum SHA-256 của XML (để kiểm toàn vẹn)

            if (xml.Length == 0)
            {
                LogEx.Warning($"Layout tải từ DB sau khi giải nén rỗng. Không thể sử dụng");
                throw new InvalidDataException("Layout XML sau khi giải nén rỗng.");
            }

            // Tính lại SHA256 của xml vừa giải nén, so sánh với sha trong DB (chống hỏng/tamper)
            if (!CryptographicOperations.FixedTimeEquals(Sha256Of(xml), sha))
            {
                LogEx.Error($"Checksum tệp tin report tải từ DB không hợp lệ: {sha} - {Sha256Of(xml)}");
                throw new InvalidOperationException("Checksum mismatch: layout bị hỏng khi tải từ DB.");
            }

            // Load layout mới vào report
            using var ms = new MemoryStream(xml);  // Bọc bytes XML vào stream bộ nhớ (không cần file tạm)
            report.LoadLayoutFromXml(ms);          // Nạp layout trực tiếp từ stream XML

            // Ghi Log sử dụng phiên bản nào
            LogEx.Info($"Người dùng sử dụng tệp tin {row.ReportName} với phiên bản {row.Version} được tải lên ngày {row.UpdatedAtUtc}");
            return true;
        }
        catch (OperationCanceledException)
        {
            // Nếu người dùng hủy qua CancellationToken
            throw;
        }
        catch (Exception ex)
        {
            // Ghi log
            LogEx.Error($"Không thể tải layout cho report {_reportName}. Lỗi: {ex}");
            throw new InvalidOperationException(
                $"Không thể tải layout cho report '{_reportName}'.",
                ex
            );
        }
    }
}



///-- SQL Server Stored Procedures for ReportLayoutStore.cs
///-- 1) Bảng lưu layout report
///```sql
//--Bảng lưu các phiên bản layout (append-only)
//CREATE TABLE dbo.ReportLayoutVersions
//(
//    LayoutId      bigint          IDENTITY(1,1) PRIMARY KEY,   -- Khóa duy nhất từng bản ghi
//    ReportName    nvarchar(128)   NOT NULL,
//    Version       int             NOT NULL,                    -- Tăng dần theo từng ReportName
//    UpdatedAtUtc  datetime2(3)    NOT NULL CONSTRAINT DF_RLV_UpdatedAtUtc DEFAULT (sysutcdatetime()),
//    UpdatedBy     nvarchar(128)   NOT NULL,
//    ContentGz     varbinary(max)  NOT NULL,                    -- GZip của XML .repx
//    Sha256        binary(32)      NOT NULL                     -- Checksum của XML gốc
//);

//--Mỗi(ReportName, Version) là duy nhất
//CREATE UNIQUE INDEX UX_RLV_Report_Version
//ON dbo.ReportLayoutVersions(ReportName, Version);

//--Tối ưu truy vấn lấy phiên bản mới nhất
//-- (SQL Server hỗ trợ DESC trong index key)
//CREATE INDEX IX_RLV_Report_VersionDesc
//ON dbo.ReportLayoutVersions(ReportName ASC, Version DESC)
//INCLUDE (UpdatedAtUtc, UpdatedBy);


//--Tạo Procedure thêm một dòng phiên bản mới và trả về phiên bản mới nhất
//CREATE OR ALTER PROCEDURE dbo.ReportLayout_AddVersion
//    @ReportName nvarchar(128),
//    @UpdatedBy  nvarchar(128),
//    @ContentGz  varbinary(max),
//    @Sha256     binary(32)
//AS
//BEGIN
//    SET NOCOUNT ON; --tránh trả rowcount thừa
//    SET XACT_ABORT ON; --lỗi là abort transaction

//    DECLARE @NewVersion int;

//BEGIN TRAN;

///* Khóa phạm vi theo ReportName để cấp phát version tuần tự,
//   tránh 2 client cùng lúc lấy chung 1 version */
//SELECT @NewVersion = ISNULL(MAX(Version), 0) + 1
//    FROM dbo.ReportLayoutVersions WITH (UPDLOCK, HOLDLOCK)
//    WHERE ReportName = @ReportName;

//INSERT INTO dbo.ReportLayoutVersions(ReportName, Version, UpdatedBy, ContentGz, Sha256)
//    VALUES (@ReportName, @NewVersion, @UpdatedBy, @ContentGz, @Sha256);

//COMMIT;

///* RẤT QUAN TRỌNG: Trả về đúng 1 resultset, 1 cột, 1 dòng */
//SELECT CAST(@NewVersion AS int) AS Version;
//END
//GO

//-- Tạo Procedure lấy phiên bản mới nhất
//CREATE OR ALTER PROCEDURE dbo.ReportLayout_GetLatest
//    @ReportName nvarchar(128)
//AS
//BEGIN
//    SET NOCOUNT ON;

//SELECT TOP(1)
//        ReportName, Version, UpdatedAtUtc, UpdatedBy, ContentGz, Sha256
//    FROM dbo.ReportLayoutVersions
//    WHERE ReportName = @ReportName
//    ORDER BY Version DESC;
//END
//GO

//-- Tạo Procedure lấy phiên bản được chỉ định và lấy version cao nhất
//CREATE OR ALTER PROCEDURE dbo.ReportLayout_Get
//    @ReportName nvarchar(128),
//    @Version    int
//AS
//BEGIN
//    SET NOCOUNT ON;

//SELECT ReportName, Version, UpdatedAtUtc, UpdatedBy, ContentGz, Sha256
//    FROM dbo.ReportLayoutVersions
//    WHERE ReportName = @ReportName AND Version = @Version;
//END
//GO

//CREATE OR ALTER PROCEDURE dbo.ReportLayout_GetLatestVersion
//    @ReportName nvarchar(128)
//AS
//BEGIN
//    SET NOCOUNT ON;

//SELECT MAX(Version) AS Version
//    FROM dbo.ReportLayoutVersions
//    WHERE ReportName = @ReportName;
//END
//GO
//```