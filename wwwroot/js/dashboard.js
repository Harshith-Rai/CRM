function initDashboardCharts(growthKeys, growthValues, industryKeys, industryValues) {
    // --- 1. Enhanced Growth Line Chart Configuration ---
    const growthCanvas = document.getElementById('growthChart');
    if (growthCanvas) {
        const growthCtx = growthCanvas.getContext('2d');

        // Create a more vibrant gradient
        const gradient = growthCtx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, 'rgba(0, 102, 204, 0.3)');
        gradient.addColorStop(0.5, 'rgba(0, 102, 204, 0.15)');
        gradient.addColorStop(1, 'rgba(0, 102, 204, 0.0)');

        new Chart(growthCtx, {
            type: 'line',
            data: {
                labels: growthKeys,
                datasets: [{
                    label: 'New Customers',
                    data: growthValues,
                    borderColor: '#0066CC',
                    backgroundColor: gradient,
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointRadius: 6,
                    pointBackgroundColor: '#ffffff',
                    pointBorderColor: '#0066CC',
                    pointBorderWidth: 3,
                    pointHoverRadius: 8,
                    pointHoverBorderWidth: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#0F172A',
                        padding: 14,
                        cornerRadius: 10,
                        displayColors: false,
                        titleFont: { size: 13, weight: '700', family: "'Plus Jakarta Sans', sans-serif" },
                        bodyFont: { size: 16, weight: '700', family: "'Plus Jakarta Sans', sans-serif" },
                        titleColor: '#94A3B8',
                        bodyColor: '#ffffff',
                        caretPadding: 10
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: '#f1f5f9',
                            borderDash: [5, 5],
                            drawBorder: false
                        },
                        ticks: {
                            stepSize: 1,
                            font: { size: 12, weight: '600' },
                            color: '#64748B',
                            padding: 10
                        },
                        border: { display: false }
                    },
                    x: {
                        grid: { display: false },
                        ticks: {
                            font: { size: 12, weight: '600' },
                            color: '#64748B',
                            padding: 10
                        },
                        border: { display: false }
                    }
                }
            }
        });
    }

    // --- 2. Enhanced Industry Doughnut Chart ---
    const industryCanvas = document.getElementById('industryChart');
    if (industryCanvas) {
        const industryCtx = industryCanvas.getContext('2d');
        new Chart(industryCtx, {
            type: 'doughnut',
            data: {
                labels: industryKeys,
                datasets: [{
                    data: industryValues,
                    backgroundColor: [
                        '#0066CC',
                        '#00B4D8',
                        '#10B981',
                        '#F59E0B',
                        '#EF4444',
                        '#8B5CF6',
                        '#EC4899',
                        '#6366F1'
                    ],
                    borderWidth: 4,
                    borderColor: '#ffffff',
                    hoverOffset: 8,
                    hoverBorderWidth: 5
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            padding: 16,
                            font: {
                                size: 12,
                                family: "'DM Sans', sans-serif",
                                weight: '600'
                            },
                            color: '#475569',
                            generateLabels: function (chart) {
                                const data = chart.data;
                                if (data.labels.length && data.datasets.length) {
                                    return data.labels.map((label, i) => {
                                        const value = data.datasets[0].data[i];
                                        const total = data.datasets[0].data.reduce((a, b) => a + b, 0);
                                        const percentage = ((value / total) * 100).toFixed(1);
                                        return {
                                            text: `${label} (${percentage}%)`,
                                            fillStyle: data.datasets[0].backgroundColor[i],
                                            hidden: false,
                                            index: i
                                        };
                                    });
                                }
                                return [];
                            }
                        }
                    },
                    tooltip: {
                        backgroundColor: '#0F172A',
                        padding: 12,
                        cornerRadius: 8,
                        displayColors: true,
                        titleFont: { size: 13, weight: '700' },
                        bodyFont: { size: 14, weight: '700' },
                        callbacks: {
                            label: function (context) {
                                const label = context.label || '';
                                const value = context.parsed;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }
}