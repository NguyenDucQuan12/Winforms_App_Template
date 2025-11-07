using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winforms_App_Template.Database.Model
{
    public sealed class Standard_Model
    {
        public int idInput { get; init; }                          // ID Form nhập dữ liệu để lấy chi tiết lỗi
        public int idStandard { get; init; }                          // ID Form nhập dữ liệu để lấy chi tiết lỗi
        public string TenTieuChuan { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public string MaTieuChuan { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public string Loai_size { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public string Loai_kytu { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public string Loai_chieudai { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public string Loai_somay { get; init; } = "";                    // Mã lý do kiểm tra: I, II, III, IV
        public string Loai_masp { get; init; } = "";
        public string Loai_ten { get; init; } = "";
        public string TCMin { get; init; } = "";
        public string TCMax { get; init; } = "";
        public string TenNVL { get; init; } = "";
        public string MaNVL { get; init; } = "";
        public DateTime Fromdate { get; init; }
        public DateTime Todate { get; init; }
    }

    public sealed class Dieu_kien_may_Model
    {
        public int idInput { get; init; }                          // ID Form nhập dữ liệu để lấy điều kiện máy
        public string? NguoiThaotac { get; init; }                          // ID Form nhập dữ liệu để lấy chi tiết lỗi
        public DateTime Thoigian { get; init; }                 // Mã lý do kiểm tra: I, II, III, IV

        public string? Remark { get; init; }                 // Tên điều kiện máy
        public string? val1 { get; init; }                 // Tên điều kiện máy
        public string val2 { get; init; } = "";                 // Tên điều kiện máy
        public string val3 { get; init; } = "";                 // Tên điều kiện máy
        public string val4 { get; init; } = "";                 // Tên điều kiện máy
        public string val5 { get; init; } = "";                 // Tên điều kiện máy
        public string val6 { get; init; } = "";                 // Tên điều kiện máy
        public string val7 { get; init; } = "";                 // Tên điều kiện máy
        public string val8 { get; init; } = "";                 // Tên điều kiện máy
        public string val9 { get; init; } = "";                 // Tên điều kiện máy
        public string val10 { get; init; } = "";                 // Tên điều kiện máy
        public string val11 { get; init; } = "";                 // Tên điều kiện máy
        public string val12 { get; init; } = "";                 // Tên điều kiện máy
        public string val13 { get; init; } = "";                 // Tên điều kiện máy
        public string val14 { get; init; } = "";                 // Tên điều kiện máy
        public string val15 { get; init; } = "";                 // Tên điều kiện máy
        public string val16 { get; init; } = "";                 // Tên điều kiện máy
        public string val17 { get; init; } = "";                 // Tên điều kiện máy
        public string val18 { get; init; } = "";                 // Tên điều kiện máy
        public string val19 { get; init; } = "";                 // Tên điều kiện máy
        public string val20 { get; init; } = "";                 // Tên điều kiện máy
        public string val21 { get; init; } = "";                 // Tên điều kiện máy
        public string val22 { get; init; } = "";                 // Tên điều kiện máy
        public string val23 { get; init; } = "";                 // Tên điều kiện máy
        public string val24 { get; init; } = "";                 // Tên điều kiện máy
        public string val25 { get; init; } = "";                 // Tên điều kiện máy
        public string val26 { get; init; } = "";                 // Tên điều kiện máy
        public string val27 { get; init; } = "";                 // Tên điều kiện máy
        public string val28 { get; init; } = "";                 // Tên điều kiện máy
        public string val29 { get; init; } = "";                 // Tên điều kiện máy
        public string val30 { get; init; } = "";                 // Tên điều kiện máy
        public string val31 { get; init; } = "";                 // Tên điều kiện máy
        public string val32 { get; init; } = "";                 // Tên điều kiện máy
        public string val33 { get; init; } = "";                 // Tên điều kiện máy
        public string val34 { get; init; } = "";                 // Tên điều kiện máy
        public string val35 { get; init; } = "";                 // Tên điều kiện máy
        public string val36 { get; init; } = "";                 // Tên điều kiện máy
        public string val37 { get; init; } = "";                 // Tên điều kiện máy
        public string val38 { get; init; } = "";                 // Tên điều kiện máy
        public string val39 { get; init; } = "";                 // Tên điều kiện máy
        public string val40 { get; init; } = "";                 // Tên điều kiện máy
        public string val41 { get; init; } = "";                 // Tên điều kiện máy
        public string val42 { get; init; } = "";                 // Tên điều kiện máy
        public string val43 { get; init; } = "";                 // Tên điều kiện máy
        public string val44 { get; init; } = "";                 // Tên điều kiện máy
        public string val45 { get; init; } = "";                 // Tên điều kiện máy
        public string val46 { get; init; } = "";                 // Tên điều kiện máy
        public string val47 { get; init; } = "";                 // Tên điều kiện máy
        public string val48 { get; init; } = "";                 // Tên điều kiện máy
        public string val49 { get; init; } = "";                 // Tên điều kiện máy
        public string val50 { get; init; } = "";                 // Tên điều kiện máy

    }
}
