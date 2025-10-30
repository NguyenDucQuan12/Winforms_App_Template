using System;
using System.Collections.Generic;
using System.Data;

namespace Winforms_App_Template.Report
{
    /// <summary>
    /// Mô tả một cột (field) hiển thị trong Field List:
    /// - Name:  Tên trường thực (sẽ dùng trong Expression: [Name])
    /// - Type:  Kiểu .NET (để Designer hiểu kiểu dữ liệu)
    /// - Label: Nhãn hiển thị thân thiện trong Field List (sẽ gán vào DataColumn.Caption)
    /// </summary>
    public sealed class ColumnSpec
    {
        public string Name;           // Tên cột trong DB để binding (ví dụ: "NguoiTT")
        public Type Type;             // Kiểu dữ liệu trong .NET (typeof(string), typeof(int), ...)
        public string Label;          // Tên hiển thị (ví dụ: "Người thao tác")

        public ColumnSpec(string name, Type type, string? label)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Không thể bỏ trống tên cột", nameof(name));

            this.Name = name;
            this.Type = type ?? typeof(string);
            this.Label = label ?? name; // mặc định Label = Name nếu không truyền
        }
    }

    /// <summary>
    /// Quản lý danh sách cột whitelisted:
    /// - Add/Remove cột
    /// - Đổi nhãn hiển thị (Label) mà KHÔNG đổi tên kỹ thuật (Name)
    /// - Sinh DataTable schema rỗng cho Designer (Field List)
    /// 
    /// Lưu ý: Designer hiển thị caption/label của DataColumn nếu version DevExpress hỗ trợ.
    /// Nếu Field List không dùng Caption, vẫn an toàn vì Expression dùng [Name] gốc.
    /// </summary>
    public sealed class FieldWhitelist
    {
        // Bản đồ Name → ColumnSpec để quản lý dễ dàng
        // Độ so sánh Ordinal để phân biệt hoa thường trong tên cột.
        // Ví dụ: "MaKT" khác "MAKT"
        private readonly Dictionary<string, ColumnSpec> _map =
            new Dictionary<string, ColumnSpec>(StringComparer.Ordinal);

        /// <summary>
        /// Thêm mới hoặc cập nhật một cột theo Name.
        /// </summary>
        public FieldWhitelist Add(string name, Type type, string? label)
        {
            // Nếu đã tồn tại cùng name → ghi đè spec, coi như "update"
            _map[name] = new ColumnSpec(name, type, label);
            return this; // cho phép chain .Add(...).Add(...)
        }

        /// <summary>
        /// Xoá cột theo Name. Trả true nếu xoá được, false nếu không có.
        /// </summary>
        public bool Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return _map.Remove(name);
        }

        /// <summary>
        /// Đổi nhãn hiển thị (Label) cho cột theo Name.
        /// Không đổi Name để không phá Expression đã kéo thả.
        /// </summary>
        public bool SetLabel(string name, string newLabel)
        {
            // Nếu không truyền Name hoặc không tìm thấy → false
            if (string.IsNullOrWhiteSpace(name)) return false;
            // Tạo biến chứa thông tin của cột
            ColumnSpec? spec;

            // Tìm cột theo Name đã có
            if (!_map.TryGetValue(name, out spec)) return false;
            // Cập nhật nhãn hiển thị
            spec.Label = string.IsNullOrWhiteSpace(newLabel) ? spec.Name : newLabel;
            // Ghi đè lại
            _map[name] = spec;
            return true;
        }

        /// <summary>
        /// Kiểm tra có cột này trong whitelist hay chưa.
        /// </summary>
        public bool Contains(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return _map.ContainsKey(name);
        }

        /// <summary>
        /// Trả danh sách cột đang có (bản copy đọc-được).
        /// </summary>
        public IReadOnlyCollection<ColumnSpec> GetAll()
        {
            return new List<ColumnSpec>(_map.Values).AsReadOnly();
        }

        /// <summary>
        /// Sinh DataTable schema rỗng phục vụ Field List của Designer trong chế độ End-User Design.
        /// - ColumnName = Name (dùng trong Expression: [Name])
        /// - Caption    = Label (nhãn thân thiện hiển thị — tuỳ version Designer)
        /// - DataType   = Type
        /// 
        /// Lưu ý: KHÔNG sinh dữ liệu; chỉ schema. Runtime vẫn bind theo từng band/subreport.
        /// </summary>
        public DataTable ToDesignSchema(string tableName)
        {
            // Nếu không truyền tên bảng → dùng tên mặc định
            if (string.IsNullOrWhiteSpace(tableName))
                tableName = "DesignSchema";

            // Tạo DataTable rỗng với tên bảng
            var dt = new DataTable(tableName);

            // Thêm từng cột theo spec đã lưu
            foreach (var spec in _map.Values)
            {
                // Tạo DataColumn với tên kỹ thuật (Name) để drag & drop sinh [Name] đúng.
                var col = new DataColumn(spec.Name, spec.Type);

                // Gắn Caption = Label để Field List/tooltip (nếu hỗ trợ) hiện nhãn đẹp.
                col.Caption = string.IsNullOrWhiteSpace(spec.Label) ? spec.Name : spec.Label;

                // Thêm cột vào DataTable
                dt.Columns.Add(col);
            }

            return dt;
        }
    }

    /// <summary>
    /// Registry các whitelist dùng trong report.
    /// </summary>
    public static class FieldWhitelistRegistry
    {
        // WHITELIST cho report chính (Catthoong_ReportRow)
        public static readonly FieldWhitelist Catthoong = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("SLSudung", typeof(int), "Số lượng sử dụng")
            .Add("Remark", typeof(string), "Ghi chú")
            .Add("val1", typeof(string), "Ống dài sử dụng")
            .Add("val2", typeof(string), "Số lượng ống dài cắt được")
            .Add("val3", typeof(string), "Mã quản lý thickness gauge")
            .Add("val4", typeof(string), "Đường kính ngoài ống dài")
            .Add("val5", typeof(string), "Đường kính ngoài ống dài yes no")
            .Add("val6", typeof(string), "Mã pingauge 098mm")
            .Add("val7", typeof(string), "Đường kính trong loại 4Fr, 4KFr xuyên (yes no)")
            .Add("val8", typeof(string), "Đường kính trong loại 4Fr, 4KFr không xuyên (yes no)")
            .Add("val9", typeof(string), "Trạng thái cắt 10 ống")
            .Add("val10", typeof(string), "Mã thước sử dụng")
            .Add("val11", typeof(string), "Thước sử dụng 1")
            .Add("val12", typeof(string), "Thước sử dụng 2")
            .Add("val13", typeof(string), "Thước sử dụng 3")
            .Add("val14", typeof(string), "Thước sử dụng yes no")
            .Add("val15", typeof(string), "Kết quả xác nhận tồn lưu yes no");

        // Mở rộng thêm:
        // .Add("val1", typeof(string), "Ống dài sử dụng")
        // .Add("Bevel_Cut", typeof(int), "Lỗi cắt vát")

        // WHITELIST cho subreport (Standard_Model)
        public static readonly FieldWhitelist Standard_Catthoong = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("Loai_size", typeof(string), "Kích cỡ Fr")
            .Add("Loai_chieudai", typeof(string), "Chủng loại")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");


        // Mở rộng:
        // .Add("Loai_size", typeof(string), "Loại size")
    }
}
