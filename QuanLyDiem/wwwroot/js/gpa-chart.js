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
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: [
                    '#28a745', // A - Xanh lá
                    '#007bff', // B - Xanh dương
                    '#17a2b8', // C - Xanh ngọc
                    '#ffc107', // D - Vàng
                    '#dc3545'  // F - Đỏ
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