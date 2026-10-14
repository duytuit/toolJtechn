# DesignLabel - Tài liệu tính năng

## 1. Tổng quan

`DesignLabel` là ứng dụng Windows Forms dùng để thiết kế, lưu, tải và in tem nhãn. Người dùng thao tác trực tiếp trên vùng canvas có khung khổ giấy, thước đo, lưới và thanh công cụ riêng.

Ứng dụng khởi chạy từ `DesignLabel/Program.cs` và màn hình chính được triển khai trong `DesignLabel/Form1.cs`.

## 2. Giao diện chính

Giao diện gồm hai khu vực:

- **Vùng thiết kế:** chiếm phần lớn cửa sổ, có canvas cuộn, lưới, khung khổ giấy nét đứt và thước ngang/dọc.
- **Bảng điều khiển:** nằm bên phải, chứa các nút thao tác và thông số thiết kế. Bảng có thanh cuộn khi chiều cao cửa sổ không đủ.

Cửa sổ được mở ở trạng thái tối đa để có nhiều không gian vẽ. Nút **Vừa khổ** tự tính mức zoom để nhìn trọn khổ giấy trong vùng thiết kế.

## 3. Khổ giấy

Có thể chọn một trong các khổ giấy:

- **A4:** 210 x 297 mm.
- **A5:** 148 x 210 mm.
- **Letter:** 215.9 x 279.4 mm.
- **Tùy chỉnh:** nhập chiều rộng, chiều cao theo mm và DPI.

Kích thước hiển thị được quy đổi theo công thức:

```text
pixel = millimeter * DPI / 25.4
```

Khổ giấy được vẽ bằng đường nét đứt màu xanh. Các phần tử có thể được đặt trong vùng canvas theo tọa độ pixel sau khi áp dụng zoom.

## 4. Zoom và canvas

Các nút zoom gồm:

- `-`: giảm zoom 10%.
- `+`: tăng zoom 10%.
- **Vừa khổ:** tự căn canvas theo vùng làm việc.

Zoom được giới hạn từ 50% đến 200%. Khi zoom, vị trí, kích thước, cỡ chữ, đường vẽ và độ dày đường được co giãn đồng bộ.

Canvas hỗ trợ:

- Cuộn dọc bằng con lăn chuột.
- Cuộn ngang/dọc bằng thanh scrollbar khi nội dung lớn hơn vùng hiển thị.
- Ruler cập nhật theo vị trí cuộn hiện tại.
- Lưới cập nhật theo mức zoom.

## 5. Thêm phần tử

Nhấp chuột phải tại vị trí cần đặt phần tử trong canvas để mở menu:

- **Thêm Text:** thêm nhãn văn bản mặc định `{ProductName}`.
- **Thêm Hình ảnh:** chọn file ảnh PNG, JPG, JPEG hoặc BMP.
- **Thêm Barcode:** tạo mã vạch Code 128.
- **Thêm QR Code:** tạo mã QR.

Vị trí thêm phần tử được quy đổi từ tọa độ màn hình sang tọa độ canvas, vì vậy vẫn giữ đúng vị trí khi canvas đang được cuộn.

## 6. Chọn, kéo và thay đổi kích thước

### Chọn phần tử

- Nhấp trái vào phần tử để chọn.
- Kéo chuột trái trên vùng trống để tạo vùng chọn nhiều phần tử.
- Phần tử được chọn hiển thị viền nét đứt màu xanh.
- Đường được chọn hiển thị hai điểm điều khiển ở hai đầu.

### Di chuyển

Kéo phần tử đã chọn để di chuyển. Khi chọn nhiều phần tử, các phần tử được di chuyển cùng nhau.

### Thay đổi kích thước

Kéo góc dưới bên phải của phần tử. Với barcode và QR Code, hình ảnh mã được tạo lại theo kích thước mới.

### Đường thẳng

Bật **Vẽ đoạn thẳng**, sau đó kéo chuột trái trên canvas để tạo đường. Có thể thay đổi độ dày từ 1 đến 10 trước khi vẽ.

- Kéo thân đường để di chuyển.
- Kéo điểm đầu hoặc điểm cuối để chỉnh sửa.
- Nhấp chuột phải vào đường để xóa.

## 7. Chỉnh sửa phần tử

Nhấp chuột phải vào phần tử để mở menu:

- **Xóa:** xóa phần tử khỏi canvas.
- **Chỉnh sửa nội dung:** mở hộp thoại tương ứng.

### Text

Có thể chỉnh:

- Nội dung văn bản.
- Cỡ chữ.
- In đậm.
- In nghiêng.
- Chữ dọc từ trên xuống.
- Đánh dấu là trường cần thay thế khi xử lý dữ liệu.

