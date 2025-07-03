
let bookingChart;

$(document).ready(function () {

    $(document)
        .ajaxStart(function () {
            $('#loadingOverlay').removeClass('d-none');
        })
        .ajaxStop(function () {
            $('#loadingOverlay').addClass('d-none');
        });

    toastr.options = {
        closeButton: true,
        progressBar: true,
        timeOut: "7000",
        extendedTimeOut: "2000",
    };

    if (document.getElementById('bookingChart')) {
        initializeSizeChart();
    }

    setupEventHandlers();
    setInterval(updateDashboard, 30000);
    initializeTheme();

});

function initializeSizeChart() {
    const canvasElem = document.getElementById('bookingChart');
    if (!canvasElem) {
        return;
    }
    bookingChart = new Chart(canvasElem, {
        type: 'line',

        data: {
            labels: bookingLabels,
            datasets: [{
                label: 'Bookings',
                data: bookingData,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
                fill: true,
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1 },
                    grid: { drawBorder: false }
                },
                x: {
                    grid: { display: false }
                }
            },
            plugins: {
                legend: { position: 'top' },
                title: {
                    display: true,
                    text: 'Booking Chart'
                }
            }
        }
    });
}

function updateCharts(data) {
    if (!bookingChart) return;
    if (!data.bookingChart || !data.bookingChart.labels || !data.bookingChart.data) {
        console.warn("Incomplete bookingChart data:", data.bookingChart);
        return;
    }

    bookingChart.data.labels = data.bookingChart.labels;
    bookingChart.data.datasets[0].data = data.bookingChart.data;
    bookingChart.update();
}

function updateStatsCards(data) {
    const statsValues = document.querySelectorAll('.stat-value');

    if (statsValues.length > 0) {
        animateValue(statsValues[0], parseInt(statsValues[0].textContent), data.totalTrips, 500);
        animateValue(statsValues[1], parseInt(statsValues[1].textContent), data.totalBookings, 500);
        animateValue(statsValues[2], parseInt(statsValues[2].textContent), data.totalUsers, 500);
    }

    // Animate Stats Cards(visual Feedback)
    const statsCards = document.querySelectorAll('.stats-card');
    statsCards.forEach(card => {
        card.classList.add('updated');
        setTimeout(() => card.classList.remove('updated'),300) // remove class after animation
    });

}

function animateValue(element, start, end, duration) {
    let startTimestamp = null;

    function step(timestamp) {
        if (!startTimestamp) startTimestamp = timestamp;
        const progress = Math.min((timestamp - startTimestamp) / duration, 1);
        element.textContent = Math.floor(progress * (end - start) + start);
        if (progress < 1) {
            window.requestAnimationFrame(step);
        }
    }

    window.requestAnimationFrame(step);
}

// Real-time Updates
function updateDashboard() {
    $.ajax({
        url: '/Admin/GetDashboard',
        method: 'GET',
        success: function (data) {
            console.log(data);
            updateCharts(data);
            updateStatsCards(data);
        }
    });
}


