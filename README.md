Chương trình windows bằng C#  

Cấu trúc dự án như sau:  

```
Winform_Template/      # Thư mục chứa toàn bộ dự án
│
├── My_App/        # Thư mục chứa code chính của dự án
├── .vscode/            # Cấu hình debug trong visual studio code
├── src/
│   ├── main.py           # Tệp chính chạy chương trình
│   ├── api/              # Các router (endpoint)
│   │   ├── __init__.py
│   │   └── health_check.py
│   ├── auth/              # Xác thực người dùng
│   │   ├── __init__.py
│   │   └── authentication.py
│   ├── controller/              # Xử lý các api
│   │   ├── __init__.py
│   │   └── user_controller.py
│   ├── db/               # Kết nối database
│   |   ├── __init__.py
│   |   └── database.py
│   ├── log/               # Cấu hình ghi log
│   |   ├── __init__.py
│   |   └── api_log.py
│   ├── middleware/              # middleware
│   │   ├── __init__.py
│   │   └── logger.py
│   ├── schemas/                # Các schema để validation dữ liệu
│   │   ├── __init__.py      # Schema cho User
│   │   └── user.py   
│   ├── services/         # Các dịch vụ khác
│   │   ├── __init__.py
|   |   └── mail_service.py
│   ├── test/               # Các tệp test api
│   │   ├── __init__.py
|   |   └── file_api_test.py
|   └──utils/
|       ├── Contants.cs         # Tệp chứa các hằng số của dự án
|       ├── LogEx.cs         # Log có chứa thêm tham số xảy ra log ở tệp nào, dòng nào
|       ├── Logging.cs         # Cấu hình chính cho logger
|       ├── Contants.cs         # Tệp chứa các hằng số của dự án
|       └── Shell.py
│
├── My_App.sln            # Tệp solution để dự án tìm được đến với nhau
├── .dockerignore            # Cấu hình bỏ qua các thư mục, tệp trong docker
├── .env            # Tệp tin chứa cấu hình các thông số
├── .gitignore            # Cấu hình bỏ qua các thư mục, tệp tin trong git
├── Dockerfile            # Xây dựng các image cho docker
├── docker-compose.yml    # Cấu hình các thông số khi chạy trên docker
└── README.md
```

Cách đóng gói tệp tin:  

> Đóng gói tất cả thành 1 tệp exe duy nhất  
> Thay đổi tham số `--self-contained` thành `--no-self-contained` để đóng gói nhỏ nhẹ, nhưng thiết bị chạy cần có dotnet  
> Khi đó đầu ra sẽ nằm ở: `bin\Release\net{version}-windows\win-x64\publish\`  

```C#
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Hoặc có thể copy cả thư mục `Debug` tại đường dẫn: `bin/Debug/phiên bản dotnet/`. Thư mục này chứa toàn bộ tệp exe và các thư viện đi kèm được copy vào đây khi chạy ở chế độ `Debug`.  


# Các compoment

## 1. Label

Dùng để hiển thị văn bản tĩnh, mô tả cho input.  

![image](Image/Github/get_label_from_toolbox.png)  

Các thuộc tính chính trong `label`.  

- `Anchor`: Cố định vị trí của nhãn tại 1-2 điểm  

![image](Image/Github/anchor_properties.png)  

Sẽ có 4 vị trí để cố định như sau:  
`right`, `left`, `top`, `bottom` là các vị trí cố định nhãn theo phía được chỉ định, khi kéo dãn cửa sổ màn hình thì nó luôn nằm 1 khoảng cách bằng với ban đầu so với phía đã đặt  
Ta có thể đặt nhãn cố định theo 2 phía cùng lúc, 3, 4 phía sẽ không được hỗ trợ. Còn nếu ko chọn phía nào (`None`) thì nó tự động co dãn theo vị trí ban đầu  

- `AutoSize`: Khi nhận giá trị `False` thì cho phép người dùng thay đổi kích thước của nhãn, hoặc khi bật `True` thì nó sẽ tự động co dãn kích thước theo nội dung văn bản trong nó  
- `Text`: Thay đổi nội dung trong label  
- `TextAlign`: Căn chỉnh nội dung văn bản là giữa, trên, dưới, ...  
- `Modifier`: Thuộc tính của nhãn cho phép nó là `Public`, `Private`, `Internal`, ...

![image](Image/Github/modifiers_properties.png)  

Mặc định sẽ là `private`, khi chuyển sang `public`, `Internal` thì cho phép chỉnh sửa các thuộc tính của nhãn từ `form` khác.  

## 2. Button

# Quản lý vòng đời  

Trong .NET, một số đối tượng giữ tài nguyên bên ngoài GC (ngoài bộ nhớ managed), ví dụ: `handle file`, `socket`, `GDI+ (ảnh, icon, bút vẽ)`, `kết nối DB` …. Những đối tượng này thường triển khai giao diện `IDisposable` và có hàm `Dispose()` để giải phóng tài nguyên sớm, chủ động (deterministic).  

Nếu ta không gọi `Dispose()`, tài nguyên có thể chỉ được thu hồi khi `GC (Garbage collector) + finalizer` chạy, trễ và có thể gây rò rỉ (memory leak / handle leak / file bị khóa).  

Tất cả các component(non-visual) nào được kéo ra từ `Toolbox` vào form ví dụ như `NotifyIcon`, `Timer`, `ImageList`, ... dều sẽ được đưa vào `components` và sẽ được `Dispose` tự động khi form `Dispose`. Và với các đối tượng thuần `.NET` như string, list, ... cũng được GC dọn tự động khi không còn tham chiếu nữa.  

Còn các tài nguyên mà ta tự tạo như ảnh clone, icon clone, stream, socket ... không nằm trong components nên ta phải tự `Dispose` sau khi sử dụng xong hoặc khi ta thay thế ảnh mới thì cần `Dispose` ảnh cũ đi.  

> Bất cứ thứ gì là `IDisposable` thì đều phải chủ động giải phóng, trừ khi vòng đời của nó gắn với components của form và được form dispose hộ  

Các dấu hiệu cho các đối tượng là `IDisposable`:  
- Khi khai báo kiểu đối tượng có ghi `: IDisposable`, hoặc mở lớp định nghĩ `(F12)` hoặc `tooltip intelliSense` có `class ... : IDisposable ...`  
- Rê chuột lên đầu đối tượng và vào mục tên kiểu sẽ hiện `Implement: system.IDisposable`  
- Tài liệu chính thức (trang Docs hoặc Object Browser trong VS) ghi rõ `Implements IDisposable`  

Các nhóm thường là `IDisposable` trong winform như sau:  
- GDI+ / vẽ: `Image`, `Bitmap`, `Icon`, `Graphics`, `Pen`, `Brush`, `Font`, `Region` …  
- I/O: `Stream (FileStream/MemoryStream)`, `StreamReader/Writer`, `ZipArchive` …  
- DB: `SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlTransaction` …  
- Crypto/Compression: `SHA256`, `Aes`, `GZipStream` …  
- Component WinForms (non-visual): `NotifyIcon`, `Timer (một số loại)`, `ImageList`, `BindingSource`, `FileSystemWatcher`, `BackgroundWorker` …  
- Network: `HttpClient` (khuyến nghị dùng singleton lâu dài, hiếm khi dispose ngay), `TcpClient`, `Socket` …  

## 1. Sử dụng using (cách được khuyến nghị)

`using` là cú pháp gọi `Dispose()` tự động và đúng thời điểm, để trả tài nguyên ngay khi ra khỏi `scope`, kể cả khi có `exception`.  

Dạng `using` phổ biến nhất như sau:  
```C#
using (var fs = File.OpenRead(path))
using (var img = Image.FromStream(fs))
{
    // Dùng img tạm thời
    pictureBox1.Image = (Image)img.Clone(); // clone để sở hữu bản riêng
} // <- fs & img tự Dispose ngay tại đây

```
Cả hai đối tượng `img` và `f`s sẽ được `Dispose` tự động khi ra khỏi khối { ... }. Thứ tự `Dispose`: đối tượng khai báo sau được `Dispose` trước. Tức là: `img.Dispose()` chạy trước, rồi mới đến `fs.Dispose()`. Đây là thứ tự an toàn vì `img` phụ thuộc vào `fs`.  
Dạng này có phạm vi sử dụng nhỏ, gọn và nhanh chóng.  

Dạng `using` thứ hai là:  

```C#
using var fs = File.OpenRead(path);
using var img = Image.FromStream(fs);
// scope là cả block hiện tại; ra khỏi block -> Dispose tự động
```
Dạng này sử dụng khi có phạm vi sử dụng là cả 1 phương thức/block  

Đối với các tệp tin được tải từ hệ thống bằng `Image.FromFile(path)` sẽ khóa file đến khi `Dispose` → hay gây lỗi `“file đang bị sử dụng”`. Cách chuẩn đó sẽ là: `mở stream + FromStream + Clone` để không khóa file.  

Ta có thể mở một tệp như sau:  
```C#
using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
using (var img = Image.FromStream(fs, useEmbeddedColorManagement: false, validateImageData: false))
{
    var copy = (Image)img.Clone(); // tách khỏi stream & file
    pictureBox1.Image = copy;      // bạn sở hữu 'copy' -> dispose sau
}
```
Hoặc theo cú pháp ngắn gọn hơn:  
```C#
using var img = Image.FromFile(path);
pictureBox1.Image = (Image)img.Clone(); // Clone để không giữ lock tệp
```

Tương tự với việc tải từ `byte[]/thiết bị (camera, api)` ta sử dụng `using`:  

```C#
using var ms  = new MemoryStream(bytes);
using var img = Image.FromStream(ms);
var copy = (Image)img.Clone();     // tách khỏi stream
pictureBox1.Image = copy;          // dispose copy khi xong
```

## 2. Tự gọi Dispose

```C#
MemoryStream? ms = null;
try {
    ms = new MemoryStream(bytes);
    using var icon = new Icon(ms);
    this.Icon = (Icon)icon.Clone();
}
finally {
    ms?.Dispose();
}
```
Tương tự với `using` (tự gọi try-catch), thì ta chủ động gọi try-finally để `Dispose` các đối tượng sau khi không còn sử dụng nó nữa.  

## 3. Kèm theo vào mẫu Dispose của form/control 
Với một số trường hợp ta ko thể `Dispose` nó liền ngay lập tức mà ta cần sử dụng nó trong xuyên suốt form thì ta sử dũng mẫu `Dispose` của form.  

```C#
// Ảnh cố định dùng chung (ít thay đổi)
var ok = (Image)Properties.Resources.Ok16.Clone();
var warn = (Image)Properties.Resources.Warn16.Clone();
// … dùng xuyên suốt form, và Dispose chúng trong Dispose(bool)

// Ảnh mỗi dòng thay đổi liên tục:
var img = LoadAndClonePerRow(...); // clone mỗi lần
row.Cells[col].Value = img;
// Khi xoá dòng hoặc refresh -> Dispose ảnh cũ nếu bạn tạo nó
```
Với các đối tượng được `Dispose` trong `Dispose(bool)` thì ta thực hiện như sau:  

