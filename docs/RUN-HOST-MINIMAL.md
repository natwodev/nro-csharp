# Chạy Nro.Host Generic Host

`Nro.Host` nạp static catalog trước rồi mới mở TCP. Nếu catalog hoặc cấu hình database lỗi, Host dừng fail-fast và không bind cổng. Listener vẫn dùng `LoggingMessageHandler` và `LoggingAcceptHandler` cho đến J-03A, nên chưa có login/nhân vật hoàn chỉnh.

## Cấu hình

Các giá trị không bí mật nằm ở `src/Nro.Host/appsettings.json`:

- `GameServer`: `Port`, `BindAddress`, giới hạn mỗi IP, socket buffer, queue và timeout phiên.
- `Startup:LoadStaticCatalogs`: mặc định `true`; chỉ đặt `false` để chạy thử khi không có DB.
- `Startup:ShutdownSaveTimeout`: hạn lưu người chơi lúc Ctrl+C, mặc định 30 giây.
- `Scheduling:AutoSaveInterval`: mặc định 5 phút; dịch vụ auto-save sẽ được nối ở gói sau.
- `Persistence:ConnectionName`: mặc định `Nro`.
- `Assets:Root`: vị trí dành cho J-06, hiện chưa có handler asset.

Không đặt chuỗi kết nối, mật khẩu hay token trong `appsettings.json`. Đặt chuỗi kết nối bằng biến môi trường (PowerShell) trước khi chạy:

```powershell
$env:ConnectionStrings__Nro = 'Host=127.0.0.1;Port=5432;Database=nro;Username=nro;Password=<secret>'
dotnet run --project src/Nro.Host -- --port 14445 --bind 127.0.0.1
```

`--port` và `--bind` là cấu hình ghi đè command line cho `GameServer:Port` và `GameServer:BindAddress`. Có thể thay biến môi trường bằng user-secrets/secret store của môi trường triển khai; không ghi bí mật vào repository.

## Chạy thử không DB

Chỉ để kiểm tra listener, dùng:

```powershell
$env:Startup__LoadStaticCatalogs = 'false'
dotnet run --project src/Nro.Host -- --port 14445 --bind 127.0.0.1
```

Host sẽ ghi cảnh báo rõ ràng. Chế độ này không xác thực dữ liệu tĩnh và không được dùng cho máy chủ công khai. Nhấn Ctrl+C: Host dừng accept, gọi cổng lưu người chơi (hiện là cài đặt rỗng có log cho đến khi registry người chơi được nối), rồi đóng listener.

Nếu console cũ vẫn vỡ chữ tiếng Việt, dùng Windows Terminal hoặc chạy `chcp 65001`.

## Trỏ client Unity

Bản PC ưu tiên là `client-mod/Mod_PC_Online/PC_srcnrofree.online/DragonBoy.exe`. Mã Unity tương ứng nằm ở `client-mod/Project_Client_Mod_Online/PRJ_2Tab_550K`.

Đổi `Management.IpServer` trong `Assets/Scripts/Assembly-CSharp/Management.cs` thành:

```csharp
public static string IpServer = "Local:127.0.0.1:14445:0,0,0";
```

Khi host đang chạy, `python -I port-csharp/tools/smoke_handshake.py` vẫn chỉ xác minh bắt tay `-27`; sau đó client chưa đi tiếp được vì đăng nhập/dispatcher thật thuộc J-03A/J-04.
