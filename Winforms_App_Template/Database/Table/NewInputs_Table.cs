using Dapper;
using DevExpress.Pdf.ContentGeneration.Interop;
using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    internal class NewInputs_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public NewInputs_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<Catongtho_HeaderModel> Get_Header_catthoong(int IdCongDoan, string ItemNumber, string LotNo, int So_Me, CancellationToken ct = default)
        {
            var get_header_catthoong_query = @"
                SELECT
                    CAST(ni.idCongDoan AS nvarchar(20)) AS ID_Congdoan, 
                    cd.TenCongDoan                        AS Name_Congdoan,
                    cd.MaCongDoan                         AS Code_Congdoan,
                    ni.ItemNumber                         AS Category_Code,
                    ni.LotNo                              AS Lotno_Congdoan,
                    ni.So_Me                              AS Batch_Number,     
                    SUM(ni.OKQty)                         AS OK_Qty_Total,
                    SUM(ni.NGQty)                         AS NG_Qty_Total
                FROM 
                    tblNewInput AS ni
                LEFT JOIN 
                    tblCongDoan AS cd
                ON 
                    ni.idCongDoan = cd.idCongDoan
                WHERE
                    ni.idCongDoan = @IdCongDoan AND
                    ni.ItemNumber = @ItemNumber AND
                    ni.LotNo      = @LotNo AND
                    ni.So_Me      = @So_Me
                GROUP BY
                    ni.idCongDoan, cd.TenCongDoan, cd.MaCongDoan, ni.LotNo, ni.So_Me, ni.ItemNumber; 
                ";
            var param = new
            {
                IdCongDoan,
                ItemNumber,
                LotNo,
                So_Me
            };

            // Thực thi
            var rows = (await _db.QueryAsync<Catongtho_HeaderModel>(get_header_catthoong_query, param, ct: ct)).ToList();
            // Trả dòng đầu nếu có, hoặc null nếu không có
            return rows.FirstOrDefault();
        }
        public async Task<NewInput_Model> Get_Cat_Ong_Tho(int IdCongDoan, string ItemNumber, string LotNo, int So_Me, CancellationToken ct = default)
        {
            var cat_ong_tho_query = @"
                SELECT 
                       idInput
                      ,idCongDoan
                      ,idLydoKT
                      ,idMay_ban
                      ,Somay
                      ,ItemNumber
                      ,LotNo
                      ,SLSudung
                      ,OKQty
                      ,NGQty
                      ,So_Me
                      ,StartTime
                      ,NguoiTT
                      ,Remark
                      ,val1, val2, val3, val4, val5, val6, val7, val8, val9, val10
                      , val11, val12, val13, val14, val15
                FROM
                      tblNewInput
                WHERE 
                      idCongDoan = @idCongDoan AND
                      ItemNumber = @ItemNumber AND
                      LotNo = @LotNo AND
                      So_Me = @So_Me
                ORDER BY StartTime DESC, IdInput DESC;
            ";

            var param = new
            {
                IdCongDoan,
                ItemNumber,
                LotNo,
                So_Me
            };

            // Thực thi
            var rows = (await _db.QueryAsync<NewInput_Row>(cat_ong_tho_query, param, ct: ct)).ToList();

            return new NewInput_Model { Rows = rows };
        }
    }
}
