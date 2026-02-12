function initDashboardCharts(growthKeys, growthValues, industryKeys, industryValues) {

    // --- 1. Customer Acquisition Trend (Area Chart) ---
    const growthCanvas = document.getElementById('growthChart');
    if (growthCanvas) {
        const ctx = growthCanvas.getContext('2d');

        // Create a rich gradient (Blue fading to transparent)
        let gradient = ctx.createLinearGradient(0, 0, 0, 300);
        gradient.addColorStop(0, "rgba(14, 165, 233, 0.4)"); // Sky Blue
        gradient.addColorStop(1, "rgba(14, 165, 233, 0.0)"); // Transparent

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: growthKeys,
                datasets: [{
                    label: 'New Customers',
                    data: growthValues,
                    borderColor: '#0284C7', // Solid Blue
                    borderWidth: 3,
                    backgroundColor: gradient,
                    fill: true, // Fills the area (makes it look less empty)
                    tension: 0.4, // Smooth curves
                    pointRadius: 5,
                    pointBackgroundColor: '#FFFFFF',
                    pointBorderColor: '#0284C7',
                    pointBorderWidth: 2,
                    pointHoverRadius: 7
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#1E293B',
                        padding: 12,
                        titleFont: { size: 13 },
                        bodyFont: { size: 14, weight: 'bold' },
                        displayColors: false,
                        callbacks: {
                            label: (context) => ` ${context.parsed.y} Customers Acquired`
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        suggestedMax: 5, // Forces the chart to have height even if data is 0 or 1
                        grid: { borderDash: [5, 5], color: '#E2E8F0' },
                        ticks: { stepSize: 1, color: '#64748B' }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { color: '#64748B', font: { weight: '600' } }
                    }
                }
            }
        });
    }

    // --- 2. Market Distribution (Doughnut) ---
    const industryCanvas = document.getElementById('industryChart');
    if (industryCanvas) {
        const ctxInd = industryCanvas.getContext('2d');

        // Calculate total for percentages
        const total = industryValues.reduce((a, b) => a + b, 0);

        new Chart(ctxInd, {
            type: 'doughnut',
            data: {
                labels: industryKeys,
                datasets: [{
                    data: industryValues,
                    // UPDATED: Professional Dark Blue Palette
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
                    borderColor: '#ffffff',
                    borderWidth: 2,
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%', // Slightly thicker ring for better visibility
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            padding: 20,
                            font: { size: 12, family: "'Plus Jakarta Sans', sans-serif" },
                            color: '#334155',
                            // Custom Label Generator: Adds Percentage to Legend
                            generateLabels: function (chart) {
                                const data = chart.data;
                                if (data.labels.length && data.datasets.length) {
                                    return data.labels.map((label, i) => {
                                        const value = data.datasets[0].data[i];
                                        const percentage = total > 0 ? ((value / total) * 100).toFixed(1) + '%' : '0%';
                                        return {
                                            text: `${label} (${percentage})`,
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
                        bodyFont: { weight: 'bold' },
                        callbacks: {
                            label: function (context) {
                                return ` ${context.label}: ${context.parsed} customers`;
                            }
                        }
                    }
                }
            }
        });
    }
}