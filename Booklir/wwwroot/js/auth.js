// auth.js

// Password Visibility Toggle
document.addEventListener('click', function (e) {
    const toggle = e.target.closest('.password-toggle');
    if (!toggle) return;

    const input = toggle.parentElement.querySelector('input');
    const icon = toggle.querySelector('i');
    const newType = (input.type === 'password') ? 'text' : 'password';

    input.type = newType;
    icon.classList.toggle('bi-eye');
    icon.classList.toggle('bi-eye-slash');
});


// Form Validation Callbacks

function onLoginSuccess(response) {
    if (response.redirectUrl) {
        window.location.href = response.redirectUrl;
    }
}

function onLoginFailure(error) {
    if (error.status === 400) {
        toastr.error(error.responseText, 'Login Failed');
    } else {
        toastr.error('An unexpected error occurred', 'Login Failed');
    }
}

function onRegisterSuccess(response) {
    if (response.redirectUrl) {
        window.location.href = response.redirectUrl;
    }
}

function onRegisterFailure(error) {
    if (error.status === 400) {
        toastr.error(error.responseText, 'Register Failed');
    } else {
        toastr.error('An unexpected error occurred', 'Register Failed');
    }
}



// Bootstrap custom validation
(function () {
    'use strict';
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
})();

// Password visibility toggle
document.addEventListener('click', e => {
    const toggle = e.target.closest('.password-toggle');
    if (!toggle) return;
    const input = toggle.closest('.mb-3').querySelector('input[type="password"]');
    const icon = toggle.querySelector('i');
    input.type = input.type === 'password' ? 'text' : 'password';
    icon.classList.toggle('bi-eye');
    icon.classList.toggle('bi-eye-slash');
});


// AJAX-powered forms (login & register)
// Single AJAX‐powered forms handler
$(function () {
    $('form[data-ajax="true"]').on('submit', function (e) {
        e.preventDefault();
        const form = $(this);

        // prevent AJAX if client validation failed
        if (!this.checkValidity()) return;

        const btn = form.find('button[type="submit"]');
        const orig = btn.data('original-text') || btn.text();
        btn.prop('disabled', true)
            .html('<span class="spinner-border spinner-border-sm text-white" role="status"></span> ' + orig);

        $.ajax({
            url: form.attr('action'),
            type: form.attr('method'),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            data: form.serialize(),
            success: function (res) {
                if (form.hasClass('register-form')) onRegisterSuccess(res);
                else onLoginSuccess(res);
            },
            error: function (err) {
                if (form.hasClass('register-form')) onRegisterFailure(err);
                else onLoginFailure(err);
            },
            complete: function () {
                btn.prop('disabled', false).html(orig);
            }
        });
    });
});

// 1) Hide loader on full page load
$(window).on('load', function () {
    $('#page-loader').addClass('hidden');
});
