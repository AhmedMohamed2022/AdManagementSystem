// ========================================
// Sidebar Toggle Functionality
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarClose = document.getElementById('sidebarClose');
    const sidebarBackdrop = document.getElementById('sidebarBackdrop');

    // Toggle sidebar on mobile
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.add('active');
            sidebarBackdrop.classList.add('active');
            document.body.style.overflow = 'hidden';
        });
    }

    // Close sidebar
    function closeSidebar() {
        sidebar.classList.remove('active');
        sidebarBackdrop.classList.remove('active');
        document.body.style.overflow = '';
    }

    if (sidebarClose) {
        sidebarClose.addEventListener('click', closeSidebar);
    }

    if (sidebarBackdrop) {
        sidebarBackdrop.addEventListener('click', closeSidebar);
    }

    // Close sidebar on escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && sidebar.classList.contains('active')) {
            closeSidebar();
        }
    });
    // Active link highlighting
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.nav-link');

    navLinks.forEach(link => {
        link.classList.remove('active'); // Remove any existing active class
        const linkPath = link.getAttribute('href')?.toLowerCase();

        // Exact match for the current page
        if (linkPath && linkPath === currentPath) {
            link.classList.add('active');
        }
    });

    // Auto-close sidebar on navigation (mobile)
    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                setTimeout(closeSidebar, 200);
            }
        });
    });

    // Handle window resize
    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (window.innerWidth >= 992) {
                closeSidebar();
            }
        }, 250);
    });
});

// ========================================
// Smooth Scroll for Anchor Links
// ========================================

document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const href = this.getAttribute('href');
        if (href !== '#' && document.querySelector(href)) {
            e.preventDefault();
            document.querySelector(href).scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// ========================================
// Chart.js Default Configuration
// ========================================

if (typeof Chart !== 'undefined') {
    Chart.defaults.font.family = "'Cairo', sans-serif";
    Chart.defaults.font.size = 13;
    Chart.defaults.color = '#637381';
    Chart.defaults.plugins.legend.display = true;
    Chart.defaults.plugins.legend.position = 'bottom';
    Chart.defaults.plugins.tooltip.backgroundColor = '#212B36';
    Chart.defaults.plugins.tooltip.padding = 12;
    Chart.defaults.plugins.tooltip.cornerRadius = 8;
    Chart.defaults.plugins.tooltip.titleColor = '#FFFFFF';
    Chart.defaults.plugins.tooltip.bodyColor = '#FFFFFF';

    // Minimal Color Palette for Charts
    window.chartColors = {
        primary: '#2065D1',
        success: '#00AB55',
        warning: '#FFAB00',
        error: '#FF5630',
        info: '#00B8D9',
        primaryLight: '#D6E8FF',
        successLight: '#D8F8E8',
        warningLight: '#FFF5CC',
        errorLight: '#FFE9E5',
        infoLight: '#CCEFF5'
    };
}

// ========================================
// Auto-hide Alerts
// ========================================

document.querySelectorAll('.alert').forEach(alert => {
    if (alert.classList.contains('alert-dismissible')) {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    }
});

// ========================================
// Form Validation Enhancement
// ========================================

document.querySelectorAll('form').forEach(form => {
    form.addEventListener('submit', function (e) {
        if (!form.checkValidity()) {
            e.preventDefault();
            e.stopPropagation();
        }
        form.classList.add('was-validated');
    });
});

// ========================================
// Tooltip & Popover Initialization
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    // Initialize Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(
        document.querySelectorAll('[data-bs-toggle="tooltip"]')
    );
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize Bootstrap popovers
    const popoverTriggerList = [].slice.call(
        document.querySelectorAll('[data-bs-toggle="popover"]')
    );
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});