```C#
private Icon? _ownedIcon;

public MainForm()
{
    InitializeComponent();
    var tmp = (Icon)Properties.Resources.AppIcon.Clone(); // clone để sở hữu
    _ownedIcon = tmp;
    this.Icon = _ownedIcon;
}

protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        _ownedIcon?.Dispose(); // trả tài nguyên icon
    }
    base.Dispose(disposing);
}

```
## 4. Mô hình ownership

> Tư duy `Ownership` (quyền sở hữu): Ai “sở hữu” đối tượng → người đó chịu trách nhiệm `Dispose()`.  

Với các tài nguyên trong `Properties.Resource.*` thì chúng ta không trực tiếp `Dispose` vì nó là bản cache dùng chung, khi ta `Dispose` thì lần tiếp theo ta truy vấn lại đối tượng sẽ gặp lỗi.  
Khi gán đối tượng vào `Form/Control` và ta muốn chủ động quản lý vòng đời dối tượng đó thì ta sẽ `clone` đối tượng đó trước rồi sử dụng đối tượng `clone`, không sử dụng bản gốc để tránh `Dispose` nhầm.  

Đây là cách thức đúng:  
```C#
// ĐÚNG (an toàn):
var iconOwned = (Icon)Properties.Resources.AppIcon.Clone();
this.Icon = iconOwned;            // bạn sở hữu -> bạn Dispose sau

// SAI (rủi ro): 
this.Icon = Properties.Resources.AppIcon;
// Nếu sau này ai đó Dispose nhầm, resource cache hỏng.
```

> Nếu bạn chấp nhận để control dùng bản cache (và cam kết không ai Dispose nhầm), bạn có thể gán trực tiếp. Thực tế, để an toàn và dễ bảo trì, khuyến nghị luôn clone khi gán vào UI và bạn có ý định thay/giải phóng sau này.  

Ví dụ mẫu code thay ảnh cho một `pictureBox`:  
```C#
void SetImageSafe(PictureBox pb, Image newImgOwned)
{
    var old = pb.Image as IDisposable;
    pb.Image = newImgOwned;  // gán
    old?.Dispose();          // trả tài nguyên ảnh cũ (nếu do mình sở hữu)
}
```
## 5. Những điểm rò rỉ bộ nhớ phổ biến

### 5.1 Ảnh/biểu tượng (GDI+)

`Image.FromFile(path)` giữ khóa file đến khi `Dispose` → nếu muốn không khóa file, dùng:  

- Cách A: `using var img = Image.FromFile(path); var copy = (Image)img.Clone();` rồi `Dispose img`; dùng phiên bản img copy.  

- Cách B: `using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); using var img = Image.FromStream(fs); var copy = (Image)img.Clone();`

Tuyệt đối không `Dispose` trực tiếp `Properties.Resources.Logo` → `Clone` trước khi gán, `Dispose clone` về sau.  
Mọi thứ tự `new` như `new Icon(...)`, `new Bitmap(...)`, `new Pen(...)`, `new Brush(...)` phải `Dispose` (trừ `Pens.Red/Brushes.Black` là shared do `.NET` quản lý — `không được Dispose`).  

### 5.2 Graphics & OnPaint

Trong `OnPaint`, không tạo đối tượng nặng mỗi frame. Nếu bắt buộc tạo, dùng `using`.  
Không `cache Graphics` lâu dài; chỉ dùng trong scope ngắn (inside `OnPaint` hoặc trong `using var g = control.CreateGraphics();` và `Dispose` ngay).  

### 5.3 Timers

`System.Windows.Forms.Timer`: chạy trên `UI thread`, thuộc components nếu kéo thả → `form Dispose` sẽ dọn. Nếu tạo tay thì cần thao tác dừng và `Dipose` nó: `timer.Stop(); timer.Dispose();`  

`System.Timers.Timer/System.Threading.Timer`: không nằm trong `components` → phải `Dispose()` thủ công, đặc biệt `trước khi form đóng`, kẻo callback bắn vào form đã Dispose.  

Khi dùng `timer/Task` cập nhật UI, nhớ check `IsDisposed`+ dùng `BeginInvoke/Invoke` hợp lệ.  

### 5.4 Sự kiện (event) – rò rỉ do đăng ký mà không bỏ đăng ký

Nếu form đăng ký vào event của đối tượng sống lâu (`singleton`, `static`, `service` nền), phải `-=` trước khi form `Dispose`; nếu không, `publisher` giữ tham chiếu đến form, `GC không thu → rò rỉ`.  

```C#
protected override void OnHandleDestroyed(EventArgs e)
{
    longLivingPublisher.SomeEvent -= FormHandler; // hủy đăng ký
    base.OnHandleDestroyed(e);
}
```

Tránh `static list` giữ tham chiếu `control/form`.  

### 5.5 NotifyIcon

Là `non-visual component` (nằm ở `component tray`). Phải:  

- Set Icon hợp lệ `(.ico)`, `Visible = true` để hiện ở system tray.  

- Trước khi thoát: `notifyIcon.Visible = false`; rồi để `form Dispose` dọn (nếu trong components). Tạo tay? → `notifyIcon.Dispose()`.  

### 5.6 Data binding & DataTable

- `BindingSource`, `DataTable`, `SqlConnection/Command/Reader` đều `IDisposable` → `Dispose` khi xong.  
- Khi thay `DataSource`, cân nhắc `Dispose` dữ liệu cũ nếu bạn sở hữu.  

### 5.7 Task/đa luồng

Tránh `“fire-and-forget”` không quản lý. Luôn có `CancellationTokenSource`:  

```C#
private CancellationTokenSource? _cts;

protected override void OnLoad(EventArgs e)
{
    _cts = new CancellationTokenSource();
    _ = RunWorkerAsync(_cts.Token);
    base.OnLoad(e);
}

protected override void OnFormClosing(FormClosingEventArgs e)
{
    _cts?.Cancel();      // yêu cầu dừng
    base.OnFormClosing(e);
}

private async Task RunWorkerAsync(CancellationToken ct)
{
    try {
        while (!ct.IsCancellationRequested) {
            // ...
            await Task.Delay(500, ct);
        }
    } catch (OperationCanceledException) { /* bình thường */ }
}
```

Khi cập nhật UI từ thread khác: `if (!IsDisposed) BeginInvoke((Action)(() => ...));` Tránh gọi vào control đã `Dispose`.  

### 5.8 Form con/đối thoại

- Tạo modal: `using (var dlg = new SettingsForm()) dlg.ShowDialog(this)`; → `Dispose` tự động.  

- Tạo non-modal: nhớ `Dispose` khi đóng:  

```C#
var f = new ChildForm();
f.FormClosed += (_, __) => f.Dispose();
f.Show(this);
```

## 6. Kiểm tra & chẩn đoán rò rỉ

- `Visual Studio Diagnostic Tools`: `Debug → Debug > Windows > Show Diagnostic Tools → Memory Usage`. Chụp snapshot trước/sau một thao tác → so sánh object còn treo.  
- Công cụ ngoài: `dotMemory`, `ANTS Memory Profiler`, `PerfView`.  

Dấu hiệu: `handle GDI` tăng đều, `file bị khóa`, bộ nhớ không giảm sau khi đóng form nhiều lần.  

Có thể bật `Roslyn Analyzer CA200` để tự động cảnh báo khi khởi tạo 1 đối tượng `IDisposable`.  Ta thêm đoạn mã sau vào tệp `Directory.Build.props` đặt ở gốc solution.  
```xml
<Project>
  <PropertyGroup>
    <!-- Bật .NET Analyzers tích hợp cho mọi project -->
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisMode>AllEnabledByDefault</AnalysisMode>

    <!-- Tuỳ chọn: biến cảnh báo thành lỗi toàn cục -->
    <!-- <TreatWarningsAsErrors>true</TreatWarningsAsErrors> -->
  </PropertyGroup>
</Project>
```
Và đặt đoạn mã sau vào tệp `.editorconfig` cũng nằm ở gốc solution:  
```ini
# Apply to all C# files
[*.cs]

# CA2000: Dispose objects before losing scope
dotnet_diagnostic.CA2000.severity = error

# (Khuyến nghị) Bật thêm vài rule liên quan IDisposable
dotnet_diagnostic.CA2213.severity = warning   # Disposable fields should be disposed
dotnet_diagnostic.CA1816.severity = warning   # Dispose methods should call SuppressFinalize
```
Nếu thấy nhiều lỗi `CA2000`, đó là tốt: nó giúp bạn rà soát mọi nơi tạo `IDisposable` mà chưa `using/Dispose`.  
Sửa bằng cách: 
- Ưu tiên `using (statement hoặc declaration)`  
- `Dispose()` trong `finally`  
- Truyền `ownership` rõ ràng (ví dụ param `leaveStreamOpen như trong ImageLoader`).  

## 7. Tạo có hleper

### 7.1 Helper tải ảnh /icon
Ta tạo 1 helper giúp tải ảnh, icon an toàn (không khóa file), luôn trả về bản clone.  

```C#
// =========================
// Utils/ImageLoader.cs
// =========================
using System;
using System.Drawing;            // Image, Icon, Bitmap
using System.IO;                 // Stream, FileStream

namespace Test.Utils
{
    /// <summary>
    /// Tiện ích tải Image/Icon từ file, stream, byte[] theo cách "KHÔNG khóa file"
    /// và trả về BẢN CLONE mà bạn SỞ HỮU (=> bạn có trách nhiệm Dispose khi xong).
    /// </summary>
    public static class ImageLoader
    {
        // ---------- IMAGE ----------

        /// <summary>
        /// Mở ảnh từ file theo cách không khóa file gốc: đọc qua Stream -> FromStream -> Clone -> trả bản clone.
        /// </summary>
        public static Image LoadImageNoLock(string path)
        {
            // Mở file cho phép process khác vẫn đọc/ghi (ReadWrite) để giảm nguy cơ lock
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // Tạo ảnh từ luồng: đối tượng này phụ thuộc stream => phải clone để tách
            using var src = Image.FromStream(fs);
            // Trả bản clone độc lập: bạn sở hữu & phải Dispose khi không dùng nữa
            return (Image)src.Clone();
        }

        /// <summary>
        /// Tạo ảnh từ byte[], không phụ thuộc byte[] sau khi Clone.
        /// </summary>
        public static Image LoadImageFromBytes(byte[] data)
        {
            using var ms  = new MemoryStream(data);
            using var src = Image.FromStream(ms);
            return (Image)src.Clone();
        }

        /// <summary>
        /// Tạo ảnh từ stream bên ngoài. Nếu leaveStreamOpen=false, hàm sẽ Dispose stream giúp bạn.
        /// </summary>
        public static Image LoadImageFromStream(Stream stream, bool leaveStreamOpen = true)
        {
            Image? src = null;
            try
            {
                src = Image.FromStream(stream);
                return (Image)src.Clone();
            }
            finally
            {
                src?.Dispose();                 // luôn Dispose ảnh nguồn tạm
                if (!leaveStreamOpen) stream.Dispose(); // chỉ Dispose stream khi bạn giao quyền sở hữu cho hàm
            }
        }

        // ---------- ICON ----------

        /// <summary>
        /// Mở icon (.ico) từ file theo cách an toàn: đọc qua stream -> new Icon(stream) -> Clone -> trả bản clone.
        /// </summary>
        public static Icon LoadIconFromFile(string path)
        {
            using var fs  = File.OpenRead(path);
            using var src = new Icon(fs);
            return (Icon)src.Clone();           // Clone để icon không phụ thuộc stream
        }

        /// <summary>
        /// Tạo icon từ byte[] theo cách an toàn.
        /// </summary>
        public static Icon LoadIconFromBytes(byte[] data)
        {
            using var ms  = new MemoryStream(data);
            using var src = new Icon(ms);
            return (Icon)src.Clone();
        }

        /// <summary>
        /// Tạo icon từ stream bên ngoài. Nếu leaveStreamOpen=false, hàm sẽ Dispose stream giúp bạn.
        /// </summary>
        public static Icon LoadIconFromStream(Stream stream, bool leaveStreamOpen = true)
        {
            Icon? src = null;
            try
            {
                src = new Icon(stream);
                return (Icon)src.Clone();
            }
            finally
            {
                src?.Dispose();
                if (!leaveStreamOpen) stream.Dispose();
            }
        }
    }
}
```
`Image.FromStream/new Icon(stream)` thường giữ tham chiếu tới `stream`; nếu bạn `Dispose stream` sớm có thể gây lỗi về sau. `Clone()` tạo bản độc lập, giúp:  
- Không khóa file  
- Vòng đời rõ ràng: bạn sở hữu ⇒ bạn `Dispose`  

