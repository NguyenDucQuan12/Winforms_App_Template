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
}
