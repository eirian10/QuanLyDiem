// wwwroot/js/gpa-chart.js

function initGpaPieChart(labels, data) {
    const ctx = document.getElementById('letterPieChart').getContext('2d');

    const existingChart = Chart.getChart(ctx.canvas);
    if (existingChart) {
        existingChart.destroy();
    }

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels, // Nhận mảng động ["A+", "A", "B+", "B", "C", "D", "F"] từ Service đổ ra
            datasets: [{
                data: data,
                // Cập nhật hệ thống 7 dải màu phân cấp chuyên nghiệp cho biểu đồ hình tròn
                backgroundColor: [
                    '#218838', // A+ - Xanh lá đậm (Xuất sắc)
                    '#28a745', // A  - Xanh lá chuẩn (Giỏi)
                    '#0069d9', // B+ - Xanh dương đậm (Khá giỏi)
                    '#007bff', // B  - Xanh dương chuẩn (Khá)
                    '#17a2b8', // C  - Xanh ngọc (Trung bình khá)
                    '#ffc107', // D  - Vàng (Trung bình)
                    '#dc3545'  // F  - Đỏ (Học lại / Yếu)
                ]
            }]
        },
        options: {
            responsive: true,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const rawValue = context.raw;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            // Nếu tổng số sinh viên bằng 0 (lớp trống), tỷ lệ % mặc định là 0
                            const percentage = total > 0 ? ((rawValue / total) * 100).toFixed(1) : 0;
                            return ` Điểm ${context.label}: ${rawValue} SV (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}