### 7.2 Helper clone từ Resource 

```C#
// =========================
// Utils/ResourceWrapper.cs
// =========================
using System;
using System.Drawing;
using System.IO;
using System.Resources;     // ResourceManager

namespace Test.Utils
{
    /// <summary>
    /// Tiện ích để lấy và CLONE Image/Icon từ Resources.
    /// Tránh Dispose trực tiếp instance cache của Resources.
    /// </summary>
    public static class ResourceWrapper
    {
        // --- CÁCH 1: Nhận trực tiếp object từ Properties.Resources (strongly-typed) ---

        /// <summary>
        /// Clone 1 Image từ resource object kiểu Image.
        /// </summary>
        public static Image CloneImage(Image resourceImage)
            => (Image)resourceImage.Clone();

        /// <summary>
        /// Clone 1 Icon từ resource object kiểu Icon.
        /// </summary>
        public static Icon CloneIcon(Icon resourceIcon)
            => (Icon)resourceIcon.Clone();

        // --- CÁCH 2: Tìm theo tên resource qua ResourceManager (linh hoạt theo tên runtime) ---

        /// <summary>
        /// Lấy & clone Image theo tên resource (hỗ trợ Bitmap/byte[] fallback).
        /// </summary>
        public static Image? CloneImage(ResourceManager rm, string name)
        {
            var obj = rm.GetObject(name);
            switch (obj)
            {
                case Image img:
                    return (Image)img.Clone();
                case byte[] bytes:
                    using (var ms = new MemoryStream(bytes))
                    using (var src = Image.FromStream(ms))
                        return (Image)src.Clone();
                default:
                    return null; // không phải image
            }
        }

        /// <summary>
        /// Lấy & clone Icon theo tên resource (hỗ trợ Icon/byte[]).
        /// </summary>
        public static Icon? CloneIcon(ResourceManager rm, string name)
        {
            var obj = rm.GetObject(name);
            switch (obj)
            {
                case Icon ic:
                    return (Icon)ic.Clone();
                case byte[] bytes:
                    using (var ms = new MemoryStream(bytes))
                    using (var src = new Icon(ms))
                        return (Icon)src.Clone();
                default:
                    return null; // không phải icon
            }
        }
    }
}
```
Cách sử dụng nhanh trong các tệp:  
```C#
// Strongly-typed
var myIcon = ResourceWrapper.CloneIcon(Properties.Resources.AppIcon);
var myImg  = ResourceWrapper.CloneImage(Properties.Resources.Logo);

// Theo tên (nếu bạn biết "key" tại runtime)
var icon2 = ResourceWrapper.CloneIcon(Properties.Resources.ResourceManager, "AppIcon");
var img2  = ResourceWrapper.CloneImage(Properties.Resources.ResourceManager, "Logo");
```

### 7.3 Set image/icon 
Ta tạo helper thiết lập icon/image mới và tự động dọn các đối tượng cũ an toàn

```C#
// =========================
/* Utils/UiDisposalGuard.cs */
// =========================
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Test.Utils
{
    /// <summary>
    /// Tiện ích "giao nhận" tài nguyên UI:
    /// - Khi gán Image/Icon mới, tự Dispose cái cũ (nếu bạn là owner).
    /// - Giảm leak khi thay ảnh/biểu tượng nhiều lần.
    /// LƯU Ý: Chỉ Dispose cái CŨ nếu đó là "bản clone bạn sở hữu".
    /// </summary>
    public static class UiDisposalGuard
    {
        /// <summary>
        /// Gán ảnh mới cho PictureBox và Dispose ảnh cũ (nếu có, và khác reference).
        /// </summary>
        public static void SetImageSafe(PictureBox pb, Image newOwned)
        {
            // Giữ tham chiếu ảnh cũ để dispose về sau
            var old = pb.Image;

            // Gán ảnh mới
            pb.Image = newOwned;

            // Dispose ảnh cũ nếu:
            // - khác với ảnh mới
            // - là IDisposable (Image là IDisposable)
            // - VÀ do bạn sở hữu (ý thức sử dụng)
            if (!ReferenceEquals(old, newOwned))
                (old as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Xoá ảnh hiện tại (nếu có) và đặt null.
        /// </summary>
        public static void ClearImageSafe(PictureBox pb)
        {
            var old = pb.Image;
            pb.Image = null;
            (old as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Gán icon mới cho Form và Dispose icon cũ nếu khác.
        /// </summary>
        public static void SetIconSafe(Form form, Icon newOwned)
        {
            var old = form.Icon;
            form.Icon = newOwned;
            if (!ReferenceEquals(old, newOwned))
                (old as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Gán icon mới cho NotifyIcon và Dispose icon cũ nếu khác.
        /// </summary>
        public static void SetIconSafe(NotifyIcon ni, Icon newOwned)
        {
            var old = ni.Icon;
            ni.Icon = newOwned;
            if (!ReferenceEquals(old, newOwned))
                (old as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Gán ảnh mới cho ToolStripItem (Button/Menu) và Dispose ảnh cũ nếu khác.
        /// </summary>
        public static void SetImageSafe(ToolStripItem item, Image newOwned)
        {
            var old = item.Image;
            item.Image = newOwned;
            if (!ReferenceEquals(old, newOwned))
                (old as IDisposable)?.Dispose();
        }
    }
}
```

Cách sử dụng trong tệp:  
```C#
// Từ file:
var img = ImageLoader.LoadImageNoLock(@"C:\logo.png"); // bạn sở hữu 'img'
UiDisposalGuard.SetImageSafe(pictureBox1, img);        // set mới, dispose ảnh cũ (nếu có)

// Từ resource:
var icon = ResourceWrapper.CloneIcon(Properties.Resources.AppIcon);
UiDisposalGuard.SetIconSafe(this, icon);

// Khi đóng form:
UiDisposalGuard.ClearImageSafe(pictureBox1);           // trả ảnh về null & dispose
```

> chỉ Dispose “cái cũ” nếu đó là bản bạn sở hữu (clone/tạo ra). Tránh dispose nhầm các instance framework giữ hộ (ví dụ một số Font mặc định).  


# Kết nối tới CSDL

- Sử dụng `Microsoft.Data.SqlClient`  
- `Open late, close early`: mở connection `ngay trước` khi query, đóng `ngay sau` kết thúc query (pooling sẽ tái dùng)  
- `Async/Await`: dùng các `API OpenAsync`, `ExecuteReaderAsync`, `ExecuteAsync (Dapper)` + `CancellationToken` → không block UI.  
- `Retry` có kiểm soát (polly): `retry` chỉ lỗi `transient` (đứt mạng, deadlock, server bận) với `exponential backoff + jitter + circuit breaker`.  
- `Timeouts`: đặt `Connect Timeout (connection string)` + `CommandTimeout (per command)` để không treo vô hạn.  
- Tham số hoá: luôn dùng `parameter (hoặc Dapper)` để tránh `SQL injection` & lỗi `encode`.  
- `Transaction`: gói các lệnh phụ thuộc trong `SqlTransaction` (hoặc `TransactionScope` nếu cần) + `retry` toàn khối khi lỗi `transient`.  

## 1. Cài đặt các gói phụ thuộc
Các gói phụ được đặt thêm vào tệp `Directory.Packages.props`  
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <ItemGroup>
    <PackageVersion Include="Microsoft.Data.SqlClient" Version="5.2.2" />
    <PackageVersion Include="Dapper" Version="2.1.35" />
    <PackageVersion Include="Polly" Version="8.4.2" />
  </ItemGroup>
</Project>
```

Ví dụ 1 tệp hoàn chỉnh:  
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
	<!-- 
    File này khai báo VERSION cho các PackageReference trong toàn solution.
    Ở từng .csproj, bạn chỉ cần liệt kê tên package (KHÔNG kèm Version).
  -->
	<ItemGroup>
		<!-- Serilog core: 4.0.0 (phù hợp với File sink 6.x) -->
		<PackageVersion Include="Serilog" Version="4.0.0" />
		<!-- File sink ghi log ra file -->
		<PackageVersion Include="Serilog.Sinks.File" Version="6.0.0" />
		<!-- Async wrapper: tránh block UI -->
		<PackageVersion Include="Serilog.Sinks.Async" Version="1.5.0" />
		<!-- Enricher ThreadId -->
		<PackageVersion Include="Serilog.Enrichers.Thread" Version="3.1.0" />
		<!-- Exception enricher -->
		<PackageVersion Include="Serilog.Exceptions" Version="8.4.0" />
		<!-- Microsoft.Data.SqlClient: 5.2.2 (phiên bản mới nhất tính đến 06/2024) -->
		<PackageVersion Include="Microsoft.Data.SqlClient" Version="5.2.2" />
		<PackageVersion Include="Dapper" Version="2.1.35" />
		<PackageVersion Include="Polly" Version="8.4.2" />
		<!-- Tương tự thêm các gói mới vào dưới đây  -->
	</ItemGroup>
</Project>
```

Trong tệp chính có đuôi `.csproj` ta điền thêm:  
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Data.SqlClient" />
  <PackageReference Include="Dapper" />
  <PackageReference Include="Polly" />
</ItemGroup>
```
Tệp hoàn chỉnh như sau:  
```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net9.0-windows</TargetFramework>
		<UseWindowsForms>true</UseWindowsForms>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>
		<OutputType>WinExe</OutputType>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
		<ApplicationIcon>Resources\Quan_ico.ico</ApplicationIcon>
	</PropertyGroup>
	<ItemGroup>
	  <Content Include="Resources\Quan_ico.ico" />
	</ItemGroup>
	<ItemGroup>
		<!-- Version được lấy từ Directory.Packages.props -->
		<PackageReference Include="Serilog" />
		<PackageReference Include="Serilog.Sinks.File" />
		<PackageReference Include="Serilog.Sinks.Async" />
		<PackageReference Include="Serilog.Enrichers.Thread" />
		<PackageReference Include="Serilog.Exceptions" />
		<PackageReference Include="Microsoft.Data.SqlClient" />
		<PackageReference Include="Dapper" />
		<PackageReference Include="Polly" />

		<!--Thêm 1 package vào đây, chỉ cần tên packages, version đã có ở directory.Packages.props-->
	</ItemGroup>
	<ItemGroup>
	  <Compile Update="Properties\Resources.Designer.cs">
	    <DesignTime>True</DesignTime>
	    <AutoGen>True</AutoGen>
	    <DependentUpon>Resources.resx</DependentUpon>
	  </Compile>
	</ItemGroup>
	<ItemGroup>
	  <EmbeddedResource Update="Properties\Resources.resx">
	    <Generator>ResXFileCodeGenerator</Generator>
	    <LastGenOutput>Resources.Designer.cs</LastGenOutput>
	  </EmbeddedResource>
	</ItemGroup>
