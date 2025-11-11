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
            .Add("Loai_chieudai", typeof(string), "Chủng loại")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");


        // Mở rộng:
        // .Add("Loai_size", typeof(string), "Loại size")


        public static readonly FieldWhitelist Kiemtrasaucattho = new FieldWhitelist()
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
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
            .Add("val9", typeof(string), "Số lượng NG 1")
            .Add("val10", typeof(string), "Số lượng NG 2")
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
            .Add("NG_xuyen_qua_1", typeof(string), "NG xuyên qua 1")
            .Add("NG_xuyen_qua_2", typeof(string), "NG xuyên qua 2");


        public static readonly FieldWhitelist Kiemtrasaucattho_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Camchot = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
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
            .Add("Thoigian", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
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
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Dap_chuoi_cat_dinh_muc = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiTT", typeof(string), "Người thao tác")
            .Add("TenMay_Ban", typeof(string), "Số máy sản xuất")
            .Add("SLSudung", typeof(int), "Số lượng sử dụng")
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
            .Add("Thoigian", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
            .Add("val1", typeof(string), "Áp lực dập")
            .Add("val2", typeof(string), "Xác nhận trạng thái chốt dập")
            .Add("val3", typeof(string), "Xác nhận cơ cấu phòng tránh dập chuôi 2 lần")
            .Add("val4", typeof(string), "Xác nhận độ chính xác của thiết bị đo độ dập sâu")
            // Ghi chú
            .Add("Remark", typeof(string), "Ghi chú");


        public static readonly FieldWhitelist Dap_chuoi_cat_dinh_muc_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Tu_dong_lap_rap_que_nong = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiTT", typeof(string), "Người thao tác")
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
            .Add("Thoigian", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
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
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");

        public static readonly FieldWhitelist Gia_cong_dau_mut_v1_5 = new FieldWhitelist()
            .Add("MaKT", typeof(string), "Lý do kiểm tra")
            .Add("StartTime", typeof(DateTime), "Thời gian thao tác")
            .Add("NguoiTT", typeof(string), "Người thao tác")
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

        public static readonly FieldWhitelist Gia_cong_dau_mut_v1_5_DKM = new FieldWhitelist()
            .Add("Thoigian", typeof(DateTime), "Thời gian bắt đầu")
            .Add("NguoiThaotac", typeof(string), "Người thao tác")
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


        public static readonly FieldWhitelist Gia_cong_dau_mut_v1_5_Standard = new FieldWhitelist()
            .Add("TenTieuChuan", typeof(string), "Hạng mục kiểm tra")
            .Add("MaTieuChuan", typeof(string), "Mã tiêu chuẩn")
            .Add("TCMin", typeof(string), "TC Min")
            .Add("TCMax", typeof(string), "TC Max");
    }
}
