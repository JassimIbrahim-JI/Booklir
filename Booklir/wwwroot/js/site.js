/* site.js */

// Navbar Scroll Effect
$(window).on('scroll', function () {
    const $navbar = $('.navbar');
    const scrollClass = 'scrolled';
    const scrollThreshold = 50;

    $navbar.toggleClass(scrollClass, $(this).scrollTop() > scrollThreshold);

    if ($navbar.hasClass(scrollClass)) {
        $navbar.css('backdrop-filter', 'blur(10px)');
    } else {
        $navbar.css('backdrop-filter', 'none');
    }
});


// hide initial page‐loader when everything (images, scripts) is loaded
$(window).on('load', function () {
    $('#page-loader').addClass('hidden');
});


// OPTIONAL: show loader for any AJAX requests
$(document)
  .ajaxStart(function() {
    $('#page-loader').removeClass('hidden');
  })
  .ajaxStop(function() {
    $('#page-loader').addClass('hidden');
  });

// Notification dropdown: mark as read on click
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.dropdown-item[data-notification-id]').forEach(item => {
        item.addEventListener('click', function () {
            const notificationId = this.dataset.notificationId;
            fetch(`/Notification/MarkAsRead/${notificationId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });
        });
    });

    // Update notification badge every 60 seconds
    setInterval(updateNotificationBadge, 60000);


    //Scroll reveal 
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.scroll-reveal').forEach(el => {
        observer.observe(el);
    });

    const scrollBtn = document.getElementById('scrollToTop');

    if (scrollBtn) {
        window.addEventListener('scroll', () => {
            if (window.pageYOffset > 300) {
                scrollBtn.classList.add('show');
            } else {
                scrollBtn.classList.remove('show');
            }
        });

        scrollBtn.addEventListener('click', () => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
    }

});

// scroll script
window.addEventListener('scroll', () => {
    const winScroll = document.body.scrollTop || document.documentElement.scrollTop;
    const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
    const scrolled = (winScroll / height) * 100;
    document.getElementById('scrollBar').style.width = scrolled + '%';
});

async function updateNotificationBadge() {
    try {
        const response = await fetch('/Notification/GetUnreadCount');
        if (response.ok) {
            const count = await response.json();
            const badge = document.querySelector('#notifDropdown .badge');
            if (badge) {
                badge.textContent = count > 0 ? count : '';
            }
        }
    } catch (error) {
        console.error('Failed to update notification badge', error);
    }
}

// Cancel Booking Modal
$(document).on('click', '.js-confirm-cancel', function () {
    const bookingId = $(this).data('id');
    const token = $('input[name="__RequestVerificationToken"]').val();
    const $modal = $('#cancelModal-' + bookingId);

    $.ajax({
        url: '/Booking/Cancel',
        method: 'POST',
        data: { bookingId, __RequestVerificationToken: token },
        success: function (res) {
            if (res.success) {
                toastr.success(res.message);
                // hide the modal
                $modal.modal('hide');
                // remove the card or row
                $(`[data-id="${bookingId}"]`).closest('.booking-card, tr').fadeOut(300);
            } else {
                toastr.error(res.message);
            }
        },
        error: function () {
            toastr.error('Error cancelling booking');
        }
    });
});

// Dropdown Animation
$('.dropdown').on('show.bs.dropdown', function () {
    const $menu = $(this).find('.dropdown-menu').first();
    $menu.stop(true, true).hide().slideDown(200);
});
$('.dropdown').on('hide.bs.dropdown', function () {
    const $menu = $(this).find('.dropdown-menu').first();
    $menu.stop(true, true).slideUp(200);
});

// Mobile Menu Close on Click
$(document).on('click', '.navbar-nav .nav-link', function () {
    $('.navbar-collapse').collapse('hide');
});

// AJAX Form Handling (generic)
$(document).on('submit', '.ajax-form', function (e) {
    e.preventDefault();
    const form = $(this);
    const button = form.find('button[type="submit"]');
    const origTxt = button.text().trim();

    // save it so we can restore later
    button.data('original-text', origTxt);

    button.prop('disabled', true)
        .html(
            '<span class="spinner-border spinner-border-sm" ' +
            'role="status" ' +
            'style="color: var(--primary-color)"></span> ' + 
            origTxt
        );

    $.ajax({
        url: form.attr('action'),
        type: form.attr('method'),
        data: form.serialize(),
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                if (response.redirectUrl) {
                    setTimeout(() => window.location.href = response.redirectUrl, 1500);
                }
            } else {
                (response.errors || [response.message]).forEach(msg => toastr.error(msg));
            }
        },
        error: () => toastr.error('An error occurred while processing your request.'),
        complete: () => button.prop('disabled', false).html(button.data('original-text'))
    });
});

// Booking Form with Stripe
$(document).on('submit', '.booking-form', function (e) {
    e.preventDefault();
    const form = $(this);
    const button = form.find('button[type="submit"]');

    button.prop('disabled', true)
        .html(
            '<span class="spinner-border spinner-border-sm" ' +
            'role="status" ' +
            'style="color: var(--primary-color)"></span> ' +
            'Processing...'
        );

    $.ajax({
        url: form.attr('action'),
        type: form.attr('method'),
        data: form.serialize(),
        success: function (response) {
            if (!response.success) {
                (response.errors || [response.message]).forEach(msg => toastr.error(msg));
                return button.prop('disabled', false).html(button.data('original-text'));
            }
            if (!response.isPaymentRequired) {
                toastr.success('Booking created! Redirecting...');
                return setTimeout(() => window.location.href = response.redirectUrl, 1000);
            }
            toastr.info('Completing payment...');
            stripe.confirmCardPayment(response.clientSecret, { payment_method: { card: cardElement } })
                .then(result => {
                    if (result.error) {
                        toastr.error(result.error.message);
                        return $.post('/api/Payment/MarkFailed', { bookingId: response.bookingId });
                    }
                    toastr.success('Payment succeeded! Booking confirmed.');
                    return $.post('/api/Payment/MarkPaid', {
                        bookingId: response.bookingId,
                        transactionId: result.paymentIntent.id
                    }).done(() => setTimeout(() => window.location.href = `/Booking/Confirmation/${response.bookingId}`, 800));
                })
                .catch(err => {
                    toastr.error('Payment processing error');
                    console.error(err);
                });
        },
        error: () => toastr.error('An error occurred while processing your request.'),
        complete: () => button.prop('disabled', false).html(button.data('original-text'))
    });
});

$(document).on('click', '.cancel-booking', function () {
    const $btn = $(this);
    const bookingId = $btn.data('id');
    const token = $('input[name="__RequestVerificationToken"]').val();

    Swal.fire({
        title: 'Cancel booking?',
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, cancel it!',
        cancelButtonText: 'No, keep it'
    }).then((result) => {
        if (!result.isConfirmed) return;

        $.ajax({
            url: '/Booking/Cancel',
            type: 'POST',
            data: {
                bookingId: bookingId,
                __RequestVerificationToken: token
            },
            success: function (res) {
                if (res.success) {
                    // remove the row
                    $btn.closest('tr').fadeOut(300, function () { $(this).remove(); });
                    Swal.fire('Cancelled!', res.message, 'success');
                } else {
                    Swal.fire('Oops...', res.message || 'Unable to cancel booking', 'error');
                }
            },
            error: function () {
                Swal.fire('Error', 'Error while cancelling booking', 'error');
            }
        });
    });
});

// Global AJAX Error Handling
$(document).ajaxError((event, jqxhr) => {
    const msg = jqxhr.responseJSON?.message || 'An error occurred while processing your request.';
    toastr.error(msg);
});

// Thumbnail Click to Swap Main Image
$('.thumbnail').on('click', function () {
    const newSrc = $(this).attr('src');
    $('.main-image').fadeOut(200, function () {
        $(this).attr('src', newSrc).fadeIn(200);
    });
});

// Price Range Filter (if you have a slider)
$('#priceRange').on('input', function () {
    const max = parseFloat($(this).val());
    $('.trip-card').each(function () {
        const price = parseFloat($(this).data('price'));
        $(this).toggle(price <= max);
    });
});

// Utility: debounce
function debounce(fn, delay) {
    let timer;
    return function () {
        const context = this;
        const args = arguments;
        clearTimeout(timer);
        timer = setTimeout(() => fn.apply(context, args), delay);
    };
}

// Live Search on Home & Featured Trips Section (debounced, spinner, no-results, error UI)
function performHomeSearch() {
    const form = $('#homeSearchForm');
    const url = form.attr('action');
    const data = form.serialize();

    $.ajax({
        url: url,
        data: data,
        type: 'GET',
        beforeSend: function () {
            $('#homeSearchResults').html('<div class="text-center py-5"><span class="spinner-border"></span></div>');
        },
        success: function (html) {
            const $container = $('#homeSearchResults');
            $container.html(html);
            if ($container.find('.row').children().length === 0) {
                $container.html('<div class="alert alert-info">No trips match your criteria.</div>');
            }
        },
        error: function () {
            $('#homeSearchResults').html('<div class="alert alert-danger">Failed to load search results. Please try again.</div>');
        }
    });
}

$(document).on('input change', '#searchTripInput, #startDateInput, #durationSelect', debounce(performHomeSearch, 300));

// AJAX Pagination for Search Results Container
$(document).on('click', '#homeSearchResults .pagination a', function (e) {
    e.preventDefault();
    const page = $(this).data('page');
    $('#homeSearchForm').find('input[name="Page"]').remove();
    $('<input>').attr({ type: 'hidden', name: 'Page', value: page }).appendTo('#homeSearchForm');
    performHomeSearch();
});
