/**
 * Xử lý tự động kiểm tra dữ liệu đầu vào và làm tròn điểm số về 1 chữ số thập phân
 */
$(document).ready(function () {
    // Lắng nghe sự kiện thay đổi dữ liệu trên tất cả các ô nhập điểm quá trình và cuối kỳ
    $('input[name$=".ProcessScore"], input[name$=".FinalScore"]').on('change blur', function () {
        let value = $(this).val();

        // Chỉ xử lý nếu ô nhập điểm không trống
        if (value !== "") {
            let num = parseFloat(value);

            if (!isNaN(num)) {
                // Ràng buộc giới hạn dữ liệu điểm từ 0.0 đến 10.0
                if (num < 0) num = 0;
                if (num > 10) num = 10;

                // Làm tròn chuẩn xác 1 chữ số thập phân (Ví dụ: 8.26 -> 8.3)
                let roundedValue = num.toFixed(1);

                // Cập nhật ngược lại giá trị sạch vào ô nhập liệu
                $(this).val(roundedValue);
            }
        }
    });
});