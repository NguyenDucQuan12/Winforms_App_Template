using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winforms_App_Template.Database.Model;

namespace Winforms_App_Template.Database.Table
{
    
    internal class MasterFormControl_Table
    {
        private readonly DbExecutor _db; // Hạ tầng thực thi Dapper + Polly
        public MasterFormControl_Table(DbExecutor db) => _db = db; // Contructor lấy db

        public async Task<List<MasterForm_Control_Model>?> Get_Detail_Control(int idMasterForm, CancellationToken ct = default)
        {
            var get_detail_control_query = @"
                SELECT
                    id,
                    ControlCode,
                    idMasterForm,
                    DBColumn,
                    FriendlyName
                FROM 
                    tblMasterForm_Controls
                
                WHERE
                    idMasterForm = @idMasterForm;
                ";
            var param = new
            {
                idMasterForm
            };

            // Thực thi
            var rows = (await _db.QueryAsync<MasterForm_Control_Model>(get_detail_control_query, param, ct: ct));
            // Trả dòng đầu nếu có, hoặc null nếu không có
            return rows.ToList();
        }
    }
}
