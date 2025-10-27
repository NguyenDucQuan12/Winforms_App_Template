// Utils/ReportLayoutStore.cs
using Dapper;
using DevExpress.CodeParser;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Wizards;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Table;
using static DevExpress.XtraBars.Docking2010.Views.BaseRegistrator;

public sealed class ReportLayoutStore
{
    private readonly string _connString;
    private readonly string _reportName;       // ví dụ: "Testreport"
    private readonly string _updatedBy;        // user hiện tại (để audit)

    public ReportLayoutStore(string connString, string reportName, string updatedBy)
    {
        _connString = connString;
        _reportName = reportName;
        _updatedBy = updatedBy;
    }

    /// <summary>
    /// Nén byte[] bằng GZip
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    private static byte[] Gzip(byte[] raw)
    {
        // Nén dữ liệu
        using var msOut = new MemoryStream();
        // Khởi tạo GZipStream
        using (var gz = new GZipStream(msOut, CompressionLevel.Optimal, leaveOpen: true))
            gz.Write(raw, 0, raw.Length); // Ghi dữ liệu vào GZipStream

        return msOut.ToArray(); // Trả về mảng byte đã nén
    }

    /// <summary>
    /// Giải nén byte[] GZip
    /// </summary>
    /// <param name="gzBytes"></param>
    /// <returns></returns>
    private static byte[] Gunzip(byte[] gzBytes)
    {
        using var msIn = new MemoryStream(gzBytes);
        using var gz = new GZipStream(msIn, CompressionMode.Decompress);
        using var msOut = new MemoryStream();
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
        using var sha = SHA256.Create();
        return sha.ComputeHash(data);
    }

    /// <summary>
    /// Lưu layout của report vào DB, trả về Version mới.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public async Task<int> SaveAsync(XtraReport report, CancellationToken ct = default)
    {
        // Ghi XML của layout vào MemoryStream (ms)
        byte[] xml;
        using (var ms = new MemoryStream())
        {
            report.SaveLayoutToXml(ms); // Nội dung tệp .repx nhưng đang nằm trong bộ nhớ RAM (mảng byte[] xml)
            xml = ms.ToArray();
        }

        // 2) Nén + checksum
        byte[] gz = Gzip(xml);  // Nén xml thành gz để giảm kích thước trước khi lưu DB (tránh phình DB/băng thông).
        byte[] hash = Sha256Of(xml);  // Tạo checksum SHA-256 trên XML gốc (không phải trên dữ liệu nén) để kiểm toàn vẹn

        // Gọi Store Procedure lưu vào DB
        var repo = new ReportLayoutVersions_Table(new DbExecutor());
        int newVersion = await repo.AddVersionAsync(
            reportName: "Testreport",
            updatedBy: Environment.UserName,
            contentGz: gz,
            sha256: hash,
            ct: ct);

        return newVersion;
    }

    /// <summary>
    /// Đọc layout phiên bản mới nhất từ DB và load vào report.
    /// </summary>
    /// <param name="report"></param>
    /// <returns></returns>
    public async Task<bool> TryLoadAsync(XtraReport report, int commandTimeoutSeconds = 30, CancellationToken ct = default)
    {
        //Truy vấn phiên bản mới nhất từ DB
        var repo = new ReportLayoutVersions_Table(new DbExecutor());
        var row = await repo.GetLatestAsync(reportName: "Testreport", ct: ct);
        if (row == null)
            return false;

        byte[] gz = row.ContentGz;  // GÓI DỮ LIỆU NÉN: GZip của XML .repx
        byte[] xml = Gunzip(gz);    // GIẢI NÉN → bytes XML thuần của layout thành tệp .repx
        byte[] sha = (byte[])row.Sha256;   // CHỮ KÝ/Checksum SHA-256 của XML (để kiểm toàn vẹn)

        // Tự tính lại SHA256 của xml vừa giải nén, so sánh với sha trong DB (chống hỏng/tamper)
        if (!CryptographicOperations.FixedTimeEquals(Sha256Of(xml), sha))
            throw new InvalidOperationException("Checksum mismatch: layout bị hỏng khi tải từ DB.");

        // Load layout mới vào report
        using var ms = new MemoryStream(xml);  // Bọc bytes XML vào stream bộ nhớ (không cần file tạm)
        report.LoadLayoutFromXml(ms);          // Nạp layout trực tiếp từ stream XML
        return true;

        // Trả mẫu: không có layout trong DB
        //return false;
    }

    public async Task<int?> GetLatestVersionAsync(int commandTimeoutSeconds = 15, CancellationToken ct = default)
    {
        using var conn = new SqlConnection(_connString);
        return await conn.ExecuteScalarAsync<int?>(
            sql: "dbo.ReportLayout_GetLatestVersion",
            param: new { ReportName = _reportName },
            commandType: CommandType.StoredProcedure,
            commandTimeout: commandTimeoutSeconds
        );
    }

    /// <summary>
    /// Lấy Version hiện tại (phục vụ polling hoặc so sánh)
    /// </summary>
    /// <returns></returns>
    public async Task<int?> GetVersionAsync()
    {
        using var conn = new SqlConnection(_connString);
        return await conn.ExecuteScalarAsync<int?>(
            "dbo.ReportLayout_GetVersion",
            new { ReportName = _reportName },
            commandType: CommandType.StoredProcedure);
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