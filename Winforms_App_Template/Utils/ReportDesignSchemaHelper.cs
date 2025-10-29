using DevExpress.DataAccess.ObjectBinding;      // ObjectDataSource
using DevExpress.XtraReports.UI;                // XtraReport, XRSubreport
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReportDesignSchemaHelper
{
    /// <summary>
    /// Gắn schema (ObjectDataSource) cho report chính và mọi subreport:
    /// - Main: typeof(Catthoong_ReportRow)  → để Field List có các cột của main
    /// - Sub : typeof(Standard_Model)       → để Field List có các cột của tiêu chuẩn
    /// Đồng thời có thể "bơm" sẵn Parameters từ model header để hiện trong Field List (tuỳ chọn).
    /// </summary>
    public static void AttachDesignSchema(XtraReport rpt, bool attachHeaderParameters = true, Type? headerModelType = null)
    {
        if (rpt == null) throw new ArgumentNullException(nameof(rpt));

        // 1) MAIN: tạo ODS theo type model của main (Catthoong_ReportRow)
        var odsMain = new ObjectDataSource
        {
            Name = "ODS_MainRows",                                         // Tên node trong Field List
            DataSource = typeof(Winforms_App_Template.Database.Model.Catthoong_ReportRow) // dùng FQN để tránh lỗi security khi load layout
        };
        if (!rpt.ComponentStorage.Contains(odsMain))
            rpt.ComponentStorage.Add(odsMain);                             // thêm vào kho component để Designer quản lý vòng đời

        // Gắn vào Report.DataSource để Designer có Field List cho main
        rpt.DataSource = odsMain;                                          // design-time only (runtime bạn sẽ bind band)
        rpt.DataMember = null;

        // 2) SUBREPORTS: duyệt tất cả XRSubreport và gắn ODS theo Standard_Model
        //foreach (var sub in EnumerateSubreports(rpt))
        //{
        //    if (sub.ReportSource is not XtraReport child) continue;

        //    var odsStd = new ObjectDataSource
        //    {
        //        Name = $"ODS_Std_{child.Name ?? child.GetType().Name}",    // khác tên nhau để tránh đụng
        //        DataSource = typeof(Winforms_App_Template.Database.Model.Standard_Model)
        //    };
        //    if (!child.ComponentStorage.Contains(odsStd))
        //        child.ComponentStorage.Add(odsStd);

        //    child.DataSource = odsStd;                                     // để Field List trong tab subreport có cột Standard_Model
        //    child.DataMember = null;
        //}

        // 3) (Tuỳ chọn) Parameters cho header: giúp người dùng kéo thả ?p_... trong Designer
        if (attachHeaderParameters)
        {
            // Nếu không truyền type, bạn có thể điền type header của bạn ở đây:
            // headerModelType = headerModelType ?? typeof(Catongtho_HeaderModel);
            if (headerModelType != null)
                EnsureParametersFromType(rpt, headerModelType, pName => "p_" + pName);
        }
    }

    /// <summary>
    /// Tạo Parameters từ type (mỗi public readable property → một parameter ?p_{PropName})
    /// </summary>
    private static void EnsureParametersFromType(XtraReport rpt, Type modelType, Func<string, string> nameSelector)
    {
        var props = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.CanRead);
        foreach (var pi in props)
        {
            var paramName = nameSelector(pi.Name);                         // ví dụ: p_Name_Congdoan
            if (rpt.Parameters[paramName] != null) continue;               // đã có thì bỏ qua

            var p = new DevExpress.XtraReports.Parameters.Parameter
            {
                Name = paramName,                                          // tên parameter (Designer sẽ thấy trong node Parameters)
                Type = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType,
                Visible = false                                            // không bật hộp thoại parameter mặc định
            };
            rpt.Parameters.Add(p);                                         // thêm vào report
        }
    }

    // Duyệt mọi XRSubreport (kể cả lồng)
    private static IEnumerable<XRSubreport> EnumerateSubreports(XtraReport rpt)
    {
        foreach (Band b in rpt.Bands)
        {
            if (b == null) continue;
            foreach (XRControl c in EnumerateControls(b.Controls))
            {
                if (c is XRSubreport s) yield return s;

                if (c is XRSubreport nest && nest.ReportSource is XtraReport child)
                    foreach (var inner in EnumerateSubreports(child))
                        yield return inner;
            }
        }
    }

    private static IEnumerable<XRControl> EnumerateControls(XRControlCollection controls)
    {
        foreach (XRControl c in controls)
        {
            yield return c;
            foreach (var inner in EnumerateControls(c.Controls))
                yield return inner;
        }
    }
}
