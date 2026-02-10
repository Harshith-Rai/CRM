// Login.js - Enhanced with animations and interactions

document.addEventListener('DOMContentLoaded', function () {
    // Initialize all features
    initPasswordToggle();
    initPasswordStrength();
    initFormValidation();
    initRippleEffect();
    initFloatingLabels();
});

// Password visibility toggle
function initPasswordToggle() {
    const toggleButton = document.querySelector('.toggle-password');
    const passwordInput = document.getElementById('password');

    if (toggleButton && passwordInput) {
        toggleButton.addEventListener('click', function () {
            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
            passwordInput.setAttribute('type', type);

            const icon = this.querySelector('i');
            icon.classList.toggle('fa-eye');
            icon.classList.toggle('fa-eye-slash');

            // Animate icon
            icon.style.transform = 'scale(0.8)';
            setTimeout(() => {
                icon.style.transform = 'scale(1)';
            }, 150);
        });
    }
}

// Password strength indicator
function initPasswordStrength() {
    const passwordInput = document.getElementById('password');
    const strengthIndicator = document.getElementById('passwordStrength');

    if (!passwordInput || !strengthIndicator) return;

    passwordInput.addEventListener('input', function () {
        const password = this.value;

        if (password.length === 0) {
            strengthIndicator.classList.remove('show', 'weak', 'fair', 'good', 'strong');
            return;
        }

        const strength = calculatePasswordStrength(password);
        const strengthText = strengthIndicator.querySelector('.strength-text');

        // Show indicator
        strengthIndicator.classList.add('show');

        // Remove all strength classes
        strengthIndicator.classList.remove('weak', 'fair', 'good', 'strong');

        // Add appropriate class and text
        if (strength.score === 1) {
            strengthIndicator.classList.add('weak');
            strengthText.textContent = 'Weak password';
        } else if (strength.score === 2) {
            strengthIndicator.classList.add('fair');
            strengthText.textContent = 'Fair password';
        } else if (strength.score === 3) {
            strengthIndicator.classList.add('good');
            strengthText.textContent = 'Good password';
        } else if (strength.score === 4) {
            strengthIndicator.classList.add('strong');
            strengthText.textContent = 'Strong password';
        }
    });

    // Hide on blur if empty
    passwordInput.addEventListener('blur', function () {
        if (this.value.length === 0) {
            strengthIndicator.classList.remove('show');
        }
    });
}

function calculatePasswordStrength(password) {
    let score = 0;

    if (password.length >= 8) score++;
    if (password.length >= 12) score++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;

    // Cap at 4
    score = Math.min(score, 4);

    return { score };
}

// Form validation with shake animation
function initFormValidation() {
    const form = document.getElementById('loginForm');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        let isValid = true;
        const inputs = form.querySelectorAll('.form-control');

        inputs.forEach(input => {
            const wrapper = input.closest('.form-group');

            if (!input.value.trim()) {
                isValid = false;

                // Add shake animation
                wrapper.classList.add('shake-animation');

                // Add error styling
                input.classList.add('input-validation-error');

                // Remove shake after animation
                setTimeout(() => {
                    wrapper.classList.remove('shake-animation');
                }, 500);
            } else {
                input.classList.remove('input-validation-error');
            }
        });

        // Email validation
        const emailInput = document.getElementById('email');
        if (emailInput && emailInput.value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(emailInput.value)) {
                isValid = false;
                const wrapper = emailInput.closest('.form-group');
                wrapper.classList.add('shake-animation');
                emailInput.classList.add('input-validation-error');

                setTimeout(() => {
                    wrapper.classList.remove('shake-animation');
                }, 500);
            }
        }

        if (!isValid) {
            e.preventDefault();
            return false;
        }

        // Show loading state
        const submitButton = form.querySelector('.btn-login');
        if (submitButton) {
            submitButton.classList.add('loading');
            submitButton.disabled = true;
        }
    });

    // Remove error styling on input
    const inputs = form.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('input', function () {
            this.classList.remove('input-validation-error');
        });
    });
}

// Ripple effect on button click
function initRippleEffect() {
    const rippleButtons = document.querySelectorAll('.ripple-button');

    rippleButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            // Get button position
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            // Create ripple element
            const ripple = document.createElement('span');
            ripple.classList.add('ripple');
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';

            this.appendChild(ripple);

            // Remove ripple after animation
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });
}

// Floating labels enhancement
function initFloatingLabels() {
    const inputs = document.querySelectorAll('.floating-label-group .form-control');

    inputs.forEach(input => {
        // Check initial state
        if (input.value) {
            input.classList.add('has-value');
        }

        // Add transition effect
        input.addEventListener('focus', function () {
            const wrapper = this.closest('.input-wrapper');
            wrapper.style.transform = 'scale(1.01)';
        });

        input.addEventListener('blur', function () {
            const wrapper = this.closest('.input-wrapper');
            wrapper.style.transform = 'scale(1)';

            if (this.value) {
                this.classList.add('has-value');
            } else {
                this.classList.remove('has-value');
            }
        });
    });
}

// Add smooth scroll reveal for elements (if needed for future enhancements)
function observeElements() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            }
        });
    }, {
        threshold: 0.1
    });

    const elements = document.querySelectorAll('.fade-in-up, .fade-in-scale');
    elements.forEach(el => observer.observe(el));
}

// Add custom validation messages
function setCustomValidationMessages() {
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');

    if (emailInput) {
        emailInput.addEventListener('invalid', function () {
            if (this.validity.valueMissing) {
                this.setCustomValidity('Please enter your email address');
            } else if (this.validity.typeMismatch) {
                this.setCustomValidity('Please enter a valid email address');
            }
        });

        emailInput.addEventListener('input', function () {
            this.setCustomValidity('');
        });
    }

    if (passwordInput) {
        passwordInput.addEventListener('invalid', function () {
            if (this.validity.valueMissing) {
                this.setCustomValidity('Please enter your password');
            }
        });

        passwordInput.addEventListener('input', function () {
            this.setCustomValidity('');
        });
    }
}

// Initialize custom validation on load
document.addEventListener('DOMContentLoaded', setCustomValidationMessages);

// Handle form errors with animation
function handleServerErrors() {
    const errorSummary = document.querySelector('.validation-summary-errors');
    if (errorSummary) {
        errorSummary.style.animation = 'fadeInUp 0.5s ease-out';

        // Scroll to error
        errorSummary.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

// Call on page load
handleServerErrors();

// Performance optimization: Use passive event listeners where appropriate
function addPassiveListeners() {
    const scrollElements = document.querySelectorAll('.scrollable');
    scrollElements.forEach(el => {
        el.addEventListener('scroll', function () {
            // Handle scroll
        }, { passive: true });
    });
}

addPassiveListeners();

// Add keyboard navigation enhancement
document.addEventListener('keydown', function (e) {
    // Enable enter key on inputs to submit form
    if (e.key === 'Enter' && e.target.tagName === 'INPUT') {
        const form = e.target.closest('form');
        if (form) {
            const submitButton = form.querySelector('[type="submit"]');
            if (submitButton && !submitButton.disabled) {
                submitButton.click();
            }
        }
    }
});

// Prevent form resubmission on page refresh
if (window.history.replaceState) {
    window.history.replaceState(null, null, window.location.href);
}
