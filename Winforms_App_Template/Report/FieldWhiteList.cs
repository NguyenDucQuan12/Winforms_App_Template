using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using Winforms_App_Template.Database;
using Winforms_App_Template.Database.Model;
using Winforms_App_Template.Database.Table;

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
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
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
            .Add("val15", typeof(string), "Kết quả xác nhận tồn lưu yes no")
            .Add("Cat_vat", typeof(string), "Lỗi cắt vát")
            .Add("Bep", typeof(string), "Lỗi bẹp")
            .Add("Bavia", typeof(string), "Lỗi Bavia")
            .Add("Roi", typeof(string), "Lỗi rơi")
            .Add("Chieu_dai_ngoai_tieu_chuan", typeof(string), "Lỗi ngoài tiêu chuẩn")
            .Add("Khac", typeof(string), "Lỗi khác");

        // Mở rộng thêm:
        // .Add("val1", typeof(string), "Ống dài sử dụng")
        // .Add("Bevel_Cut", typeof(int), "Lỗi cắt vát")

        // WHITELIST cho subreport (Standard_Model)
        public static readonly FieldWhitelist Standard_Catthoong = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("Loai_size", typeof(string), "Kích cỡ Fr")
            .Add("Pingauge_xuyen", typeof(string), "Pingauge xuyên")
            .Add("Pingauge_khong_xuyen", typeof(string), "Pingauge không xuyên")
            .Add("Loai_size", typeof(string), "Kích cỡ Fr")
            .Add("Loai_chieudai", typeof(string), "Chủng loại")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        // Mở rộng:
        // .Add("Loai_size", typeof(string), "Loại size"

        public static readonly FieldWhitelist Kiemtrasaucattho = new FieldWhitelist()
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Bàn thao tác")
            .Add("SLSudung", typeof(int), "Số lượng sử dụng")
            .Add("Remark", typeof(string), "Ghi chú")
            .Add("val1", typeof(string), "Mã pingauge 1")
            .Add("val2", typeof(string), "Mã pingauge 2")
            .Add("val3", typeof(string), "Độ dài pingauge 1")
            .Add("val4", typeof(string), "Độ dài pingauge 2")
            .Add("val5", typeof(string), "Số lượng kiểm tra 1")
            .Add("val6", typeof(string), "Số lượng kiểm tra 2")
            .Add("val7", typeof(string), "Số lượng OK 1")
            .Add("val8", typeof(string), "Số lượng OK 2")
            //.Add("val9", typeof(string), "Số lượng NG 1")
            //.Add("val10", typeof(string), "Số lượng NG 2")
            .Add("val11", typeof(string), "Mã quản lý mẫu giới hạn")
            .Add("val13", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
            .Add("Di_vat_duc", typeof(string), "Lỗi dị vật đúc")
            .Add("Xuoc", typeof(string), "Lỗi xước")
            .Add("Cong", typeof(string), "Lỗi Cong")
            .Add("Di_vat_ban", typeof(string), "Lỗi dị vật bẩn")
            .Add("Lo_thung", typeof(string), "Lỗi lỗ thủng")
            .Add("loi_lom", typeof(string), "Lỗi lồi lõm")
            .Add("Nham_xu_long", typeof(string), "Lỗi nhám xù lông")
            .Add("KTNQ_loi_lom", typeof(string), "Kiểm tra ngoại quan lỗi lõm")
            .Add("KTNG_Khac", typeof(string), "Kiểm tra ngoại quan khác")
            .Add("Khac", typeof(string), "Lỗi khác")
            .Add("NG_xuyen_qua_1", typeof(string), "Số lượng NG 1")
            .Add("NG_xuyen_qua_2", typeof(string), "Số lượng NG 2");


        public static readonly FieldWhitelist Kiemtrasaucattho_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("Pingauge_xuyen", typeof(string), "Pingauge xuyên")
            .Add("Pingauge_khong_xuyen", typeof(string), "Pingauge không xuyên")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Camchot = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("SLSudung", typeof(int), "Số lượng thao tác")
            .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
            .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")
            .Add("val1", typeof(string), "U1")
            .Add("val2", typeof(string), "U2")
            .Add("val3", typeof(string), "U3")
            .Add("val4", typeof(string), "U4")
            .Add("val5", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
            .Add("Nong_sau", typeof(string), "Lỗi Nông sâu (độ sâu cắm không đạt)")
            .Add("Bep_gap_ong", typeof(string), "Lỗi Bẹp ống")
            .Add("Bat_thuong_may", typeof(string), "Lỗi Bất thường máy")
            .Add("Roi", typeof(string), "Lỗi Rơi")
            .Add("Khac", typeof(string), "Lỗi khác")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Camchot_DKM = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("Thoigian", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "số máy sản xuất")
            .Add("val1", typeof(string), "Mã quản lý thước kẹp")
            .Add("val2", typeof(string), "Đường kính cắm chốt jig 1")
            .Add("val3", typeof(string), "Đường kính cắm chốt jig 2")
            .Add("val4", typeof(string), "Đường kính cắm chốt jig 3")
            .Add("val5", typeof(string), "Đường kính cắm chốt jig 4")
            .Add("val6", typeof(string), "Trạng thái jig cắm chốt 1")
            .Add("val7", typeof(string), "Trạng thái jig cắm chốt 2")
            .Add("val8", typeof(string), "Trạng thái jig cắm chốt 3")
            .Add("val9", typeof(string), "Trạng thái jig cắm chốt 4")
            .Add("val10", typeof(string), "Nhiệt độ bộ gia nhiệt 1")
            .Add("val11", typeof(string), "Nhiệt độ bộ gia nhiệt 2")
            .Add("val12", typeof(string), "Nhiệt độ bộ gia nhiệt 3")
            .Add("val13", typeof(string), "Nhiệt độ bộ gia nhiệt 4")
            .Add("val14", typeof(string), "Thời gian cắm")
            .Add("val15", typeof(string), "Miếng đệm cho vào bộ kẹp")
            .Add("val16", typeof(string), "Áp suất kẹp ống")
            .Add("val17", typeof(string), "Moment xoắn")
            .Add("val18", typeof(string), "Kết quả xác nhận thiết bị")
            .Add("Remark", typeof(string), "Ghi chú");


        public static readonly FieldWhitelist Camchot_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("Pingauge_xuyen", typeof(string), "Pingauge xuyên")
            .Add("Pingauge_khong_xuyen", typeof(string), "Pingauge không xuyên")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Dap_chuoi_cat_dinh_muc = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("SLSudung", typeof(int), "Số lượng thao tác")
            .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
            .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")

            .Add("val1", typeof(string), "Mã số quản lý dept gauge 1")
            .Add("val2", typeof(string), "Mã số quản lý dept gauge 2")
            .Add("val3", typeof(string), "Độ dập sâu 1")
            .Add("val4", typeof(string), "Độ dập sâu 2")
            .Add("val5", typeof(string), "Độ dập sâu 3")
            .Add("val6", typeof(string), "Xác nhận kích thước cắt 1")
            .Add("val7", typeof(string), "Xác nhận kích thước cắt 2")
            .Add("val8", typeof(string), "Xác nhận kích thước cắt 3")
            .Add("val9", typeof(string), "Xác nhận tình trạng dập")
            .Add("val13", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
            .Add("Bat_thuong_may", typeof(string), "Lỗi bất thường máy")
            .Add("Roi", typeof(string), "Lỗi Rơi")
            .Add("Khac", typeof(string), "Lỗi khác")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Dap_chuoi_cat_dinh_muc_DKM = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("Thoigian", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("val1", typeof(string), "Áp lực dập")
            .Add("val2", typeof(string), "Xác nhận trạng thái chốt dập")
            .Add("val3", typeof(string), "Xác nhận cơ cấu phòng tránh dập chuôi 2 lần")
            .Add("val4", typeof(string), "Xác nhận độ chính xác của thiết bị đo độ dập sâu")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");


        public static readonly FieldWhitelist Dap_chuoi_cat_dinh_muc_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("Pingauge_xuyen", typeof(string), "Pingauge xuyên")
            .Add("Pingauge_khong_xuyen", typeof(string), "Pingauge không xuyên")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Tu_dong_lap_rap_que_nong = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("SLSudung", typeof(int), "Số lượng thao tác")
            .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
            .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")

            .Add("val1", typeof(string), "Mã số quản lý thước vạch")
            .Add("val2", typeof(string), "Độ dập sâu 1")
            .Add("val3", typeof(string), "Độ dập sâu 2")
            .Add("val4", typeof(string), "Độ dập sâu 3")
            .Add("val5", typeof(string), "Độ dập sâu 4")
            .Add("val6", typeof(string), "Xác nhận kích thước cắt 1")
            .Add("val7", typeof(string), "Xác nhận kích thước cắt 2")
            .Add("val8", typeof(string), "Xác nhận kích thước cắt 3")
            .Add("val9", typeof(string), "Xác nhận kích thước cắt 4")
            .Add("val10", typeof(string), "Xác nhận tình trạng dập 1")
            .Add("val11", typeof(string), "Xác nhận tình trạng dập 2")
            .Add("val12", typeof(string), "Xác nhận tình trạng dập 3")
            .Add("val13", typeof(string), "Xác nhận tình trạng dập 4")
            .Add("val14", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
            .Add("NG_cam_chot", typeof(string), "Lỗi NG cắm chốt")
            .Add("NG_do_dap_sau", typeof(string), "Lỗi NG độ sâu")
            .Add("Bat_thuong_may", typeof(string), "Lỗi bất thường máy")
            .Add("Roi", typeof(string), "Lỗi Rơi")
            .Add("Khac", typeof(string), "Lỗi khác")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Tu_dong_lap_rap_que_nong_DKM = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("Thoigian", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("val1", typeof(string), "Xác nhận trạng thái jig cắm chốt 1")
            .Add("val2", typeof(string), "Xác nhận trạng thái jig cắm chốt 2")
            .Add("val3", typeof(string), "Xác nhận trạng thái jig cắm chốt 3")
            .Add("val4", typeof(string), "Xác nhận trạng thái jig cắm chốt 4")
            .Add("val5", typeof(string), "Xác nhận trạng thái jig cắm chốt 5")
            .Add("val6", typeof(string), "Xác nhận trạng thái jig cắm chốt 6")
            .Add("val7", typeof(string), "Xác nhận trạng thái jig cắm chốt 7")
            .Add("val8", typeof(string), "Xác nhận trạng thái jig cắm chốt 8")
            .Add("val9", typeof(string), "Trạng thái chốt dập 1")
            .Add("val10", typeof(string), "Trạng thái chốt dập 2")
            .Add("val11", typeof(string), "Nhiệt độ bộ gia nhiệt A")
            .Add("val12", typeof(string), "Nhiệt độ bộ gia nhiệt B")
            .Add("val13", typeof(string), "Thời gian cắm")
            .Add("val14", typeof(string), "Áp suất kẹp ống")
            .Add("val15", typeof(string), "Vị trí ấn của Robocylinder cắm ống vào chốt A")
            .Add("val16", typeof(string), "Vị trí ấn của Robocylinder cắm ống vào chốt B")
            .Add("val17", typeof(string), "Vị trí cắt robocylinder nâng hạ dao cắt")
            .Add("val18", typeof(string), "Áp suất dập")
            .Add("val19", typeof(string), "Hoạt động của thiết bị kiểm tra trạng thái cắm chốt")
            .Add("val20", typeof(string), "Độ chính xác của thiết bị đo độ dập sâu")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");


        public static readonly FieldWhitelist Tu_dong_lap_rap_que_nong_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("Pingauge_xuyen", typeof(string), "Pingauge xuyên")
            .Add("Pingauge_khong_xuyen", typeof(string), "Pingauge không xuyên")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Gia_cong_dau_mut_v1_5 = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("SLSudung", typeof(int), "Số lượng thao tác")
            .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
            .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")

            .Add("val1", typeof(string), "Lô dung môi 1233Z/MDX 1")
            .Add("val2", typeof(string), "Lô dung môi 1233Z/MDX 2")
            .Add("val3", typeof(string), "Hạn sử dụng 1233Z/MDX 1")
            .Add("val4", typeof(string), "Hạn sử dụng 1233Z/MDX 2")
            .Add("val5", typeof(string), "Hạn sử dụng 1233Z 1")
            .Add("val6", typeof(string), "Hạn sử dụng 1233Z 2")
            .Add("val7", typeof(string), "Lô dung môi 1233Z 1")
            .Add("val8", typeof(string), "Lô dung môi 1233Z 2")
            .Add("val9", typeof(string), "Hạn sử dụng yes no 1233Z/MDX 1")
            .Add("val10", typeof(string), "Hạn sử dụng yes no 1233Z/MDX 2")
            .Add("val11", typeof(string), "Hạn sử dụng yes no 1233Z 1")
            .Add("val12", typeof(string), "Hạn sử dụng yes no 1233Z 2")
            .Add("val13", typeof(string), "Mã pingauge/kích cỡ pingauge 1")
            .Add("val14", typeof(string), "Mã pingauge/kích cỡ pingauge 2")
            .Add("val15", typeof(string), "Chiều dài mã pingauge/kích cỡ pingauge 1")
            .Add("val16", typeof(string), "Chiều dài mã pingauge/kích cỡ pingauge 2")
            .Add("val17", typeof(string), "Đường kính trong đầu mút xuyên")
            .Add("val18", typeof(string), "Đường kính trong đầu mút không xuyên")
            .Add("val19", typeof(string), "Tình trạng gia công (5pcs/unit) U1")
            .Add("val20", typeof(string), "Tình trạng gia công (5pcs/unit) U2")
            .Add("val21", typeof(string), "Tình trạng gia công (5pcs/unit) U3")
            .Add("val22", typeof(string), "Mã quản lý mẫu giới hạn")
            .Add("val23", typeof(string), "Mã thước vạch")
            .Add("val24", typeof(string), "Chiều dài U1 1")
            .Add("val25", typeof(string), "Chiều dài U1 2")
            .Add("val26", typeof(string), "Chiều dài U1 3")
            .Add("val27", typeof(string), "Chiều dài U1 4")
            .Add("val28", typeof(string), "Chiều dài U2 1")
            .Add("val29", typeof(string), "Chiều dài U2 2")
            .Add("val30", typeof(string), "Chiều dài U2 3")
            .Add("val31", typeof(string), "Chiều dài U2 4")
            .Add("val32", typeof(string), "Chiều dài U3 1")
            .Add("val33", typeof(string), "Chiều dài U3 2")
            .Add("val34", typeof(string), "Chiều dài U3 3")
            .Add("val35", typeof(string), "Chiều dài U3 4")
            .Add("val36", typeof(string), "Chiều dài U1 yes no")
            .Add("val37", typeof(string), "Chiều dài U2 yes no")
            .Add("val38", typeof(string), "Chiều dài U3 yes no")
            .Add("val39", typeof(string), "Số lượng lõi kim loại sử dụng")
            .Add("val40", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
            .Add("Bavia", typeof(string), "Lỗi bavia")
            .Add("Thung", typeof(string), "Lỗi thủng")
            .Add("Sut", typeof(string), "Lỗi sứt")
            .Add("Lom_thieu_nhua", typeof(string), "Lỗi Lõm, thiếu nhựa") 
            .Add("Di_vat_ban_khuon", typeof(string), "Lỗi Dị vật bẩn khuôn") 
            .Add("Di_vat_duc", typeof(string), "Lỗi dị vật đúc")
            .Add("Xuoc", typeof(string), "Lỗi xước")
            .Add("Ngan", typeof(string), "Lỗi ngấn")
            .Add("Mang_ca", typeof(string), "Lỗi mang cá")
            .Add("Ran_ong", typeof(string), "Lỗi rạn ống")
            .Add("Dap_dau_mut", typeof(string), "Lỗi Dập đầu mút")
            .Add("Nut_vo", typeof(string), "Lỗi Nứt, vỡ")
            .Add("Vang_chay_dau_mut", typeof(string), "Lỗi Vàng cháy đầu mút")
            .Add("Gia_cong_chua_hoan_thien", typeof(string), "Lỗi Gia công chưa hoàn thiện")
            .Add("Loi", typeof(string), "Lỗi Lồi")
            .Add("Cong_bien_dang", typeof(string), "Lỗi Cong, biến dạng")
            .Add("Thieu_linh_kien", typeof(string), "Lỗi Thiếu linh kiện")
            .Add("Bat_thuong_may", typeof(string), "Lỗi Bất thường máy")
            .Add("Roi", typeof(string), "Lỗi rơi")
            .Add("Khac", typeof(string), "Lỗi khác")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Gia_cong_dau_mut_v1_5_DKM = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("Thoigian", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("val1", typeof(string), "Mã quản lý thickness gauge/thước vạch 1")
            .Add("val2", typeof(string), "Mã quản lý thickness gauge/thước vạch 2")
            .Add("val3", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 1")
            .Add("val4", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 2")
            .Add("val5", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 3")
            .Add("val6", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 4")
            .Add("val7", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 1")
            .Add("val8", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 2")
            .Add("val9", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 3")
            .Add("val10", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 4")
            .Add("val23", typeof(string), "Thời gian phát chấn 1")
            .Add("val24", typeof(string), "Thời gian phát chấn 2")
            .Add("val25", typeof(string), "Thời gian phát chấn 3")
            .Add("val26", typeof(string), "Thời gian thổi khí 1 1")
            .Add("val27", typeof(string), "Thời gian thổi khí 1 2")
            .Add("val28", typeof(string), "Thời gian thổi khí 1 3")
            .Add("val29", typeof(string), "Thời gian thổi khí 2 1")
            .Add("val30", typeof(string), "Thời gian thổi khí 2 2")
            .Add("val31", typeof(string), "Thời gian thổi khí 2 3")
            .Add("val32", typeof(string), "Thời gian tiến xylanh A 1")
            .Add("val33", typeof(string), "Thời gian tiến xylanh A 2")
            .Add("val34", typeof(string), "Thời gian tiến xylanh A 3")
            .Add("val35", typeof(string), "Áp suất đẩy xylanh B 1")
            .Add("val36", typeof(string), "Áp suất đẩy xylanh B 2")
            .Add("val37", typeof(string), "Áp suất đẩy xylanh B 3")
            .Add("val38", typeof(string), "Vị trí cuộn dây 1")
            .Add("val39", typeof(string), "Vị trí cuộn dây 2")
            .Add("val40", typeof(string), "Vị trí cuộn dây 3")
            .Add("val41", typeof(string), "Giá trị Panme 1")
            .Add("val42", typeof(string), "Giá trị Panme 2")
            .Add("val43", typeof(string), "Giá trị Panme 3")
            .Add("val44", typeof(string), "Áp suất khí thổi 1")
            .Add("val45", typeof(string), "Áp suất khí thổi 2")
            .Add("val46", typeof(string), "Áp suất khí thổi 3")
            .Add("val74", typeof(string), "Ngoại quan lõi kim loại")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Gia_cong_dau_mut_v1_5_dkm_Standard = new FieldWhitelist()
            .Add("TenMay_Ban", typeof(string), "Máy")
            .Add("val3", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 1")
            .Add("val4", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 2")
            .Add("val5", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 3")
            .Add("val6", typeof(string), "Đường kính ngoài đầu mút lõi kim loại (mm) 4")
            .Add("val7", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 1")
            .Add("val8", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 2")
            .Add("val9", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 3")
            .Add("val10", typeof(string), "Chiều dài hữu hiệu của lõi kim loại (mm) 4")
            .Add("val23", typeof(string), "Thời gian phát chấn 1")
            .Add("val24", typeof(string), "Thời gian phát chấn 2")
            .Add("val25", typeof(string), "Thời gian phát chấn 3")
            .Add("val26", typeof(string), "Thời gian thổi khí 1 1")
            .Add("val27", typeof(string), "Thời gian thổi khí 1 2")
            .Add("val28", typeof(string), "Thời gian thổi khí 1 3")
            .Add("val29", typeof(string), "Thời gian thổi khí 2 1")
            .Add("val30", typeof(string), "Thời gian thổi khí 2 2")
            .Add("val31", typeof(string), "Thời gian thổi khí 2 3")
            .Add("val32", typeof(string), "Thời gian tiến xylanh A 1")
            .Add("val33", typeof(string), "Thời gian tiến xylanh A 2")
            .Add("val34", typeof(string), "Thời gian tiến xylanh A 3")
            .Add("val35", typeof(string), "Áp suất đẩy xylanh B 1")
            .Add("val36", typeof(string), "Áp suất đẩy xylanh B 2")
            .Add("val37", typeof(string), "Áp suất đẩy xylanh B 3")
            .Add("val38", typeof(string), "Vị trí cuộn dây 1")
            .Add("val39", typeof(string), "Vị trí cuộn dây 2")
            .Add("val40", typeof(string), "Vị trí cuộn dây 3")
            .Add("val41", typeof(string), "Giá trị Panme 1")
            .Add("val42", typeof(string), "Giá trị Panme 2")
            .Add("val43", typeof(string), "Giá trị Panme 3")
            .Add("val44", typeof(string), "Áp suất khí thổi 1")
            .Add("val45", typeof(string), "Áp suất khí thổi 2")
            .Add("val46", typeof(string), "Áp suất khí thổi 3")
            .Add("val74", typeof(string), "Ngoại quan lõi kim loại");

        public static readonly FieldWhitelist Gia_cong_dau_mut_v1_5_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("Loai_size", typeof(string), "Kích cỡ Fr")
            .Add("Loai_chieudai", typeof(string), "Chủng loại")
            .Add("Pingauge_xuyen", typeof(string), "Pingauge xuyên")
            .Add("Pingauge_khong_xuyen", typeof(string), "Pingauge không xuyên");

        public static readonly FieldWhitelist Rua_dau_mut_que_nong = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Mã kiểm tra")
            .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("SLSudung", typeof(int), "Số lượng sử dụng")
            .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
            .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")
            .Add("val1", typeof(string), "Lô 1233Z/IPA")
            .Add("val2", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
            .Add("Roi", typeof(string), "Lỗi Rơi")
            .Add("Khac", typeof(string), "Lỗi khác")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Kiem_tra_ngoai_quan = new FieldWhitelist()
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
            .Add("TenMay_Ban", typeof(string), "Bàn thao tác số")
            .Add("SLSudung", typeof(int), "Số lượng sử dụng")
            .Add("Remark", typeof(string), "Ghi chú")
            .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
            .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")
            .Add("val1", typeof(string), "Mã kích cỡ pingauge 1")
            .Add("val2", typeof(string), "Độ dài mã pingauge 1")
            .Add("val3", typeof(string), "Đường kính trong đầu mút xuyên")
            .Add("val4", typeof(string), "Mã kích cỡ pingauge 2")
            .Add("val5", typeof(string), "Độ dài mã pingauge 2")
            .Add("val6", typeof(string), "Đường kính trong đầu mút không xuyên")
            .Add("val7", typeof(string), "Mã số quản lý thước đo")
            .Add("val8", typeof(string), "Chiều dài hữu hiệu")
            .Add("val9", typeof(string), "Mã bảng tiêu chuẩn dị vật")
            .Add("val10", typeof(string), "Mã quản lý mẫu giới hạn")
            .Add("val11", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
            .Add("Bavia", typeof(string), "Lỗi bavia")
            .Add("Thung", typeof(string), "Lỗi thủng")
            .Add("Sut", typeof(string), "Lỗi sứt")
            .Add("Roi", typeof(string), "Lỗi rơi")
            .Add("Di_vat_ban_khuon", typeof(string), "Lỗi Dị vật bẩn khuôn")
            .Add("Di_vat_duc", typeof(string), "Lỗi dị vật đúc")
            .Add("Xuoc", typeof(string), "Lỗi xước")
            .Add("Ngan", typeof(string), "Lỗi ngấn")
            .Add("Mang_ca", typeof(string), "Lỗi mang cá")
            .Add("Ran_ong", typeof(string), "Lỗi rạn ống")
            .Add("Vang_chay_dau_mut", typeof(string), "Lỗi Vàng cháy đầu mút")
            .Add("Dap_dau_mut", typeof(string), "Lỗi Dập đầu mút")
            .Add("Nut_vo", typeof(string), "Lỗi Nứt, vỡ")
            .Add("Gia_cong_chua_hoan_thien", typeof(string), "Lỗi Gia công chưa hoàn thiện")
            .Add("Cong_bien_dang", typeof(string), "Lỗi Cong, biến dạng")
            .Add("Thieu_linh_kien", typeof(string), "Lỗi Thiếu linh kiện")
            .Add("Loi", typeof(string), "Lỗi Lồi")
            .Add("Lom_thieu_nhua", typeof(string), "Lỗi Lõm, thiếu nhựa")
            .Add("Khac", typeof(string), "Lỗi khác");

        // WHITELIST cho subreport (Standard_Model)
        public static readonly FieldWhitelist Kiem_tra_ngoai_quan_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("Pingauge_xuyen", typeof(string), "Pingauge xuyên")
            .Add("Pingauge_khong_xuyen", typeof(string), "Pingauge không xuyên")
            .Add("Loai_size", typeof(string), "Kích cỡ Fr")
            .Add("Loai_chieudai", typeof(string), "Chủng loại")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Xu_ly_silicon = new FieldWhitelist()
           .Add("MaKT", typeof(string), "Mã kiểm tra")
           .Add("Ly_do_kiem_tra", typeof(string), "Lý do kiểm tra")
           .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
           .Add("NguoiTT", typeof(string), "Người thao tác")
           .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
           .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
           .Add("SLSudung", typeof(int), "Số lượng thao tác")
           .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
           .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")
           .Add("val1", typeof(string), "Số lô dung dịch silicon sử dụng")
           .Add("val2", typeof(string), "Kết quả xác nhận tồn lưu")
           // Ghi chú
           .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Kiem_tra_lan_cuoi = new FieldWhitelist()
           .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
           .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenNguoiThaoTac", typeof(string), "Tên người thao tác")
           .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
           .Add("SLSudung", typeof(int), "Số lượng thao tác")
           .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
           .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")
           .Add("val1", typeof(string), "Mã bảng tiêu chuẩn dị vật")
           .Add("val2", typeof(string), "Mã quản lý mẫu giới hạn")
           .Add("val3", typeof(string), "Kết quả xác nhận tồn lưu")
            // Chi tiết lỗi
           .Add("Bavia", typeof(string), "Lỗi bavia")
           .Add("Thung", typeof(string), "Lỗi thủng")
           .Add("Sut", typeof(string), "Lỗi sứt")
           .Add("Lom", typeof(string), "Lỗi lõm")
           .Add("Di_vat_ban_khuon", typeof(string), "Lỗi Dị vật bẩn khuôn")
           .Add("Di_vat_duc", typeof(string), "Lỗi dị vật đúc")
           .Add("Xuoc", typeof(string), "Lỗi xước")
           .Add("Ngan", typeof(string), "Lỗi ngấn")
           .Add("Mang_ca", typeof(string), "Lỗi mang cá")
           .Add("Ran_ong", typeof(string), "Lỗi rạn ống")
           .Add("Vang_chay_dau_mut", typeof(string), "Lỗi Vàng cháy đầu mút")
           .Add("Dap_dau_mut", typeof(string), "Lỗi Dập đầu mút")
           .Add("Nut_vo", typeof(string), "Lỗi Nứt, vỡ")
           .Add("Thieu_nhua", typeof(string), "Lỗi thiếu nhựa")
           .Add("Gia_cong_chua_hoan_thien", typeof(string), "Lỗi Gia công chưa hoàn thiện")
           .Add("Cong_bien_dang", typeof(string), "Lỗi Cong, biến dạng")
           .Add("Thieu_linh_kien", typeof(string), "Lỗi Thiếu linh kiện")
           .Add("Loi", typeof(string), "Lỗi Lồi")
           .Add("Roi", typeof(string), "Lỗi rơi")
           .Add("Khac", typeof(string), "Lỗi khác")
           // Ghi chú
           .Add("Remark", typeof(string), "Ghi chú");

        public static readonly FieldWhitelist Tong_ket = new FieldWhitelist()
           .Add("val1", typeof(string), "Số lô dung dịch silicon sử dụng")
           .Add("val2", typeof(string), "Số lượng mẫu kiểm tra công đoạn trả lại")
           .Add("val3", typeof(string), "Số lượng hàng chuyển sang công đoạn sau (hàng phù hợp)")
           .Add("val4", typeof(string), "Người xác nhận - hủy")
           .Add("val8", typeof(string), "Ngày/tháng xác nhận - hủy")
           .Add("val5", typeof(string), "Số lượng hàng không phù hợp")
           .Add("val6", typeof(string), "Hủy")
           .Add("val7", typeof(string), "Mẻ cuối lô");

        public static readonly FieldWhitelist Summary_Report = new FieldWhitelist()
           .Add("ItemNumber", typeof(string), "Mã sản phẩm")
           .Add("LotNo", typeof(string), "Số lô sản xuất")
           // Loại dòng tổng kết: "CRS25" hoặc "RS25"
           .Add("Loai", typeof(string), "Loại tổng kết (CRS25 / RS25)")
           .Add("So_luong_su_dung", typeof(int), "Tổng số lượng sử dụng")
           .Add("So_hang_phu_hop", typeof(int), "Tổng số hàng phù hợp")
           .Add("So_hang_khong_phu_hop", typeof(int), "Tổng số hàng không phù hợp");
    }

    /// Tự động tạo các filed tự động với dữ liệu đã có ở DB
    public sealed class Auto_Build_FieldWhiteList
    {
        /// <summary>
        /// Repo truy vấn bảng tblMasterForm_Controls.
        /// </summary>
        private MasterForm_Table _masterFormRepo;
        private MasterFormControl_Table _masterFormcontrolRepo;

        /// <summary>
        /// Cache nội bộ: lưu map idMasterForm => FieldWhitelist đã build.
        /// Dùng để tránh truy vấn DB nhiều lần cho cùng 1 form.
        /// </summary>
        private readonly Dictionary<int, FieldWhitelist> _cache = new Dictionary<int, FieldWhitelist>(capacity: 16);

        /// <summary>
        /// Lock để bảo vệ cache trong môi trường đa luồng.
        /// </summary>
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Khởi tạo instance registry với repo cho bảng controls.
        /// Truyền repo thực tế từ Form (vd: new MasterFormControls_Table(executor)).
        /// </summary>
        /// <param name="masterFormControlsRepo">Repo truy vấn tblMasterForm_Controls</param>
        public Auto_Build_FieldWhiteList(DbExecutor? db = null)
        {
            var executor = db ?? new DbExecutor();
            _masterFormRepo = new MasterForm_Table(executor);
            _masterFormcontrolRepo = new MasterFormControl_Table(executor);
        }

        /// <summary>
        /// Trả về FieldWhitelist cho 1 idMasterForm cụ thể.
        /// - Nếu đã có trong cache → trả nhanh.
        /// - Nếu chưa có → truy vấn DB, build whitelist, lưu cache và trả về.
        /// </summary>
        /// <param name="idMasterForm">Id của form (idMasterForm)</param>
        /// <param name="ct">CancellationToken để hủy</param>
        public async Task<FieldWhitelist> GetWhitelistForFormAsync(int idMasterForm, CancellationToken ct = default)
        {
            // 1) Kiểm tra cache nhanh (không lock) — tránh overhead lock nếu đã tồn tại.
            if (_cache.TryGetValue(idMasterForm, out var cached))
            {
                return cached; // trả ngay
            }

            // 2) Nếu không có trong cache -> lock để thực hiện truy vấn & build
            await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // Double-check: có thể một task khác đã build trong lúc chờ lock
                if (_cache.TryGetValue(idMasterForm, out cached))
                    return cached;

                // 3) Truy vấn repo: lấy tất cả control cho idMasterForm này
                var controlRows = await _masterFormcontrolRepo.Get_Detail_Control(idMasterForm, ct).ConfigureAwait(false);

                // 4) Build FieldWhitelist từ rows trả về
                var whitelist = BuildWhitelistFromRows(controlRows);

                // 5) Lưu vào cache
                _cache[idMasterForm] = whitelist;

                // 6) Trả kết quả
                return whitelist;
            }
            finally
            {
                // Giải phóng lock (quan trọng!)
                _cacheLock.Release();
            }
        }

        /// <summary>
        /// Trả về map idMasterForm -> FieldWhitelist cho nhiều id 1 lượt.
        /// - Thực hiện truy vấn batch (nếu repo hỗ trợ), build tất cả và trả về dictionary.
        /// - Dùng để khởi tạo nhiều form cùng lúc.
        /// </summary>
        public async Task<Dictionary<int, FieldWhitelist>> GetWhitelistsForFormsAsync(IEnumerable<int> idCongDoan, CancellationToken ct = default)
        {
            // Chuẩn hóa input thành mảng unique
            var ids = idCongDoan?.Distinct().ToArray() ?? Array.Empty<int>();
            var result = new Dictionary<int, FieldWhitelist>(ids.Length);

            if (ids.Length == 0) return result; // không có gì -> trả rỗng

            // Lấy danh sách id chưa có trong cache
            var idsToFetch = ids.Where(id => !_cache.ContainsKey(id)).ToArray();

            // Nếu có id cần fetch -> gọi repo 1 lần
            if (idsToFetch.Length > 0)
            {
                // Lấy masterForm có phiên bản mới nhất
                var rows = await _masterFormRepo.Get_Latest_MasterForm(idsToFetch, ct).ConfigureAwait(false);

                // Group row theo id
                var grouped = rows.GroupBy(r => r.Id).ToDictionary(g => g.Key, g => g.ToList());

                // Build whitelist cho từng group và ghi vào cache
                await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    foreach (var id in grouped.Keys)
                    {
                        // Truy vấn các control trong form này
                        var rowsForId = await _masterFormcontrolRepo.Get_Detail_Control(idMasterForm: id, ct: ct);
                        var wl = BuildWhitelistFromRows(rowsForId);
                        _cache[id] = wl;
                    }
                }
                finally
                {
                    _cacheLock.Release();
                }
            }

            // 3) Bây giờ đọc cache cho tất cả ids -> trả về result
            foreach (var id in ids)
            {
                if (_cache.TryGetValue(id, out var wl))
                    result[id] = wl;
                else
                    result[id] = new FieldWhitelist(); // nếu không có rows -> trả empty whitelist
            }

            return result;
        }


        // =================================================================
        // PRIVATE: BUILDING LOGIC
        // =================================================================

        /// <summary>
        /// Tạo FieldWhitelist từ danh sách rows lấy từ DB.
        /// - Nếu rows null hoặc rỗng -> trả FieldWhitelist rỗng.
        /// - Map DBColumn -> Type + FriendlyName.
        /// - Heuristic để đoán Type: nếu tên chứa 'time' -> DateTime,
        ///   nếu chứa 'qty'/'số lượng' -> int, else string.
        /// </summary>
        private FieldWhitelist BuildWhitelistFromRows(List<MasterForm_Control_Model>? rows)
        {
            // Tạo whitelist mới
            var whitelist = new FieldWhitelist();

            // 0) Thêm các field header mặc định (theo mẫu bạn dùng trong các báo cáo).
            //    Nếu bạn đã có logic khác để thêm header, bạn có thể bỏ block này.
            whitelist
                .Add("MaKT", typeof(string), "Lý do kiểm tra")
                .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
                .Add("NguoiTT", typeof(string), "Người thao tác")
                .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
                .Add("SLSudung", typeof(int), "Số lượng sử dụng")
                .Add("OKQty", typeof(int), "Số lượng hàng phù hợp")
                .Add("NGQty", typeof(int), "Số lượng hàng không phù hợp")
                .Add("Remark", typeof(string), "Ghi chú");

            // Nếu không có rows -> trả whitelist chỉ chứa các header mặc định
            if (rows == null || rows.Count == 0) return whitelist;

            // 1) Tập các DBColumn đã thêm để tránh duplicate
            var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // mark header names as added để tránh DB trùng với header
            foreach (var h in new[] { "MaKT", "StartTime", "NguoiTT", "TenMay_Ban", "SLSudung", "OKQty", "NGQty", "Remark" })
                added.Add(h);

            // 2) Cố gắng lấy property "ControlType" hay "ControlKind" nếu model của bạn có (tự động, bằng phản chiếu).
            //    Nhiều model lưu kiểu control (TextBox/Numeric/YESNO/DateTime) ở property khác nhau — ta tìm mọi khả năng.
            var modelType = typeof(MasterForm_Control_Model);
            // Tên property khả dĩ có control type (tuỳ model)
            var possibleTypeProps = new[] { "ControlType", "ControlKind", "Type", "ControlFormat" };
            var controlTypeProperty = possibleTypeProps
                .Select(name => modelType.GetProperty(name))
                .FirstOrDefault(p => p != null);

            // 3) Duyệt từng row DB để thêm vào whitelist
            foreach (var r in rows)
            {
                // an toàn: lấy dbColumn, friendly
                var dbColumn = (r.DBColumn ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(dbColumn))
                {
                    // nếu DBColumn rỗng thì bỏ qua (không thể map)
                    continue;
                }

                // nếu đã thêm rồi thì skip
                if (added.Contains(dbColumn)) continue;

                // lấy friendlyName (nếu rỗng sẽ fallback về dbColumn)
                var friendly = string.IsNullOrWhiteSpace(r.FriendlyName) ? dbColumn : r.FriendlyName.Trim();

                // 4) cố gắng đọc control-type từ model nếu có (ví dụ "Numeric", "YESNO", "DateTime", "TextBox")
                string? controlTypeValue = null;
                if (controlTypeProperty != null)
                {
                    try
                    {
                        var val = controlTypeProperty.GetValue(r);
                        if (val != null) controlTypeValue = val.ToString();
                    }
                    catch
                    {
                        // silent: nếu phản chiếu thất bại thì bỏ qua, dùng heuristics phía dưới
                    }
                }

                // 5) Heuristic đoán kiểu: ưu tiên controlTypeValue, nếu không có thì dựa trên dbColumn/friendly
                var fieldType = DetectFieldType(controlTypeValue, dbColumn, friendly);

                // 6) Đưa vào whitelist (fluent Add)
                whitelist.Add(dbColumn, fieldType, friendly);

                // 7) đánh dấu đã thêm để tránh duplicate
                added.Add(dbColumn);
            }

            return whitelist;
        }

        /// <summary>
        /// Helpers: dựa trên controlType (nếu có) + dbColumn/friendly để đoán System.Type cho field.
        /// Quy tắc:
        ///  - Nếu controlType chứa "date" / "datetime" => DateTime
        ///  - Nếu controlType chứa "yesno" / "checkbox" => string (OK/NG) -> keep as string
        ///  - Nếu controlType chứa "numeric" => int cho các tên lượng (SL,Qty,Count...), ngược lại string (nhiều numeric là đo lường)
        ///  - Nếu dbColumn match ^val\d+$ => string
        ///  - Nếu dbColumn chứa "SLSudung|OKQty|NGQty|Qty|SL|Count" => int
        ///  - Nếu friendly chứa từ chỉ thời gian => DateTime
        ///  - Mặc định => string
        /// </summary>
        private static Type DetectFieldType(string? controlTypeValue, string dbColumn, string friendly)
        {
            // chuẩn hoá text cho so sánh
            var ctrl = (controlTypeValue ?? string.Empty).ToLowerInvariant();
            var text = (dbColumn + " " + friendly).ToLowerInvariant();

            // 1) controlType có precedence
            if (!string.IsNullOrEmpty(ctrl))
            {
                if (ctrl.Contains("date") || ctrl.Contains("datetime") || ctrl.Contains("time"))
                    return typeof(DateTime);

                // YESNO / checkbox / boolean-like: ta map về string (OK/NG, Yes/No) vì bạn hiển thị text.
                if (ctrl.Contains("yesno") || ctrl.Contains("checkbox") || ctrl.Contains("boolean"))
                    return typeof(string);

                if (ctrl.Contains("numeric") || ctrl.Contains("number") || ctrl.Contains("spin"))
                {
                    // nếu dbColumn rõ ràng là 1 counter/qty => int, else string to be safe (measurement might need string)
                    if (RegexIsIntegerLike(dbColumn, friendly))
                        return typeof(int);
                    // else fallback to string (keeps original formatting)
                    return typeof(string);
                }

                // TextBox / Label -> string
                if (ctrl.Contains("text") || ctrl.Contains("label"))
                    return typeof(string);
            }

            // 2) Nếu dbColumn là valN thì coi là string (theo convention của bạn)
            if (System.Text.RegularExpressions.Regex.IsMatch(dbColumn, @"^val\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return typeof(string);

            // 3) Nếu tên cột chứa ngày/thời gian
            if (text.Contains("thoigian") || text.Contains("thời gian") || text.Contains("ngày") || text.Contains("time") || text.Contains("date"))
                return typeof(DateTime);

            // 4) Nếu tên cột chứa 'số lượng' / qty / sl / okqty / ngqty -> int
            if (text.Contains("số lượng") || text.Contains("qty") || text.Contains("okqty") || text.Contains("ngqty") || text.Contains("sl "))
                return typeof(int);

            // 5) Nếu tên cột có dạng rõ ràng 'SLSudung' 'OKQty' 'NGQty' -> int (exact match)
            var intExact = new[] { "slsudung", "okqty", "ngqty", "slloi", "sldung", "sl" };
            if (intExact.Any(k => dbColumn.Equals(k, StringComparison.OrdinalIgnoreCase) || friendly.ToLowerInvariant().Contains(k)))
                return typeof(int);

            // 6) Mặc định: string (an toàn cho hiển thị & biểu diễn)
            return typeof(string);
        }

        /// <summary>
        /// Trả về true nếu dbColumn/friendly cho thấy field nên là integer (ví dụ SL / Qty).
        /// Dùng khi controlType = Numeric để quyết int vs string.
        /// </summary>
        private static bool RegexIsIntegerLike(string dbColumn, string friendly)
        {
            var t = (dbColumn + " " + friendly).ToLowerInvariant();
            // các keyword thường gặp
            if (t.Contains("số lượng") || t.Contains("qty") || t.Contains("sl ") || t.Contains("okqty") || t.Contains("ngqty") || t.Contains("số lượng"))
                return true;

            // nếu dbColumn chính là SLSudung/OKQty/NGQty
            if (RegexExact(dbColumn, @"^(SLSudung|OKQty|NGQty|SL|SLLoi|SLSuDung|SLL|Count|Qty)$", RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        // helper nhỏ regex exact match
        private static bool RegexExact(string input, string pattern, System.Text.RegularExpressions.RegexOptions opts = System.Text.RegularExpressions.RegexOptions.None)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input ?? string.Empty, pattern, opts);
        }

        // =================================================================
        // OPTIONAL: flush cache (nếu bạn thay đổi cấu hình form runtime)
        // =================================================================

        /// <summary>
        /// Xoá cache cho form cụ thể (vd khi admin chỉnh tblMasterForm_Controls và bạn muốn reload)
        /// </summary>
        public void InvalidateCacheFor(int idMasterForm)
        {
            lock (_cache)
            {
                _cache.Remove(idMasterForm);
            }
        }

        /// <summary>
        /// Clear toàn bộ cache
        /// </summary>
        public void ClearCache()
        {
            lock (_cache)
            {
                _cache.Clear();
            }
        }
    }
}
