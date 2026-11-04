# Chạy host tối thiểu

Host này chỉ mở TCP, trả khóa bắt tay `-27`, và ghi log kết nối/gói. Nó chưa đăng nhập, chưa có nhân vật, DB hay dữ liệu tĩnh.

Từ thư mục `nro-csharp`, build và chạy:

```powershell
dotnet run --project src/Nro.Host -- --port 14445
```

Có thể giới hạn địa chỉ lắng nghe, ví dụ `--bind 127.0.0.1`. Không truyền `--bind` thì host nghe mọi giao diện. Nhấn Ctrl+C để dừng gọn gàng.

Khi host đang chạy, từ thư mục gốc chạy kiểm tra khói:

```powershell
python -I port-csharp/tools/smoke_handshake.py
```

Kết quả đúng in khóa `NguyenDucVuEntertainment` (24 byte) và `BAT TAY DAT`.

## Trỏ client Unity

Bản PC ưu tiên là `client-mod/Mod_PC_Online/PC_srcnrofree.online/DragonBoy.exe`. Mã Unity tương ứng nằm ở `client-mod/Project_Client_Mod_Online/PRJ_2Tab_550K`.

Đổi giá trị `Management.IpServer` trong `Assets/Scripts/Assembly-CSharp/Management.cs` thành đúng chuỗi sau rồi build lại project Unity:

```csharp
public static string IpServer = "Local:127.0.0.1:14445:0,0,0";
```

Giá trị này được `Game1/ServerListScreen.cs` (và bản sao `Game2/ServerListScreen.cs`) đọc làm `linkDefault`, sau đó tách thành tên, IP và cổng; `Game1/GameCanvas.cs` gán chúng vào `GameMidlet.IP` và `GameMidlet.PORT`. Chuỗi ở trên đã là giá trị local hiện có, nên không cần sửa nếu chưa bị thay đổi.

Không tìm thấy tệp cấu hình văn bản riêng trong bản PC đã biên dịch; nó chứa `GameAssembly.dll`, vì vậy địa chỉ runtime của bản PC cần được xác nhận bằng cách build từ project Unity trên hoặc kiểm tra source/binary riêng của bản phát hành. Khi chạy client, log Host phải có `Phiên mở` và dòng `nhận cmd -27 (GET_SESSION_ID) bắt tay`; sau đó client sẽ không đi xa hơn vì host cố ý chưa xử lý đăng nhập.

Client có thể thử kết nối nhiều lần. `GameServerOptions` giới hạn mặc định 3 kết nối đồng thời trên mỗi IP, nên hãy đóng các lần thử cũ trước khi thử lại nhiều lần.