</Project>
```
## 2. Tạo cấu trúc dự án

Ta tạo cấu trúc dự án với Databse như sau:  

```
Database/
 ├─ Model                       // Chứa các schema dữ liệu trả về
 │  ├── Wallet_Model.cs         // Schema của bảng dữ liệu Wallet trong SQL Server
 │  ...                         // Các Schema của các bảng khác được viết ở đây
 │  └── Users_Login_Model.cs    // Schema của bảng dữ liệu Wallet trong SQL Server  
 │
 ├─ Table                       // Chứa các lệnh truy vấn tương ứng với từng bảng trong SQL Server 
 │  ├── Users_Login_Table.cs    // Các lệnh truy vấn của bảng Users_Login được viết ở đây
 │  ...                         // Các lệnh truy vấn của các bảng còn lại được viết ở đây
 │  └── Wallet_Table.cs         // Các lệnh truy vấn của bảng Wallet_Table được viết ở đây
 │
 ├─ DbConfig.cs                 // Lấy connection string (ENV trước, fallback config)
 ├─ SqlPolicies.cs              // Chính sách Polly: retry + circuit breaker
 ├─ SqlConnectionFactory.cs     // Tạo SqlConnection mở sẵn (async)
 ├─ DbExecutor.cs               // Hàm thực thi (Dapper) có retry, timeout, cancel
 └─ TransientErrorDetector.cs   // Cấu hình các lỗi cho phép retry hoặc mở break
```

### 2.1 Chuỗi kết nối

Ta tạo hàm lấy chuỗi kết nối từ biến môi trường (`.env`) nếu tồn tại các giá trị, còn nếu không thì ta lấy mặc định chuỗi được cung cấp. Câu lệnh được viết tại tệp `DbConfig.cs`  

```C#
using System;

namespace Test.Database
{
    /// <summary>
    /// Lấy chuỗi kết nối từ ENV trước, sau đó mới đến fallback truyền vào.
    /// GIỮ: Encrypt=True; Application Name=YourApp; Connect Timeout=15 là tối thiểu.
    /// </summary>
    public static class DbConfig
    {
        /// <summary>
        /// Lấy chuỗi kêt nối SQL Server.
        /// </summary>
        /// <param name="fallback"></param>
        /// <returns>Connection String</returns>
        public static string GetConnectionString(string? fallback = null)
        {
            // 1) Ưu tiên ENV để dễ đổi ở máy thật / CI/CD
            var env = Environment.GetEnvironmentVariable("YOURAPP_SQL_CONN");
            if (!string.IsNullOrWhiteSpace(env))
                return env;

            // 2) Dùng fallback nếu có
            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;

            // 3) Mặc định DEV (đổi lại cho phù hợp)
            // - TrustServerCertificate=False: nên cấu hình CA đúng; dev có thể tạm True
            // - Connect Timeout=15: kết nối không quá 15s
            // - Application Name: giúp DBA truy vết kết nối
            return "Server=localhost,1433;" +
                    "Database=Docker_DB;" +
                    "User Id=sa;" +
                    "Password=123456789;" +
                    "TrustServerCertificate=True;" +
                    "Connect Timeout=15;" +
                    "Application Name=Test";
        }
    }
}
```

Nhớ thay thế các thông tin tương ứng trong `chuỗi kết nối`.  

### 2.2 Nhận diện lỗi

Khi truy vấn dữ liệu từ CSDL, nếu SQL Server trả về lỗi mà trùng với các lỗi nên `retry` thì chúng ta sẽ thử lại lần nữa.  

```C#
using Microsoft.Data.SqlClient;

namespace Test.Database
{
    /// <summary>
    /// Xác định lỗi SQL "transient" (có thể tự khỏi) để Retry.
    /// Danh sách gồm: deadlock (1205), connection reset/network (10053/10054/10060),
    /// login/server busy (4060/40197/40501) – bao trùm cả on-prem & Azure.
    /// </summary>
    public static class TransientErrorDetector
    {
        // Các mã lỗi hay gặp (không đầy đủ tuyệt đối, đủ dùng thực tế)
        private static readonly int[] TransientNumbers = new[]
        {
            1205,   // Deadlock
            4060,   // Cannot open database requested by the login
            40197,  // The service has encountered an error processing your request (Azure)
            40501,  // The service is currently busy (Azure)
            40613,  // Database not currently available (Azure)
            10928, 10929, // Resource limits (Azure)
            49918, 49919, 49920, // Cannot process request (Azure)
            10053, 10054, 10060, // Net/connection aborted/reset/timed out
            233,    // Connection init error / pipe error
        };

        /// <summary>
        /// Trả true nếu nên retry.
        /// </summary>
        public static bool IsTransient(SqlException ex)
        {
            foreach (SqlError err in ex.Errors)
                if (Array.IndexOf(TransientNumbers, err.Number) >= 0)
                    return true;
            return false;
        }
    }
}
```
### 2.3 Chính sách truy vấn lại hoặc mở break

Khi SQL Server tự động truy vấn lại, ta cũng nên kiểm soát việc truy vấn lại, ko để mwor kết nối bừa bãi, truy vấn liên tục gây treo hệ thống, ảnh hưởng xấu đến dữ liệu hiện có. Vì vậy ta cấu hình các chính sách tương ứng vào tệp `SqlPolicies.cs`:  

Việc `retry` hay `đóng/mở break` được thực hiện trong `Pipeline`. `Pipeline` này chịu trách nhiệm mở kết nối và thực hiện toàn bộ quá trình truy vấn.  
Đối với `retry` thì ta cấu hình vào biến `retry` trong `pipeline`:  
```C#
var retry = new RetryStrategyOptions
{
    MaxRetryAttempts = maxRetryAttempts,         // tối đa N lần thử lại
    Delay = baseDelay.Value,                     // delay cơ sở
    BackoffType = DelayBackoffType.Exponential,  // exponential backoff
    UseJitter = true,                            // thêm jitter tránh đồng loạt retry cùng lúc
    ShouldHandle = shouldHandle,                 // chỉ retry các lỗi định nghĩa ở trên

    // Callback khi RETRY diễn ra
    // Chú ý: OnRetryArguments KHÔNG có Delay → không log DelayMs ở đây.
    OnRetry = args =>
    {
        var ex = args.Outcome.Exception; // Exception gây retry (nếu có)
        var sql = ex as SqlException ?? ex?.InnerException as SqlException;
        // Gom các mã lỗi SQL (nếu có) thành chuỗi, ví dụ: "1205,4060"
        var codes = sql == null
            ? "n/a"
            : string.Join(",", sql.Errors.Cast<SqlError>().Select(e => e.Number));

        // Ghi log cảnh báo (warning)
        LogEx.Warning(
            "Thử lại truy vấn SQL lần thứ {Attempt}, sqlErrors=[{Codes}]",
            args.AttemptNumber, codes);

        return default; // ValueTask.CompletedTask
    }
};
```
Trong đó:  
- `MaxRetryAttempts`: Là số lần tối đa cho phép thử lại, nên để dưới 5 lần, vì nếu một khi đã lỗi, không nên thử lại quá nhiều lần, chỉ nên thử lại 2-3 lần và vẫn tiếp tục gặp lỗi thì chúng ta nên `ghi Log` để quan sát, tránh thử lại liên tục gây quá tải cho hệ thống, hoặc gây lỗi, ...  
- `Delay`: Thời gian giữa các lần thử lại  
- `BackoffType`: Chưa tìm hiểu  
- `UseJitter`: Tránh viêc retry xảy ra đồng thời  
- `ShouldHandler`: Chỉ `retry` các lỗi nằm trong tệp `TransientErrorDetector.cs` mà ta đã quy ước, tránh `retry` các lỗi như `sai cú pháp`, `dữ liệu không hợp lệ`, ...  
- `OnRetry`: Trong mỗi lần truy vấn lại, ta đều phải `ghi log` rõ ràng truy vấn lại lần bao nhiêu, vì sao lại truy vấn lại để dễ dàng phân tích và xử lý  

Ta có code hoàn chỉnh cho cấu hình chính sách khi truy vấn như bên dưới:  
```C#
using System;
using System.Linq;                             // dùng Select(...) để gom mã lỗi SQL
using Microsoft.Data.SqlClient;                // SqlException
using Polly;                                   // ResiliencePipeline, PredicateBuilder, builder
using Polly.Retry;                             // RetryStrategyOptions, DelayBackoffType
using Polly.CircuitBreaker;                    // CircuitBreakerStrategyOptions
using Test.Utils;                              // LogEx (Serilog wrapper)

namespace Test.Database
{
    /// <summary>
    /// Tạo Resilience Pipeline: Retry + Circuit Breaker
    /// + Gắn log (LogEx) cho sự kiện retry / breaker open / close / half-open.
    /// </summary>
    public static class SqlPolicies
    {
        public static ResiliencePipeline CreatePipeline(
            int maxRetryAttempts = 3,                 // số lần thử lại (không tính lần đầu)
            TimeSpan? baseDelay = null,               // delay cơ sở cho backoff
            int minThroughput = 20,                   // Tối thiểu số request trong “cửa sổ” để breaker đánh giá
            double failureRatio = 0.5,                // tỷ lệ lỗi để mở mạch (50%)
            TimeSpan? breakDuration = null)           // thời gian “mở mạch”
        {
            baseDelay ??= TimeSpan.FromMilliseconds(200);  // mặc định 200ms nếu không truyền vào
            breakDuration ??= TimeSpan.FromSeconds(20);     // mặc định 20s nếu không truyền vào

            // 1) shouldHandle: quy định LOẠI lỗi mà Retry/Breaker sẽ can thiệp
            //    - .Handle<SqlException>(predicate): chỉ nhận các SqlException "transient" (tạm thời)
            //    - .Handle<TimeoutException>(): cộng thêm lỗi Timeout tự định nghĩa
            var shouldHandle = new PredicateBuilder()
                .Handle<SqlException>(TransientErrorDetector.IsTransient)  // chỉ lỗi SQL “transient”
                .Handle<TimeoutException>();                               // + timeout

            // 2) Chiến lược Retry 
            var retry = new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetryAttempts,         // tối đa N lần thử lại
                Delay = baseDelay.Value,                     // delay cơ sở
                BackoffType = DelayBackoffType.Exponential,  // exponential backoff
                UseJitter = true,                            // thêm jitter tránh đồng loạt retry cùng lúc
                ShouldHandle = shouldHandle,                 // chỉ retry các lỗi định nghĩa ở trên

                // Callback khi RETRY diễn ra
                // Chú ý: OnRetryArguments KHÔNG có Delay → không log DelayMs ở đây.
                OnRetry = args =>
                {
                    var ex = args.Outcome.Exception; // Exception gây retry (nếu có)
                    var sql = ex as SqlException ?? ex?.InnerException as SqlException;
                    // Gom các mã lỗi SQL (nếu có) thành chuỗi, ví dụ: "1205,4060"
                    var codes = sql == null
                        ? "n/a"
                        : string.Join(",", sql.Errors.Cast<SqlError>().Select(e => e.Number));

                    // Ghi log cảnh báo (warning)
                    LogEx.Warning(
                        "Thử lại truy vấn SQL lần thứ {Attempt}, sqlErrors=[{Codes}]",
                        args.AttemptNumber, codes);

                    return default; // ValueTask.CompletedTask
                }
            };

