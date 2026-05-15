# Kế hoạch & Tài liệu Kiểm thử (Testing Document) - MyDeusTools

Tài liệu này xác định các cấp độ kiểm thử và các kịch bản kiểm thử (Test Cases) để đảm bảo chất lượng cho siêu ứng dụng MyDeusTools.

---

## 1. Unit Testing (Kiểm thử đơn vị)

**Mục tiêu:** Kiểm tra các hàm logic độc lập trong các `Services`.

| Thành phần            | Kịch bản kiểm thử                | Kết quả mong đợi                                             |
| :-------------------- | :------------------------------- | :----------------------------------------------------------- |
| **SystemService**     | Truyền vào 3600 giây để hẹn giờ. | Gọi `shutdown.exe` với tham số `/s /t 3600`.                 |
| **SystemService**     | Gọi lệnh hủy hẹn giờ.            | Gọi `shutdown.exe` với tham số `/a`.                         |
| **AutoClickService**  | Start với interval = 100ms.      | Timer chạy và kích hoạt sự kiện mỗi 100ms.                   |
| **StickyNoteService** | Lưu một danh sách 3 ghi chú.     | File `notes.json` được tạo ra và chứa đúng 3 đối tượng JSON. |
| **StickyNoteService** | Đọc file JSON bị lỗi định dạng.  | Ứng dụng không crash, khởi tạo danh sách trống.              |

---

## 2. Integration Testing (Kiểm thử tích hợp)

**Mục tiêu:** Kiểm tra sự tương tác giữa ViewModel, Service và DI Container.

- **DI Registration:** Đảm bảo toàn bộ ViewModels và Services được đăng ký đúng trong `App.xaml.cs`. Nếu thiếu, ứng dụng sẽ báo lỗi ngay khi khởi động.
- **Navigation Flow:** Kiểm tra khi nhấn vào Menu "Auto Clicker", `NavigationService` phải khởi tạo đúng `AutoClickPage` và Inject đúng `AutoClickViewModel`.
- **Data Binding:** Thay đổi giá trị `Interval` trên UI của AutoClicker phải cập nhật ngay lập tức vào biến tương ứng trong ViewModel.

---

## 3. System Testing (Kiểm thử hệ thống)

**Mục tiêu:** Kiểm tra toàn bộ ứng dụng trên môi trường Windows thực tế.

- **Hiệu năng:** RAM chiếm dụng khi chạy ngầm phải < 50MB (như yêu cầu trong BRD).
- **Độ ổn định:** Chạy Auto Clicker liên tục trong 10 phút với tốc độ cao (10ms) xem có gây treo máy hoặc rò rỉ bộ nhớ (Memory Leak) không.
- **Tính di động (Portable):** Di chuyển folder ứng dụng sang vị trí khác (ổ đĩa khác) và chạy thử xem có mất dữ liệu không.

---

## 4. User Acceptance Testing (UAT) & Test Cases

**Mục tiêu:** Các kịch bản người dùng thực tế.

### Nhóm TC 01: Sticky Note (Ghi chú)

- **TC-01.1:** Người dùng gõ nội dung vào Note 1, sau đó tắt app bằng Alt+F4. Mở lại app, nội dung Note 1 phải còn nguyên.
- **TC-01.2:** Nhấn nút "Xóa" ghi chú, file `notes.json` phải được cập nhật dung lượng nhỏ đi.
- **TC-01.3:** Nhấn phím Enter khi đang gõ ghi chú, kiểm tra xem dữ liệu có được lưu ngay không (Auto-save trigger).

### Nhóm TC 02: Auto Clicker Pro

- **TC-02.1:** Nhấn phím tắt F6 khi đang ở ngoài màn hình Desktop. Clicker phải bắt đầu hoạt động.
- **TC-02.2:** Thiết lập số lần click = 10. Clicker phải tự dừng sau đúng 10 lần click.
- **TC-02.3:** Kiểm tra Double Click: Thử nghiệm trên một icon thư mục, xem nó có mở thư mục đó ra không.

### Nhóm TC 03: Schedule Shutdown

- **TC-03.1:** Hẹn giờ tắt máy sau 1 phút. Kiểm tra xem Windows có hiện thông báo "You are about to be signed out" không.
- **TC-03.2:** Nhấn nút "Hủy lệnh", thông báo của Windows phải biến mất.

### Nhóm TC 04: UI & Window Management

- **TC-04.1:** Kiểm tra chuyển Form bằng Button: Click "Khoảng cách & Lặp lại", giao diện phải đổi sang form cấu hình thời gian nghỉ ngay lập tức.
- **TC-04.2:** Kiểm tra khóa độ phân giải: Thử kéo mép cửa sổ hoặc double-click thanh tiêu đề, cửa sổ không được phép thay đổi kích thước.
- **TC-04.3:** Kiểm tra ẩn nút Maximize: Thanh tiêu đề chỉ được hiển thị nút Minimize và Close.

---

## 5. Checklist sau kiểm thử (Exit Criteria)

- [x] 100% các Unit Test logic core vượt qua. (Đã đạt: 17/17 tests passed)
- [x] Không có lỗi nghiêm trọng (Critical Bug) gây crash ứng dụng. (Đã xác nhận qua Integration Tests)
- [x] Giao diện hiển thị đúng trên cả Light và Dark mode. (Passed)
- [x] File dữ liệu được lưu đúng chỗ và không bị lỗi font tiếng Việt. (Đã chuyển sang folder local/Data)

---

## 6. Nhật ký kết quả (Test Logs)

_Ngày cập nhật: 15/05/2026_

- **Tổng số bài test:** 20 (Thêm 3 test UI & Window)
- **Vượt qua (Passed):** 20
- **Thất bại (Failed):** 0
- **Trạng thái hệ thống:** Ổn định. Giao diện AutoClicker đã được đại tu, cửa sổ được bảo mật kích thước, các chức năng core vẫn hoạt động chính xác.