### Barcode

Có thể chỉnh nội dung mã và tùy chọn `PureBarcode` để ẩn/hiện phần chữ bên dưới mã.

### QR Code

Có thể chỉnh nội dung được mã hóa trong QR Code.

## 8. Lưu và tải bố cục

### Lưu bố cục

Nút **Lưu bố cục** ghi dữ liệu ra file JSON, bao gồm:

- Loại phần tử.
- Nội dung.
- Tọa độ X/Y.
- Chiều rộng và chiều cao.
- Cỡ chữ và kiểu chữ.
- Hướng chữ.
- Thông tin barcode/QR.
- Danh sách đường thẳng.
- Khổ giấy, kích thước tùy chỉnh và DPI.

Tọa độ và kích thước được quy đổi về kích thước chưa zoom trước khi lưu, giúp bố cục không phụ thuộc mức zoom hiện tại.

### Tải bố cục

Nút **Tải bố cục** đọc file JSON và khôi phục:

- Khổ giấy.
- Các phần tử trên canvas.
- Kích thước và vị trí phần tử.
- Nội dung barcode/QR.
- Các đường thẳng.
- Trạng thái chữ dọc, đậm, nghiêng.

Khi tải, trạng thái chọn cũ được xóa, canvas cập nhật lại vùng cuộn và viewport trở về góc đầu để tránh lệch tọa độ.

## 9. In tem

Nút **In tem** mở cửa sổ xem trước khi in. Kích thước trang in được lấy theo khổ giấy đang chọn, lề được đặt về 0.

Khi in:

- Text được vẽ theo đúng vị trí và cỡ chữ.
- Hình ảnh, barcode và QR Code được in theo kích thước thiết kế.
- Đường thẳng được in cùng bố cục.
- Viền chọn và lưới không được in.

## 10. Quy ước tọa độ

Ứng dụng sử dụng hai hệ tọa độ:

1. **Tọa độ canvas:** vị trí thực của phần tử trong khổ giấy.
2. **Tọa độ viewport:** vị trí hiển thị sau khi áp dụng scrollbar.

Mọi thao tác chuột trên canvas phải chuyển từ tọa độ viewport sang tọa độ canvas thông qua `ToCanvasPoint`. Khi vẽ nền, lưới, khung giấy và vùng chọn, offset scrollbar được áp dụng để khớp với các control con trên canvas.

## 11. Cấu trúc mã chính

```text
DesignLabel/
├── Form1.cs              # Giao diện, canvas và toàn bộ logic thiết kế
├── Form1.Designer.cs     # Thành phần WinForms được sinh bởi Designer
├── Program.cs            # Điểm khởi chạy ứng dụng
├── Conditional.cs        # Chức năng điều kiện/dữ liệu liên quan
├── ImportExcel.cs        # Nhập dữ liệu Excel
└── DesignLabel.csproj    # Cấu hình project và package
```

Các lớp dữ liệu bố cục được khai báo trong `Form1.cs`:

- `LineObject`: mô tả đường thẳng.
- `ElementData`: dữ liệu nội bộ của phần tử.
- `LayoutItem`: một phần tử trong file JSON.
- `LayoutData`: toàn bộ bố cục.

## 12. Thư viện và môi trường

- .NET Framework 4.7.2.
- Windows Forms.
- `Newtonsoft.Json 13.0.3` để đọc/ghi JSON.
- `ZXing.Net 0.16.8` để tạo barcode và QR Code.
- `ClosedXML 0.105.1` cho chức năng Excel.
- `PDFsharp 6.2.0` cho chức năng PDF liên quan.
- `QRCoder 1.6.0` được khai báo trong project.

## 13. Chạy project

Mở solution `toolJtechn.sln` bằng Visual Studio, khôi phục NuGet package, chọn project `DesignLabel` và chạy.

Có thể build project bằng:

```powershell
dotnet restore DesignLabel\DesignLabel.csproj
dotnet build DesignLabel\DesignLabel.csproj --configuration Debug
```

Project yêu cầu môi trường Windows vì sử dụng Windows Forms và .NET Framework.

## 14. Lưu ý sử dụng

- Nên chọn khổ giấy và DPI trước khi đặt phần tử.
- Dùng **Vừa khổ** trước khi bố trí để nhìn toàn bộ khổ.
- Khi tải bố cục cũ, kiểm tra lại khổ giấy và DPI nếu file được tạo từ phiên bản khác.
- File ảnh được nạp trực tiếp từ đường dẫn người dùng chọn; cần giữ file ảnh khi mở lại bố cục nếu muốn tiếp tục sử dụng ảnh đó.
- File JSON nên được lưu ở thư mục có quyền ghi.