            // 3) Circuit Breaker : khi lỗi dồn dập, tạm "mở mạch" để fail nhanh thay vì dồn thêm tải
            var breaker = new CircuitBreakerStrategyOptions
            {
                ShouldHandle = shouldHandle,                 // Cùng tiêu chí lỗi như Retry, lỗi nào được tính (SqlException transient, TimeoutException)
                FailureRatio = failureRatio,                 // Ngưỡng tỷ lệ lỗi trong SamplingDuration (vd 0.5 = 50%)
                MinimumThroughput = minThroughput,           // Số lượng tối thiểu trong cửa sổ để breaker đánh giá
                SamplingDuration = TimeSpan.FromSeconds(30), // Cửa sổ đo quan sát (30s)
                BreakDuration = breakDuration.Value,         // Khi mở mạch, giữ nguyên trong bao lâu

                // Khi BREAKER chuyển sang trạng thái mở mạch (open)
                OnOpened = args =>
                {
                    var ex = args.Outcome.Exception;
                    var sql = ex as SqlException ?? ex?.InnerException as SqlException;
                    var codes = sql == null
                        ? "n/a"
                        : string.Join(",", sql.Errors.Cast<SqlError>().Select(e => e.Number));

                    LogEx.Error(
                        "SQL CIRCUIT đã chuyển sang trạng thái OPEN trong {BreakMs}ms, sqlErrors=[{Codes}]. Tất cả kết nối sẽ tạm thời thất bại.",
                        (int)args.BreakDuration.TotalMilliseconds, codes);

                    return default;
                },

                // Khi BREAKER khép lại (Reset → Closed) sau thời gian nghỉ
                OnClosed = args =>
                {
                    LogEx.Information("SQL CIRCUIT đã đóng, tất cả kết nối tới DB được mở lại");
                    return default;
                },

                // Khi breaker Half-Open (nửa mở): Polly cho qua vài request “thăm dò”
                OnHalfOpened = args =>
                {
                    LogEx.Information("SQL CIRCUIT chuyển sang chế độ HALF-OPEN, cho phép một số yêu cầu tới DB.");
                    return default;
                }
            };

            // 4) Dựng pipeline theo thứ tự: Retry → CircuitBreaker
            //    - Request đi vào tầng Retry trước (có thể lặp lại)
            //    - Nếu lỗi dồn dập, Breaker sẽ mở để các request tiếp theo fail nhanh
            var pipeline = new ResiliencePipelineBuilder()
                .AddRetry(retry)               // tầng Retry
                .AddCircuitBreaker(breaker)    // tầng Circuit Breaker
                .Build();                      // tạo pipeline

            return pipeline;
        }
    }
}
```
Song song với đó ta có `Circuit Breaker` sẽ chịu trách nhiệm kiểm soát lỗi xảy ra, nếu lượng lỗi ta quy định trong `TransientErrorDetector` xảy ra quá nhiều trong 1 thời gian ngắn thì ta phải `tạm dừng` các `request` tới CSDL của chúng ta để `giảm tải áp lực` cho CSDL.  

Có 3 trạng thái chính của `Circuit Breaker (Polly v8)` như sau:  
#### 1. Closed (đóng)

Đây là trạng thái bình thường, `Circuit break` không hoạt động, vì vậy các yêu cầu đến DB hoạt động bình thường, tuy nhiên các lỗi thuộc `ShouldHandle` sẽ được đếm và tính lưu lượng trong `mỗi cửa sổ đo`.  
`Mỗi cửa sổ đo` 30 phút là hệ thống tính toán trong 30 phút có bao nhiêu yêu cầu (yêu cầu thuộc `ShouldHandler`) được ghi lại, nếu vượt quá số lượng này thì tiến hành chuyển sang trạng thái khác.  

#### 2. Open (mở)

Khi tỉ lệ lỗi `vượt ngưỡng` cho phép trong `mỗi cửa sổ` và `đủ số lượng tối thiểu` (`MinimumThroughput`) thì `Breaker` sẽ chuyển sang trạng thái `OPEN` trong `BreakDuration` (trong khoảng thời gian ví dụ 20s).  

Trong khoảng thời gian ở trạng thái này thì mọi yêu cầu gửi tới DB đều sẽ thất bại và trả về với ngoại lệ `BrokenCircuitException`, việc này giúp cho giảm áp lực lên DB đang gặp tình trạng quá nhiều yêu cầu trong thời gian ngắn.  

`Circuit Breaker` sẽ ngăn chặn các kết nối trước khi nó được kết nối tới CSDL nên CSDL sẽ được `nghỉ` trong suốt thời gian trạng thái `OPEN`.  
#### 3. Half-Open (nửa mở)

Là khoảng thời gian sau khi hết `BreakDuration`, `Breaker` chuyển sang giai đoạn `Half-Open`, `Polly` cho phép một số ít yêu cầu gửi đến DB, nếu tất cả đều thành công, không có lỗi nào nằm trong `ShouldHandler` thì `breaker` đóng lại, chuyển về trạng thái `CLOSED`. Còn nếu lại lỗi thì `Breaker` mở lại trạng thái `OPEN` thêm khoảng thời gian `BrakDuration` lần nữa.  

Vì vậy ta cần cấu hình `breaker` như sau:  
```C#
var breaker = new CircuitBreakerStrategyOptions
{
    ShouldHandle = shouldHandle,                 // Cùng tiêu chí lỗi như Retry, lỗi nào được tính (SqlException transient, TimeoutException)
    FailureRatio = failureRatio,                 // Ngưỡng tỷ lệ lỗi trong SamplingDuration (vd 0.5 = 50%)
    MinimumThroughput = minThroughput,           // Số lượng tối thiểu trong cửa sổ để breaker đánh giá
    SamplingDuration = TimeSpan.FromSeconds(30), // Cửa sổ đo quan sát (30s)
    BreakDuration = breakDuration.Value,         // Khi mở mạch, giữ nguyên trong bao lâu

    // Khi BREAKER chuyển sang trạng thái mở mạch (open)
    OnOpened = args =>
    {
        var ex = args.Outcome.Exception;
        var sql = ex as SqlException ?? ex?.InnerException as SqlException;
        var codes = sql == null
            ? "n/a"
            : string.Join(",", sql.Errors.Cast<SqlError>().Select(e => e.Number));

        LogEx.Error(
            "SQL CIRCUIT đã chuyển sang trạng thái OPEN trong {BreakMs}ms, sqlErrors=[{Codes}]. Tất cả kết nối sẽ tạm thời thất bại.",
            (int)args.BreakDuration.TotalMilliseconds, codes);

        return default;
    },

    // Khi BREAKER khép lại (Reset → Closed) sau thời gian nghỉ
    OnClosed = args =>
    {
        LogEx.Information("SQL CIRCUIT đã đóng, tất cả kết nối tới DB được mở lại");
        return default;
    },

    // Khi breaker Half-Open (nửa mở): Polly cho qua vài request “thăm dò”
    OnHalfOpened = args =>
    {
        LogEx.Information("SQL CIRCUIT chuyển sang chế độ HALF-OPEN, cho phép một số yêu cầu tới DB.");
        return default;
    }
};
```
Cách thức mà `Breaker` hoạt động như sau:  

Đầu tiên hệ thống (`Breaker`) sẽ nhìn vào khoảng thời gian `SamplingDuration` (ví dụ 30s) gần nhất, sau đó nếu trong 30s có ít nhất `MinimumThroughput` (ví dụ 10 request trong 30s) thì `Breaker` mới bắt đầu xem xét mở mạch (tránh mở mạch vì một vài lỗi lẻ tẻ).  

Một khi `Breaker` tiến hành xem xét mở mạch, nó sẽ kiểm tra trong các yêu cầu bị lỗi (các lỗi phải nằm trong `ShouldHandle`) thì sẽ toán toán tỉ lệ lỗi dựa theo `FailureRatio` (ví dụ 0.5 tương đương với 50%), nếu tỉ lệ lỗi vượt ngưỡng `FailureRatio` thì `Breaker` chuyển sang chế độ `OPEN`  

Hết thời gian `BreakDuration` thì sang chế độ `HALF-OPEN` rồi xem xét có sang `CLOSED` hoặc mở tiếp `OPEN`.  

Ta có thể xử lý khi ở trạng thái này bằng cách bắt ngoại lệ `BrokenCircuitException` được ném ra:  
```C#
try
{
    // Khoá nút Load trong khi đang tải để tránh bấm liên tục
    btnLoad.Enabled = false;
    statusLabel.Text = "Đang tải từ database...";

    // ====== Gọi repository ======
    // Trường hợp 1: Lấy tất cả (email = null)
    var results = await _loginRepo.GetLoginAsync(email: null, ct: _cts.Token);

    // Lấy dòng đầu tiên (nếu có). Bạn có thể xử lý theo nhu cầu của mình (ví dụ đếm rows, v.v.)
    var first = results.FirstOrDefault();

    // ====== Cập nhật UI ======
    if (first != null)
    {
        // Hiển thị họ tên + email (tuỳ biến theo DTO của bạn)
        statusLabel.Text = $"User: {first.User_Name} | Email: {first.Email}";
    }
    else
    {
        statusLabel.Text = "Không có bản ghi nào trong Users_Login.";
    }
}
catch (BrokenCircuitException) // Circuit đang mở → fail nhanh
{
    statusLabel.Text = "Dịch vụ DB tạm quá tải. Thử lại sau ít giây...";
}
```

Nếu DB đang quá tải/mạng chập chờn, việc cứ tiếp tục gửi `hàng nghìn request` sẽ làm tình hình tệ hơn. `Breaker` “mở” giúp bảo vệ DB bằng cách từ chối sớm trong một khoảng thời gian, cho hạ tầng có thời gian hồi.  

## 3. Tạo kết nối đến DB

Ta đã tạo các chính sách cho mỗi kết nối, bây giờ ta cần tạo kết nối tới CSDL ở tệp `SqlConnectionFactory.cs` như sau:  

```C#
using Microsoft.Data.SqlClient;           // Dùng provider hiện đại cho SQL Server (thay cho System.Data.SqlClient)

