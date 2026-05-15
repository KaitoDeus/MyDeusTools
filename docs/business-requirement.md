# Business Requirement Document: MyDeusTools

## 1. Tổng quan dự án (Project Overview)

**MyDeusTools** là một hệ sinh thái công cụ tiện ích (Super-App) dành cho máy tính để bàn, được thiết kế để hợp nhất các công cụ nhỏ lẻ (micro-tools) vào một giao diện duy nhất. Thay vì phải mở nhiều ứng dụng rời rạc, người dùng chỉ cần một "điểm chạm" duy nhất để quản lý toàn bộ năng suất làm việc của mình.

**Tầm nhìn:** Trở thành "bộ não" điều khiển mọi tác vụ phụ trợ trên máy tính, có khả năng mở rộng không giới hạn thông qua các module.

## 2. Mục tiêu dự án (Project Goals & Metrics)

Để đảm bảo tính hiệu quả và chuyên nghiệp, dự án đặt ra các chỉ số mục tiêu cụ thể:

- **Hiệu năng (Performance):**
  - Thời gian khởi động ứng dụng (Startup Time): **< 1.5 giây**.
  - Chiếm dụng tài nguyên RAM ở chế độ nền: **< 50MB**.
- **Tính linh hoạt (Flexibility):** Hỗ trợ kiến trúc plug-and-play cho ít nhất **10 công cụ** khác nhau trong tương lai.
- **Trải nghiệm người dùng (UX):**
  - Tối đa **2 click** để truy cập bất kỳ công cụ nào từ màn hình Dashboard.
  - Hỗ trợ phím tắt (Hotkeys) cho toàn bộ các tính năng chính.

## 3. Yêu cầu tính năng (Functional Requirements)

### 3.1. Dashboard trung tâm (Central Hub)

- **Quản lý danh sách công cụ:** Hiển thị dưới dạng Grid hoặc Sidebar với icon và tên công cụ.
- **Trạng thái hoạt động:** Hiển thị công cụ nào đang chạy ngầm hoặc đang được kích hoạt.
- **Tìm kiếm nhanh:** Ô tìm kiếm để lọc nhanh công cụ cần thiết.

### 3.2. Nhóm công cụ giai đoạn 1 (MVP Tools)

1.  **AutoClicker Pro (Advanced Mouse Automation):**
    - **Click Configuration:**
      - **Button Type:** Hỗ trợ Chuột Trái (Left), Phải (Right), và Giữa (Middle). [Hoàn thành]
      - **Click Mode:** Tùy chọn Click đơn (Single) hoặc Click đúp (Double). [Hoàn thành]
    - **Sequence & Multiple Click:**
      - **Interval Management:** Thiết lập khoảng trễ linh hoạt (Giờ, Phút, Giây, Mili giây). [Hoàn thành]
      - **Recording Engine:** Khả năng ghi lại (Record) một chuỗi các tọa độ chuột thông qua Click trực tiếp và phát lại (Replay) theo trình tự. [Hoàn thành]
    - **Repeat & Termination Settings:**
      - **Loop Mode:** Chạy vô hạn hoặc lặp lại theo số lần chỉ định.
      - **Schedule Execution:** Tự động dừng sau một khoảng thời gian nhất định.
    - **Control & UX:**
      - **Global Hotkeys:** Người dùng tự định nghĩa phím tắt để Bật/Tắt (Hỗ trợ phím tổ hợp). [Hoàn thành]
      - **Visual Feedback:** Hiển thị tọa độ trực tiếp qua Overlay khi đang ghi và số lần đã click thực tế. [Hoàn thành]
      - **Modernized UI:** Giao diện dạng form chuyển đổi bằng nút bấm (Button-based navigation), bố cục tối giản, dàn hàng ngang cho các thiết lập thời gian để tối ưu không gian. [Hoàn thành]
    - **Window Management:**
      - **Fixed Resolution:** Cửa sổ ứng dụng được khóa ở độ phân giải 1280x720. [Hoàn thành]
      - **Lock Resize:** Vô hiệu hóa tính năng thay đổi kích thước và phóng to (Maximize) để đảm bảo tính ổn định của layout. [Hoàn thành]

2.  **Schedule Shutdown (Hẹn giờ hệ thống):**
    - Hỗ trợ: Tắt máy (Shutdown), Khởi động lại (Restart), Ngủ đông (Hibernate).
    - Hẹn giờ theo: Đếm ngược (Countdown) hoặc Thời điểm cụ thể trong ngày (Specific time).
3.  **Sticky Note (Ghi chú nhanh):**
    - Hỗ trợ định dạng văn bản cơ bản. [Hoàn thành]
    - Tính năng "Always on top" để ghim ghi chú lên màn hình. [Hoàn thành]
    - Tự động lưu (Auto-save) dữ liệu ngay khi nhập. [Hoàn thành]

4. **Alarm Clock (Đồng hồ báo thức):**
    - **Alarms:** Thiết lập và quản lý nhiều mốc báo thức với âm báo tùy chỉnh.
    - **Timer:** Đồng hồ đếm ngược linh hoạt (Bắt đầu, Tạm dừng, Reset).
    - **Stopwatch:** Bấm giờ chính xác, hỗ trợ ghi lại số vòng (Lap recording).
    - **World Clock:** Hiển thị giờ hiện tại tại các múi giờ khác nhau trên thế giới.

5. **Screen Recorder (Quay màn hình):**
    - **Capture Options:** Quay toàn màn hình, cửa sổ ứng dụng hoặc khu vực chỉ định.
    - **Audio Sources:** Thu âm từ hệ thống (System Sound), Micro hoặc cả hai.
    - **Overlay & Effects:** Chèn Webcam overlay, hiển thị con trỏ chuột và hiệu ứng click.
    - **Drawing Tools:** Công cụ vẽ và chú thích trực tiếp lên màn hình trong khi quay.
    - **Quality Settings:** Tùy chỉnh độ phân giải, khung hình (FPS) và định dạng đầu ra.


### 3.3. Tính năng hệ thống (System Features)

- **System Tray Integration:** Thu nhỏ ứng dụng xuống thanh Taskbar để tiết kiệm không gian.
- **Auto-start with Windows:** Tùy chọn khởi động cùng hệ thống.
- **Theme Management:** Hỗ trợ giao diện Sáng/Tối (Light/Dark Mode) đồng bộ với hệ điều hành.

## 4. Yêu cầu phi chức năng (Non-functional Requirements)

- **Tính di động (Portability):** Ưu tiên phiên bản Portable (không cần cài đặt).
- **Độ tin cậy:** Không gây xung đột với các tiến trình hệ thống khác khi thực hiện Shutdown hoặc Auto-click.
- **Khả năng mở rộng:** Mã nguồn được tổ chức sao cho việc thêm một công cụ mới không làm ảnh hưởng đến code của các công cụ cũ.