function setupEventHandlers() {
    $('#sidebarCollapse').on('click', function () {

        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    $('.sidebar-item a').each(function () {
        if (this.href === window.location.href) {
            $(this).addClass('active');
        }
    });

    // User Management
    $(document).on('click', '.delete-user', function () {
        const userId = $(this).data('user-id');
        const userName = $(this).data('user-name');

        Swal.fire({
            title: 'Confirm Deletion',
            html: `Delete user <strong>${userName}</strong>?`,
            icon: 'warning',
            showCancelButton: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `@Url.Action("DeleteUser", "Admin")/${userId}`,
                    method: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $(`tr[data-user-id="${userId}"]`).fadeOut();
                        } else {
                            toastr.error(response.message);
                        }
                    }
                });
            }
        });
    });

    $(document).on('click', '.toggle-status', function () {
        const userId = $(this).data('user-id');
        const $row = $(this).closest('tr');
        const $badge = $row.find('.status-badge');

        $.ajax({
            url: `@Url.Action("ToggleUserStatus", "Admin")/${userId}`,
            method: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    $badge.toggleClass('bg-success bg-danger')
                        .text(response.isActive ? 'Active' : 'Inactive');

                    if (response.lastLogin) {
                        $row.find('[data-last-login]').text(response.lastLogin);
                    }
                }
            },
            error: function (xhr) {
                toastr.error('Error updating user status');
            }
        });
    });

    $('#userSearch').on('input', function () {
        const search = $(this).val();
        window.location.href = `@Url.Action("Users")?search=${search}`;
    });

    $('#applyBulkAction').on('click', function () {
        const action = $('.bulk-action-select').val();
        const selectedIds = $('.user-checkbox:checked').map(function () {
            return $(this).closest('tr').data('user-id');
        }).get();

        $.ajax({
            url: '/Admin/BulkUserActions',
            method: 'POST',
            data: { action, userIds: selectedIds },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
            }
        });
    });

    // Booking Management
    $(document).on('click', '.cancel-booking', function () {
        const $btn = $(this);
        const bookingId = $btn.data('id');

        Swal.fire({
            title: "Confirm Cancellation",
            text: "Are you sure you want to cancel this booking?",
            icon: "warning",
            showCancelButton: true,
        }).then((result) => {
            if (!result.isConfirmed) return;

            $.ajax({
                url: `/Admin/CancelBooking/${bookingId}`,
                method: 'POST'
            }).done(function (response) {
                if (response.success) {
                    toastr.success('Booking cancelled successfully');
                    $btn.closest('tr').fadeOut();
                } else {
                    toastr.error('Cancellation failed');
                }
            }).fail(function () {
                toastr.error('Server error during cancellation');
            });
        });
    });
    $('.change-status').on('click', function () {
        const bookingId = $(this).data('id');
        const status = $(this).data('status');

        $.ajax({
            url: '/Admin/UpdateBookingStatus',
            method: 'POST',
            data: { bookingId: bookingId, status: status },
            success: function (response) {
                if (response.success) {
                    toastr.success('Status Updated');
                    location.reload();
                }
            }
        });

    });

    $(document).on('click', '.mark-cash-paid', function () {
        const bookingId = $(this).data('id');
        $.ajax({
            url: `/Admin/MarkCashPaid/${bookingId}`,
            method: 'POST',
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (resp) {
                if (resp.success) {
                    toastr.success(resp.message);
                    // update that row’s "PaymentStatus" cell to “Paid” badge
                    $(`[data-booking-id="${bookingId}"] .payment-status-cell`)
                        .removeClass('badge-warning')
                        .addClass('badge-success')
                        .text('Paid');
                } else {
                    toastr.error(resp.message);
                }
            }
        });
    });

    $('#bulkForm').on('submit', function (e) {
        e.preventDefault();
        $.ajax({
            url: $(this).attr('action'),
            method: 'POST',
            data: $(this).serialize(),
        })
            .done(function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Bulk action completed', 'Success');
                    setTimeout(() => location.reload(), 500);
                } else {
                    toastr.error(response.message || 'Bulk action failed', 'Error');
                }
            })
            .fail(function () {
                toastr.error('An unexpected error occurred', 'Error');
            });
    });


    $('.hard-delete-booking').on('click', function () {
        const bookingId = $(this).data('id');
        const tripTitle = $(this).closest('tr').find('td:nth-child(2)').text();
        Swal.fire({
            title: 'Permanently Delete',
            html: `This action cannot be undone.<br>Are you sure you want to permanently delete <strong>${tripTitle}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Delete Permanently',
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Admin/HardDeleteBooking?id=${bookingId}`,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $(`[data-id="${bookingId}"]`).closest('tr').fadeOut();
                        } else {
                            toastr.error(response.message || 'Failed to delete permanently.');
                        }
                    },
                    error: function (xhr) {
                        toastr.error(xhr.responseText || 'An error occurred while permanently deleting.');
                    }
                });
            }
        });
    });

    $('.restore-booking').on('click', function () {
        const bookingId = $(this).data('id');
        const tripTitle = $(this).data('title');

        Swal.fire({
            title: 'Restore Confirmation',
            html: `Are you sure you want to restore booking trip: <strong>${tripTitle}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            confirmButtonText: 'Restore'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Admin/RestoreBooking?id=${bookingId}`,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $(`[data-id="${bookingId}"]`).closest('tr').fadeIn();
                        }
                        else {
                            toastr.error(response.message);
                        }
                    }
                });
            }
        })
    });


    // Trip Management
    $('.soft-delete-trip').on('click', function () {
        const tripId = $(this).data('id');
        const tripTitle = $(this).closest('tr').find('td:nth-child(2)').text();

        Swal.fire({
            title: 'Confirm Delete',
            html: `Are you sure you want to delete <strong>${tripTitle}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            confirmButtonText: 'Delete',
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Admin/DeleteSoftTrip?id=${tripId}`,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $(`[data-id="${tripId}"]`).closest('tr').fadeOut();
                        }
                    },
                    error: function (xhr) {
                        toastr.error(xhr.responseText || 'An error occurred while deleting the trip.');
                    }
                });
            }

        });

    });

    $('.hard-delete-trip').on('click', function () {
        const tripId = $(this).data('id');
        const tripTitle = $(this).closest('tr').find('td:nth-child(2)').text();

        Swal.fire({
            title: 'Permanently Delete',
            html: `This action cannot be undone.<br>Are you sure you want to permanently delete <strong>${tripTitle}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Delete Permanently',
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Admin/DeleteTrip?id=${tripId}`,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $(`[data-id="${tripId}"]`).closest('tr').fadeOut();
                        } else {
                            toastr.error(response.message || 'Failed to delete permanently.');
                        }
                    },
                    error: function (xhr) {
                        toastr.error(xhr.responseText || 'An error occurred while permanently deleting.');
                    }
                });
            }
        });
    });

    $('.restore-trip').on('click', function () {
        const tripId = $(this).data('id');
        const tripTitle = $(this).data('title');

        Swal.fire({
            title: 'Restore Confirmation',
            html: `Are you sure you want to restore <strong>${tripTitle}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            confirmButtonText: 'Restore'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/Admin/TripRestore?id=${tripId}`,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $(`[data-id="${tripId}"]`).closest('tr').fadeIn();
                        }
                        else {
                            toastr.error(response.message);
                        }
                    }
                });
            }
        })
    });

    // Select/Deselect All
    $('#selectAll').on('change', function () {
        $('input[name="Ids"]').prop('checked', this.checked);
    });

    // Import CSV
    $('#importBtn').on('click', function () {
        const file = $('#importFile')[0].files[0];
        const form = new FormData();
        form.append('file', file);
        form.append('__RequestVerificationToken', $('[name="__RequestVerificationToken"]').val());
        $.ajax({
            url: '/Admin/Trips/ImportCsv',
            method: 'POST',
            data: form,
            contentType: false,
            processData: false
        }).done(() => location.reload());
    });

 
    $('.toggle-dest-active').on('change', function () {
        var $row = $(this).closest('tr'),
            id = $row.data('id'),
            isOn = $(this).is(':checked');
        token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();
        $.ajax({
            method: 'POST',
            url: '/Admin/DestinationToggleActive',
            data: {
                id: id,
                status: isOn,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message);
                }
            }
        });
    });
}

function initializeTheme() {
    const savedTheme = localStorage.getItem('admin-theme') || document.documentElement.getAttribute('data-user-theme') || 'light';
    setTheme(savedTheme);
}

function setTheme(theme) {
    const icons = document.querySelectorAll('[data-theme-icon]');
    icons.forEach(icon => {
        icon.style.display = icon.dataset.themeIcon === theme ? 'inline-block' : 'none';
    });

    document.documentElement.setAttribute('data-bs-theme', theme);
    localStorage.setItem('admin-theme', theme);

    fetch('/Admin/UpdateThemePreference?theme=' + theme, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value,
        },
    });
}

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', function () {
    const savedTheme = localStorage.getItem('admin-theme') || '@user.ThemePreference' || 'light';
    setTheme(savedTheme);
});

// Toggle handler
document.querySelector('[data-bs-theme-toggle]').addEventListener('click', function () {
    const currentTheme = document.documentElement.getAttribute('data-bs-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    setTheme(newTheme);
});

/*function initializeTheme() {
    // Use consistent localStorage key
    const savedTheme = localStorage.getItem('admin-theme') || '@user.ThemePreference' || 'light';
    setTheme(savedTheme);
}*/


// or document.addEventListener('DOMContentLoaded', initializeTheme)