namespace Test.Database
{
    /// <summary>
    /// Factory tạo kết nối SQL theo nguyên tắc:
    /// - "open late": chỉ mở ngay trước khi dùng
    /// - "close early": đóng/Dispose càng sớm càng tốt (để trả về pool)
    /// </summary>
    public static class SqlConnectionFactory
    {
        /// <summary>
        /// Tạo và TRẢ VỀ 1 SqlConnection đã MỞ SẴN.
        /// Lưu ý: người GỌI chịu trách nhiệm Dispose() (thường dùng 'using' / 'await using').
        /// </summary>
        public static async Task<SqlConnection> OpenAsync(string connString, CancellationToken ct)
        {
            // Tạo đối tượng kết nối dựa vào connection string.
            // ADO.NET có connection pooling tự động theo connString: đóng/Dispose không phá kết nối thật ngay,
            // mà đưa nó về pool để tái sử dụng cực nhanh ở lần sau.
            var conn = new SqlConnection(connString);

            // Mở kết nối ở chế độ async để KHÔNG chặn (block) UI thread.
            // Truyền CancellationToken 'ct' để có thể HỦY quá trình mở nếu người dùng bấm Cancel
            /*
             * Ví dụ: mất mạng, SQL Server không phản hồi, v.v.
             * 
             * using var cts = new CancellationTokenSource(); // tạo CTS để hủy (phải bấm Cancel)
             * using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // auto-cancel sau 10s
             * 
             * await SqlConnectionFactory.OpenAsync(connStr, cts.Token); // kết nối tới SQL và truyền token
             * 
             * cts.Cancel(); // --> OpenAsync sẽ ném OperationCanceledException nếu đang chờ (bất cứ khi nào người dùng bấm Cancel)
             */
            await conn.OpenAsync(ct)

                // ConfigureAwait(false): trong THƯ VIỆN (library) nên dùng false để:
                //  - không yêu cầu "quay về" SynchronizationContext ban đầu (UI/ASP.NET)
                //  - tránh deadlock khi người gọi (ở nơi nào đó) lỡ làm .Wait()/.Result
                //  - tiết kiệm chi phí marshal context
                // Ở tầng UI, KHÔNG dùng false nếu cần quay lại đúng UI thread sau await.
                .ConfigureAwait(false);

            // Trả về kết nôi đã mở sẵn
            return conn;
        }
    }
}
```
Khi kết nối đến DB ta cần 2 tham số sau:  
- `connString`: Là chuỗi kết nối được lấy từ hàm `GetGetConnectionString`  
- `ct`: Là `CancellationToken`, nó có nhiệm vụ hủy bỏ kết nối tới DB bất cứ lúc nào ta muốn (truyền tín hiệu vào để dừng kết nối)  

Bất cứ khi nào nhận đc tín hiệu từ `CancellationToken` thì hệ thống lập tức ném ra một ngoại lệ `OperationCanceledException` để hủy bỏ kết nối tới DB. Sử dụng cho các tác vụ khi bị treo, truy vấn quá lâu, hoặc quá tải thì ta có thể `cường ép` dừng truy vấn tới DB.  

Ta có `UI thread` (luồng giao diện) là luồng duy nhất chạy message loop của WinForms/WPF, chịu trách nhiệm:  
- Nhận và xử lý sự kiện (click, paint, key…)  
- Vẽ controls  
- Cập nhật giao diện  
Mọi thao tác đối với giao diện đều `chỉ được phép` thực hiện ở `UI Thread`, vì đây là luồng đã tạo ra nó ban đầu. Khi cập nhật giao diện từ các luồng khác có thể dẫn đến lỗi `InvalidOperationException (“Cross-thread operation not valid”)`.  

Vì vậy không được chặn `UI Thread` bằng các thao tác nặng, lâu như mạng, DB, IO hoặc xử lý CPU lấu, nếu ko `UI thread` sẽ bị chặn, của sổ màn hình sẽ `Not Responding`.  

Ta có `UI Handler` là các luồng xử lý sự kiện chạy trên `UI Thread`. Ví dụ như xử lý sự kiện click nút bấm, ...  
```C#
private async void btnLoad_Click(object sender, EventArgs e) { ... }
```
Khi người dùng bấm nút, WinForms gọi handler này trên UI thread --> handler nên trả quyền sớm (không block), để UI tiếp tục mượt.  

Và vì hàm truy vấn tới DB là hàm bất đồng bộ (`sử dụng được await`) nên việc cấu hình `ConfigureAwait(false)` là nên thực hiện. Để báo cho hàm này không được bám vào `UI context`, phần tiếp theo không cần quay lại `UI thread` (phần tiếp theo là truy vấn vào DB nên ko đc chạy trên UI Thread để tránh làm đơ giao diện). Tránh `deadlock` cổ điển (khi người gọi lỡ dùng `.Result/.Wait()` trong môi trường có `SynchronizationContext` — tệ, nhưng vẫn hay xảy ra  

> Lưu ý chỉ sử dụng ConfigureAwait(false) trong thư viện OpenAsyn và các thư viện khác, không sử dụng trong UI Handler (nó sẽ ảnh hưởng đến UI)  

Ví dụ:  
Khi người dùng xử lý sự kiện bấm nút kết nối tới CSDL từ giao diện. Ta có 2 cách xử lý như sau:  

Nếu đã cấu hình `ConfigureAwait(false)` trong hàm kết nối thì trong `UI Handler` click nút bấm ta chỉ cần gọi `await` xong tiếp tục cập nhật giao diện hoặc xử lý dữ liệu, vì `ConfigureAwait(false)` chỉ cấu hình trong thư viện kết nối DB, chứ ko phải ở UI.  
```C#
// UI handler — KHÔNG dùng .ConfigureAwait(false)
statusLabel.Text = "Đang mở...";
var conn = await SqlConnectionFactory.OpenAsync(connStr, _cts.Token); // bên dưới có ConfigureAwait(false)
statusLabel.Text = "Đã mở xong";  // OK: bạn vẫn đang ở UI thread
```
Còn nếu ta cấu hình `ConfigureAwait(false)` ngay tại `UI handler đang chạy trên UI thread` thì cần phải chuyển đổi thủ công về `UI thread` rồi mới cập nhật giao diện phần mềm.  
```C#
await SomethingAsync().ConfigureAwait(false);
// lúc này thường KHÔNG ở UI thread
BeginInvoke((Action)(() => statusLabel.Text = "xong")); // chuyển về UI thread thủ công
```

## 4. Thực hiện lệnh truy vấn, chỉnh sửa DB

Sau khi kết nối tới DB, các lệnh áp dụng tới `SQL Server như SELECT/INSERT/UPDATE ...` đều được thực hiện qua thư viện `Dapper`, để đảm bảo các truy vấn được áp dụng chính sách đã quy định.  

Các cú pháp được ghi vào tệp `DbExecutor.cs`:  

```C#
using System.Data;
using Dapper;                           // Dapper micro-ORM
using Polly;                            // ResiliencePipeline

namespace Test.Database
{
    /// <summary>
    /// Thực thi truy vấn SQL (Dapper) qua Polly v8 ResiliencePipeline:
    /// - Có Retry + Circuit Breaker thông qua SQLPolicies 
    /// - Có Timeout (CommandDefinition) + CancellationToken tránh treo vô hạn
    /// - Kết nối ngắn: open late / close early
    /// </summary>
    public class DbExecutor
    {
        private readonly string _connString;           // chuỗi kết nối, không cho phép thay đổi
        private readonly ResiliencePipeline _pipeline; // pipeline resilience dùng chung

        public DbExecutor(string? connString = null, ResiliencePipeline? pipeline = null)
        {
            _connString = DbConfig.GetConnectionString(connString);        // Lấy chuỗi kết kết nối
            _pipeline = pipeline ?? SqlPolicies.CreatePipeline();        // Tạo pipeline mặc định như đã cấu hình trong SQLPilicies
        }

        /// <summary>
        /// SELECT trả IEnumerable&lt;T&gt; (Dapper).
        /// Dùng .AsTask() vì ExecuteAsync (v8) trả ValueTask&lt;T&gt;.
        /// </summary>
        /// Hàm này bất đồng bộ (Task) trả về một tập hợp IEnumerable<T>
        /// Trong đó T (Generic) là kiểu mà ta tự định nghĩa khi trả về kết quả
        public Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? param = null,
            int commandTimeoutSeconds = 30,
            CommandType commandType = CommandType.Text,
            CancellationToken ct = default)
        => _pipeline.ExecuteAsync(async token =>                 // => là cú pháp rút gọn của return
        {
            // Mỗi lần gọi mở 1 kết nối mới (pooling tái sử dụng ngầm), và tự Dispose để trả connection về pool
            using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);

            // Dapper QueryAsync có CommandDefinition → set timeout + cancel
            var result = await conn.QueryAsync<T>(new CommandDefinition(
                commandText: sql,
                parameters: param,
                commandType: commandType,
                commandTimeout: commandTimeoutSeconds,
                cancellationToken: token)).ConfigureAwait(false);

            return result; // IEnumerable<T>
        }, ct).AsTask();  // <— CHUYỂN ValueTask<IEnumerable<T>> → Task<IEnumerable<T>>

        /// <summary>
        /// INSERT/UPDATE/DELETE trả số dòng ảnh hưởng.
        /// </summary>
        public Task<int> ExecuteAsync(
            string sql,
            object? param = null,
            int commandTimeoutSeconds = 30,
            CommandType commandType = CommandType.Text,
            CancellationToken ct = default)
        => _pipeline.ExecuteAsync(async token =>
        {
            using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);

            var affected = await conn.ExecuteAsync(new CommandDefinition(
                commandText: sql,
                parameters: param,
                commandType: commandType,
                commandTimeout: commandTimeoutSeconds,
                cancellationToken: token)).ConfigureAwait(false);

            return affected; // int
        }, ct).AsTask(); // <— CHUYỂN ValueTask<int> → Task<int>

        /// <summary>
        /// Giao dịch nhiều lệnh phụ thuộc nhau (retry toàn khối nếu transient).  
        /// Nhận kết quả trả về
        /// </summary>
        public Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<IDbConnection, IDbTransaction, CancellationToken, Task<TResult>> body,
            IsolationLevel isolation = IsolationLevel.ReadCommitted,
            CancellationToken ct = default)
        => _pipeline.ExecuteAsync(async token =>
        {
            // Tạo connect và IDTransaction
            // Mọi lệnh SQL muốn nằm trong cùng một giao dịch phải được truyền transaction: tx (và dùng đúng conn đã tạo tx)
            using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);
            using var tx = conn.BeginTransaction(isolation);

            try
            {
                // Cho phần thân tự thực thi các lệnh Dapper; đảm bảo truyền conn, tx để cùng nằm trong 1 giao dịch
                var result = await body(conn, tx, token).ConfigureAwait(false);

                // Nếu tất cả lệnh thành công thì commit để lưu lại
                tx.Commit();
                return result; // dummy int để khớp generic; sẽ bị bỏ qua
            }
            catch
            {
                // Tiến hành rollback nếu 1 giao dịch lỗi
                try { tx.Rollback(); } catch { /* ignore rollback error */ }
                throw;
            }
        }, ct).AsTask();

        /// <summary>
        /// Giao dịch nhiều lệnh phụ thuộc nhau (retry toàn khối nếu transient).  
        /// Không nhận kết quả trả về
        /// </summary>
        //public Task ExecuteInTransactionAsync(
        //    Func<IDbConnection, IDbTransaction, CancellationToken, Task> body,
        //    IsolationLevel isolation = IsolationLevel.ReadCommitted,
        //    CancellationToken ct = default)
        //=> _pipeline.ExecuteAsync(async token =>
        //{
        //    // Tạo connect và IDTransaction
        //    // Mọi lệnh SQL muốn nằm trong cùng một giao dịch phải được truyền transaction: tx (và dùng đúng conn đã tạo tx)
        //    using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);
        //    using var tx = conn.BeginTransaction(isolation);

        //    try
        //    {
        //        // Cho phần thân tự thực thi các lệnh Dapper; đảm bảo truyền conn, tx để cùng nằm trong 1 giao dịch
        //        await body(conn, tx, token).ConfigureAwait(false);

        //        // Nếu tất cả lệnh thành công thì commit để lưu lại
        //        tx.Commit();
        //        return 0; // dummy int để khớp generic; sẽ bị bỏ qua
        //    }
        //    catch
        //    {
        //        // Tiến hành rollback nếu 1 giao dịch lỗi
        //        try { tx.Rollback(); } catch { /* ignore rollback error */ }
        //        throw;
        //    }
        //}, ct).AsTask(); // <— CHUYỂN ValueTask<int> → Task
    }
}
```
Ta có class `DbExecutor` thực hiện cho ta các lệnh `Query`, `Executor` hoặc tạo các `transaction`.  Class này bọc việc gọi SQL qua `Dapper` kèm theo `Polly pipeline` để:  

- Retry lỗi tạm (deadlock, reset…)  
- Circuit Breaker để “fail nhanh” khi lỗi dồn dập  
- Timeout + Cancel để không treo  

Mỗi thao tác mở kết nối đều sử dụng `using` để có thể trả `connection` về `pool` sớm nhất.  

Hàm `Query` có cấu trúc:  
```C#
public Task<IEnumerable<T>> QueryAsync<T>(
    string sql,
    object? param = null,
    int commandTimeoutSeconds = 30,
    CommandType commandType = CommandType.Text,
    CancellationToken ct = default)
