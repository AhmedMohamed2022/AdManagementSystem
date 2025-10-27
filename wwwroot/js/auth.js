// Password Strength Checker
document.addEventListener('DOMContentLoaded', function () {
    const passwordInput = document.querySelector('input[type="password"]');

    if (passwordInput) {
        // Create strength indicator
        const strengthDiv = document.createElement('div');
        strengthDiv.className = 'password-strength';
        strengthDiv.innerHTML = '<div class="password-strength-bar"></div>';
        passwordInput.parentElement.appendChild(strengthDiv);

        const strengthBar = strengthDiv.querySelector('.password-strength-bar');

        passwordInput.addEventListener('input', function () {
            const password = this.value;
            let strength = 0;

            if (password.length >= 8) strength++;
            if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
            if (/\d/.test(password)) strength++;
            if (/[^a-zA-Z\d]/.test(password)) strength++;

            strengthBar.className = 'password-strength-bar';

            if (strength <= 1) {
                strengthBar.classList.add('weak');
            } else if (strength <= 3) {
                strengthBar.classList.add('medium');
            } else {
                strengthBar.classList.add('strong');
            }
        });
    }

    // Form submission loading state
    const forms = document.querySelectorAll('form[id*="account"], form[id*="registerForm"]');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.classList.contains('btn-loading')) {
                submitBtn.classList.add('btn-loading');
                submitBtn.disabled = true;
            }
        });
    });

    // Auto-focus first input
    const firstInput = document.querySelector('.auth-card input:not([type="hidden"])');
    if (firstInput) {
        firstInput.focus();
    }
});