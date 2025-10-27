using System;

namespace Winforms_App_Template.Database.Model
{
    public sealed class ReportLayoutVersion_Model
    {
        public long LayoutId { get; set; }     // IDENTITY
        public string ReportName { get; set; } = "";
        public int Version { get; set; }     // Tăng dần theo ReportName
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedBy { get; set; } = "";
        public byte[] ContentGz { get; set; } = Array.Empty<byte>(); // GZip của XML
        public byte[] Sha256 { get; set; } = Array.Empty<byte>(); // checksum XML gốc (32 bytes)
    }
}