=> _pipeline.ExecuteAsync(async token =>                 // => là cú pháp rút gọn của return
{
    // Mỗi lần gọi mở 1 kết nối mới (pooling tái sử dụng ngầm), và tự Dispose để trả connection về pool
    using var conn = await SqlConnectionFactory.OpenAsync(_connString, token).ConfigureAwait(false);

    // Dapper QueryAsync có CommandDefinition → set timeout + cancel
    var result = await conn.QueryAsync<T>(new CommandDefinition(
        commandText: sql,
        parameters: param,
        commandType: commandType,
        commandTimeout: commandTimeoutSeconds,
        cancellationToken: token)).ConfigureAwait(false);

    return result; // IEnumerable<T>
}, ct).AsTask();  // <— CHUYỂN ValueTask<IEnumerable<T>> → Task<IEnumerable<T>>
```
Một `T (Generic)` mẫu:  
Theo kiểu `schema`:  
```C#
public sealed class LoginDto
{
    public string ID { get; init; } = "";           
    public string User_Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Password { get; init; } = "";  // Thực tế: DIỄN GIẢN – không nên expose
    public string Avatar { get; init; } = "";    // đường dẫn/URL?
    public string Privilege { get; init; } = "";
    public DateTime? Activate { get; init; }          // nếu BIT NOT NULL
}
```
Trong đó:  
- `Task<IEnumerable<T>` : Trả về bất đồng bộ (`Task`) một tập hợp `IEnumerable<T>`, với T (`Generic`) là kiểu phần tử mà ta tự quyết định khi gọi (ví dụ như schema Login, kiểu int, kiểu dynamic, ...). Trong winform `bắt buộc` kiểu trả về `phải rõ ràng` ở chữ ký method. Nếu chưa biết kiểu trả về là gì thì ta có thể sử dụng `dynamic`, `Dapper` map mỗi cột trong kết quả SQL thành thuộc tính của một đối tượng `DapperRow` (trả về qua dynamic)  
- `_pipeline.ExecuteAsync(async token => { ... }, ct)`: Mọi thao tác đều phải đi qua `Polly` để có chức năng nếu gặp lỗi (thỏa mãn `Policies`) thì tự retry, nếu nhiều lỗi dồn dập trong thời gian ngắn thì mở `Breaker`  
- `conn.QueryAsync<T>(new CommandDefinition(...))`: `Dapper` sẽ đọc dữ liêu trả về và tự `map` nó với `T` ta đã định nghĩa (dựa theo `tên cột` ↔ `tên property`)
- `CommandDefinition`: Cho phép đặt `timeout`, `cancellationToken` trong mỗi lệnh truy vấn  
- `AsTask()`: Cuối cùng `Polly` trả về `ValueTask<T>` và ta sẽ chuyển về `Task<T>` để thân thiện với toàn bộ code (Vì trong winform nó đủ tốt và rất phổ biến)  

Ví dụ mẫu truy vấn CSDL: 
```C#
// Lấy kết nối từ DbExecutor
private readonly Test.Database.DbExecutor _db;

// 1) Lấy danh sách tài khoản → ánh xạ vào DTO
IEnumerable<LoginDto> rows = await _db.QueryAsync<LoginDto>(
    "SELECT ID, User_Name, Email, Privilege, Activate FROM dbo.Users_Login WHERE Activate = 1;",
    ct: _cts.Token);

// 2) Lấy danh sách int (ví dụ ID)
IEnumerable<int> ids = await _db.QueryAsync<int>(
    "SELECT ID FROM dbo.Users_Login WHERE Privilege = @p;",
    new { p = "Admin" }, ct: _cts.Token);

// 3) Dùng stored procedure
var users = await _db.QueryAsync<LoginDto>(
    "dbo.GetActiveUsers",
    param: new { minPrivilege = 2 },
    commandType: CommandType.StoredProcedure,
    ct: _cts.Token);

// Cập nhật tên
int n = await _db.ExecuteAsync(
    "UPDATE dbo.Users_Login SET User_Name = @Name WHERE ID = @ID;",
    new { Name = "Quan", ID = 123 }, ct: _cts.Token);

// Xoá user chưa kích hoạt
int removed = await _db.ExecuteAsync(
    "DELETE FROM dbo.Users_Login WHERE Activate = 0;", ct: _cts.Token);

// Thực hiện transaction
await _db.ExecuteInTransactionAsync(async (conn, tx, token) =>
{
    // 1) Trừ tiền tài khoản A
    await conn.ExecuteAsync(new CommandDefinition(
        "UPDATE Wallet SET Balance = Balance - @amount WHERE UserId = @u;",
        new { amount = 100, u = 1 }, transaction: tx, cancellationToken: token));

    // 2) Cộng tiền tài khoản B
    await conn.ExecuteAsync(new CommandDefinition(
        "UPDATE Wallet SET Balance = Balance + @amount WHERE UserId = @v;",
        new { amount = 100, v = 2 }, transaction: tx, cancellationToken: token));

    // 3) Ghi log giao dịch
    await conn.ExecuteAsync(new CommandDefinition(
        "INSERT INTO TransferLog(FromUser, ToUser, Amount, At) VALUES (@a, @b, @m, SYSUTCDATETIME());",
        new { a = 1, b = 2, m = 100 }, transaction: tx, cancellationToken: token));
},
isolation: IsolationLevel.ReadCommitted,
ct: _cts.Token);
```

## 5. Truy vấn trong 1 bảng dữ liệu

Ta tạo 1 bảng `Wallet` trong `SQL Server` để lưu trữ các giao dịch về tiền:  
```SQL
IF OBJECT_ID('dbo.Wallet', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Wallet(
        UserId  INT        NOT NULL PRIMARY KEY,
        Balance DECIMAL(18,2) NOT NULL DEFAULT(0)
    );

    INSERT INTO dbo.Wallet(UserId, Balance) VALUES (1, 500.00), (2, 200.00);
END
```
Sau đó ta tạo model để trả về kiểu dữ liệu cho các giao dịch của bảng này tại thư mục: `Database/Model/Transfer_Model.cs`:  
```C#
namespace Test.Database.Model
{
    public sealed class TransferResult
    {
        public int FromUserId { get; init; }
        public int ToUserId { get; init; }
        public decimal Amount { get; init; }
        public decimal FromBalanceAfter { get; init; }
        public decimal ToBalanceAfter { get; init; }
    }

}
```

Sau đó tạo `transaction` cho việc trừ tiền trong tài khoản người `A` và cộng tiền cho người `B` tại thư mục: `Database/Table/TransferTable.cs`:  
```C#
// WalletService.cs
using Dapper;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Test.Database.Model;

namespace Test.Database.Table
{
    public class WalletService
    {
        private readonly DbExecutor _db;
        public WalletService(DbExecutor db) => _db = db;

        public Task<TransferResult> TransferAsync(
            int fromUserId, int toUserId, decimal amount, CancellationToken ct)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (fromUserId == toUserId) throw new ArgumentException("Nguồn và đích phải khác nhau.");

            return _db.ExecuteInTransactionAsync<TransferResult>(async (conn, tx, token) =>
            {
                // 1) Khóa & kiểm tra số dư nguồn (UPDLOCK để giữ lock ghi đến hết transaction)
                var fromBalance = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition(
                    "SELECT Balance FROM dbo.Wallet WITH (UPDLOCK, ROWLOCK) WHERE UserId = @U;",
                    new { U = fromUserId }, transaction: tx, cancellationToken: token));

                if (fromBalance < amount)
                    throw new InvalidOperationException("Số dư không đủ.");

                // 2) Trừ tiền nguồn và LẤY SỐ DƯ MỚI bằng OUTPUT
                var newFromBalance = await conn.QuerySingleAsync<decimal>(new CommandDefinition(
                    @"UPDATE dbo.Wallet
                      SET Balance = Balance - @Amt
                      OUTPUT inserted.Balance    -- <— số dư mới
                      WHERE UserId = @U;",
                    new { Amt = amount, U = fromUserId }, transaction: tx, cancellationToken: token));

                // 3) Cộng tiền đích và LẤY SỐ DƯ MỚI bằng OUTPUT
                var newToBalance = await conn.QuerySingleAsync<decimal>(new CommandDefinition(
                    @"UPDATE dbo.Wallet
                      SET Balance = Balance + @Amt
                      OUTPUT inserted.Balance
                      WHERE UserId = @U;",
                    new { Amt = amount, U = toUserId }, transaction: tx, cancellationToken: token));

                // 4) Trả về kết quả có ý nghĩa
                return new TransferResult
                {
                    FromUserId = fromUserId,
                    ToUserId = toUserId,
                    Amount = amount,
                    FromBalanceAfter = newFromBalance,
                    ToBalanceAfter = newToBalance
                };
            }, IsolationLevel.ReadCommitted, ct);
        }
    }
}
```
Trong câu lệnh `SELECT` ta sử dụng cú pháp `UPDLOCK` là table hint buộc câu lệnh nhận `“update lock” (U-lock)` thay vì `“shared lock” (S-lock)` khi đọc. Mục đích chính là `giảm deadlock` kiểu `“đọc rồi cập nhật” (read–then–update)`. Khi nhiều phiên cùng muốn cập nhật cùng một hàng, `UPDLOCK` đảm bảo chỉ một phiên lấy được `U-lock` trước, các phiên khác đợi, từ đó tránh tình huống 2 phiên đều đọc `S-lock` xong rồi cùng lúc chuyển `S→X (exclusive)` và cầm chân nhau.  

`UPDLOCK` sẽ giữ khóa cho đến khi nào `hết transaction` (nếu bạn đang ở trong `BEGIN TRAN … COMMIT`) chứ không phải là chỉ giữ khóa hết mỗi `Statement (cú pháp)` như `S-Lock` thông thường.  

Các kiểu tương thích trong loại `Lock` như sau:  
- `S (Shared) ↔ S`: tương thích (cùng đọc): Là các câu lệnh `SELECT thông thường`  
- `S ↔ U`: tương thích (đọc vẫn đọc được khi ai đó giữ U-lock): Là khi người khác đang giữ LOCK với câu lệnh `WITH UPDLOCK` thì người khác vẫn `SELECT (không đi kèm WITH UPDLOCK)` đọc giá trị  
- `U ↔ U`: không tương thích (chỉ một bên được quyền “chuẩn bị cập nhật”): Là 1 bên đang giữu LOCK, 1 bên khác SELECT WITH UPDLOCK tiếp thì ai nhanh hơn sẽ được, người sau bị chặn, phải đợi người trước xong đã.  
- `U ↔ X (Exclusive)`: không tương thích: Là trong lúc người nắm giữ LOCK đang thực hiện cập nhật, xóa dữ liệu thì không cho phép người khác nắm quyền LOCK  
- `X ↔ bất cứ thứ gì khác`: không tương thích.  

Ví dụ `Transaction A`:  
```SQL
BEGIN TRAN;

-- A1: đọc hàng để chuẩn bị cập nhật
SELECT * 
FROM dbo.Wallet WITH (UPDLOCK, ROWLOCK)
WHERE UserId = 1;

-- ... làm gì đó ...

-- A2: cập nhật chính hàng đó
UPDATE dbo.Wallet
SET Balance = Balance - 100
WHERE UserId = 1;

COMMIT;
```
Ở đây `Transaction A` sẽ nắm giữ khóa `LOCK` của hàng có `UsserID = 1` cho đến hết đoạn lệnh `COMMIT (Kết thúc 1 transaction)`.  Trong lúc đó nếu 1 phiên `B`:  
- `SELECT ... WHERE UserId=1 (không UPDLOCK)`: đọc được (S ↔ U tương thích).  
- `SELECT ... WITH (UPDLOCK) WHERE UserId=1`: bị chặn (U ↔ U không tương thích).  
- `UPDATE dbo.Wallet ... WHERE UserId=1`: bị chặn (cần X, X ↔ U không tương thích).  

Trong `Transaction A` khi đến bước `A2` là thực hiện cập nhật dữ liệu `X`thì `Transaction A` cần nâng `U → X`; nếu có `B đang giữ S-lock (đọc)`, `A` sẽ đợi tới khi `B trả S-lock` (thường rất ngắn, vì S-lock theo statement). Nếu B lúc này cũng muốn `chuyển sang X` (ví dụ B cũng `chuẩn bị update cùng hàng`) ⇒ có thể hình thành `deadlock` nếu hai bên giữ tài nguyên theo thứ tự khác nhau.  

>  UPDLOCK giảm khả năng deadlock “S→X conversion” khi nhiều phiên cùng cập nhật một hàng (vì không cho cả hai cùng giữ U-lock). Nhưng nó không loại bỏ mọi deadlock — ví dụ deadlock “thứ tự tài nguyên” (A giữ U-lock hàng 1 rồi đòi hàng 2; B giữ U-lock hàng 2 rồi đòi hàng 1)  

Ta có thể truy vấn `bỏ qua các hàng đang bị khóa` với cú pháp `READPAST`, `ném ra lỗi thay vì đợi hàng bị khóa` với cú pháp `NOWAIT`, `đặt thời gian chờ` với cú pháp `SET LOCK_TIMEOUT xxxx` để đợi, nếu quá thời gian sẽ ném ra lỗi `1222: Lock request timeout period exceeded`.  

Cuối cùng trong giao diện `Form` ta gọi như sau:  
```C#
// Trong Form1 (tiếp tục)
private readonly WalletService _wallet;

public Form1()
{
    InitializeComponent();
    var pipeline = SqlPolicies.CreatePipeline();
    _db = new DbExecutor(connString: null, pipeline: pipeline);
    _loginRepo = new LoginTable(_db);
    _wallet = new WalletService(_db);          // <— khởi tạo service chuyển tiền

    btnLoad.Click += btnLoad_Click;
    btnCancel.Click += btnCancel_Click;

    // GIẢ SỬ bạn kéo thêm nút "Transfer 100" đặt tên btnTransfer
    btnTransfer.Click += btnTransfer_Click;    // <— gắn sự kiện
}

private async void btnTransfer_Click(object? sender, EventArgs e)
{
    _cts?.Cancel();
    _cts = new CancellationTokenSource();

    try
    {
        btnTransfer.Enabled = false;
        statusLabel.Text = "Đang chuyển...";

        var result = await _wallet.TransferAsync(fromUserId: 1, toUserId: 2, amount: 100m, ct: _cts.Token);

        statusLabel.Text =
            $"OK: {result.Amount:0.##} từ {result.FromUserId}→{result.ToUserId} | " +
            $"Số dư mới: From={result.FromBalanceAfter:0.##}, To={result.ToBalanceAfter:0.##}";
    }
    catch (OperationCanceledException)
    {
        statusLabel.Text = "Đã hủy.";
    }
    catch (BrokenCircuitException)
    {
        statusLabel.Text = "DB bận (fail nhanh). Thử lại sau ít giây...";
    }
    catch (InvalidOperationException ex)
    {
        statusLabel.Text = "Lỗi nghiệp vụ: " + ex.Message;  // ví dụ “Số dư không đủ.”
    }
    catch (Exception ex)
    {
        statusLabel.Text = "Lỗi: " + ex.Message;
    }
    finally
    {
        btnTransfer.Enabled = true;
    }
}
```

Một `Form` mẫu như sau:  

```C#
using Polly.CircuitBreaker;
using System;                         // EventArgs, Exception
using System.Linq;                    // FirstOrDefault()
using System.Threading;               // CancellationTokenSource
using System.Threading.Tasks;         // Task
using System.Windows.Forms;           // WinForms Control
using Test.Database;                  // DbExecutor, SqlPolicies, LoginTable
using Test.Database.Table;
using Test.Utils;                  

namespace Test
{
    public partial class Form1 : Form
    {
        // ====== Trường (field) dùng trong Form ======

        private readonly DbExecutor _db;                // Khai báo biến thực thi truy vấn (Dapper + Polly v8)
        private readonly User_Login_Service _loginRepo;         // Repository cho bảng Users_Login
        private readonly Wallet_Service _wallet;         
        private CancellationTokenSource? _cts;          // Để hủy tác vụ async truy vấn CSDL khi bấm Cancel

        public Form1()
        {
            InitializeComponent();                      // Khởi tạo các control do Designer sinh ra

            // this.Icon = Properties.Resources.Quan_ico;


            // ====== Khởi tạo hạ tầng truy vấn ======
            // 1) Tạo pipeline Polly (Retry + Circuit Breaker)
            var pipeline = SqlPolicies.CreatePipeline();

            // 2) Tạo DbExecutor với pipeline trên
            _db = new DbExecutor(connString: null, pipeline: pipeline);

            // 3) Tạo repository cho bảng Users_Login
            _loginRepo = new User_Login_Service(_db);
            _wallet = new Wallet_Service(_db);

            // trạng thái ban đầu
            statusLabel.Text = "Sẵn sàng.";
        }

        // ========== Nút Load: đọc dữ liệu và hiển thị lên label ==========
        private async void btnLoad_Click(object? sender, EventArgs e)
        {
            // Hủy tác vụ trước (nếu còn) để không chồng chéo truy vấn
            _cts?.Cancel();

            // Tạo CTS mới cho lần bấm này
            _cts = new CancellationTokenSource();

            try
            {
                // Khoá nút Load trong khi đang tải để tránh bấm liên tục
                btnLoad.Enabled = false;
                statusLabel.Text = "Đang tải dữ liệu từ database...";

                // ====== Gọi repository ======
                // Trường hợp 1: Lấy tất cả (email = null) hoặc chỉ định cụ thể email muốn truy vấn
                var results = await _loginRepo.GetLoginAsync(email: null, ct: _cts.Token);

                // Tính số dòng (ToList() để không enumerate nhiều lần)
                var list = results.ToList();
                var count = list.Count;

                // Lấy dòng đầu (nếu có)
                var first = list.FirstOrDefault();

                // Cập nhật label với cú pháp rút gọn
                statusLabel.Text = first == null
                    ? "Không có dữ liệu."
                    : $"Tổng: {count} | First: {first.User_Name} <{first.Email}> | Active={first.Activate}";

                //Hoặc cập nhật label với cú pháp đầy đủ
                //if (first != null)
                //{
                //    // Hiển thị họ tên + email (tuỳ biến theo DTO của bạn)
                //    statusLabel.Text = $"Tổng: {count} | First: {first.User_Name} <{first.Email}> | Active={first.Activate}";
                //}
                //else
                //{
                //    statusLabel.Text = "Không có dữ liệu.";
                //}
            }
            catch (OperationCanceledException)
            {
                // Người dùng bấm Cancel: khi gọi hàm _cts.Cancel() là sẽ ném lỗi này
                statusLabel.Text = "Đã hủy thao tác.";
            }
            catch (BrokenCircuitException) // Circuit đang mở → fail nhanh
            {
                statusLabel.Text = "Dịch vụ DB tạm quá tải. Thử lại sau ít giây...";
            }
            catch (Exception ex)
            {
                // Lỗi khác: có thể là SQL syntax, mất kết nối lâu (breaker), v.v.
                statusLabel.Text = "Lỗi: " + ex.Message;
                // Ghi log lại
                LogEx.Error(ex.Message, "Lỗi khi tải Users_Login");
            }
            finally
            {
                // Mở lại nút Load cho lần sau
                btnLoad.Enabled = true;
            }
        }

        // ========== Nút Cancel: yêu cầu hủy tác vụ truy vấn DB đang chạy ==========
        private void btnCancel_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();                  // Yêu cầu hủy
            statusLabel.Text = "Đang gửi yêu cầu hủy...";
        }

        private async void btnTransfer_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                btnTransfer.Enabled = false;
                statusLabel.Text = "Đang chuyển...";

                var result = await _wallet.TransferAsync(fromUserId: 1, toUserId: 2, amount: 100m, ct: _cts.Token);

                statusLabel.Text =
                    $"OK: {result.Amount:0.##} từ {result.FromUserId}→{result.ToUserId} | " +
                    $"Số dư mới: From={result.FromBalanceAfter:0.##}, To={result.ToBalanceAfter:0.##}";
            }
            catch (OperationCanceledException)
            {
                statusLabel.Text = "Đã hủy.";
            }
            catch (BrokenCircuitException)
            {
                statusLabel.Text = "DB bận (fail nhanh). Thử lại sau ít giây...";
            }
            catch (InvalidOperationException ex)
            {
                statusLabel.Text = "Lỗi nghiệp vụ: " + ex.Message;  // ví dụ “Số dư không đủ.”
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                btnTransfer.Enabled = true;
            }
        }
    }
}
```
