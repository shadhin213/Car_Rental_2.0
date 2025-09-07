// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Vehicle card interaction
document.addEventListener('DOMContentLoaded', function() {
    // Vehicle card hover effects
    const vehicleCards = document.querySelectorAll('.vehicle-card');
    
    vehicleCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            const details = this.querySelector('.vehicle-details');
            if (details) {
                details.style.display = 'block';
                setTimeout(() => {
                    details.style.opacity = '1';
                    details.style.transform = 'translateY(0)';
                }, 10);
            }
        });
        
        card.addEventListener('mouseleave', function() {
            const details = this.querySelector('.vehicle-details');
            if (details) {
                details.style.opacity = '0';
                details.style.transform = 'translateY(10px)';
                setTimeout(() => {
                    details.style.display = 'none';
                }, 300);
            }
        });
    });

    // Counter animation
    const counters = document.querySelectorAll('.counter');
    const speed = 200;

    const animateCounter = (counter) => {
        const target = parseInt(counter.getAttribute('data-target'));
        const count = parseInt(counter.innerText);
        const increment = target / speed;

        if (count < target) {
            counter.innerText = Math.ceil(count + increment);
            setTimeout(() => animateCounter(counter), 1);
        } else {
            counter.innerText = target;
        }
    };

    // Intersection Observer for counter animation
    const observerOptions = {
        threshold: 0.7,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const counter = entry.target;
                animateCounter(counter);
                observer.unobserve(counter);
            }
        });
    }, observerOptions);

    counters.forEach(counter => {
        observer.observe(counter);
    });

    // Smooth scrolling for anchor links
    const anchorLinks = document.querySelectorAll('a[href^="#"]');
    
    anchorLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const targetId = this.getAttribute('href');
            const targetElement = document.querySelector(targetId);
            
            if (targetElement) {
                const offsetTop = targetElement.offsetTop - 80; // Account for fixed navbar
                window.scrollTo({
                    top: offsetTop,
                    behavior: 'smooth'
                });
            }
        });
    });

    // Loading animation for elements
    const loadingElements = document.querySelectorAll('.vehicle-card, .feature-card, .stat-card');
    
    const loadingObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('loaded');
                loadingObserver.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    });

    loadingElements.forEach(element => {
        element.classList.add('loading');
        loadingObserver.observe(element);
    });

    // Carousel auto-play with pause on hover
    const carousel = document.getElementById('heroSlider');
    if (carousel) {
        const carouselInstance = new bootstrap.Carousel(carousel, {
            interval: 5000,
            wrap: true
        });

        carousel.addEventListener('mouseenter', () => {
            carouselInstance.pause();
        });

        carousel.addEventListener('mouseleave', () => {
            carouselInstance.cycle();
        });
    }

    // Parallax effect for hero images
    window.addEventListener('scroll', () => {
        const scrolled = window.pageYOffset;
        const parallaxElements = document.querySelectorAll('.hero-image');
        
        parallaxElements.forEach(element => {
            const speed = 0.5;
            element.style.transform = `translateY(${scrolled * speed}px)`;
        });
    });

    // Vehicle type filter (for future use)
    const vehicleTypeLinks = document.querySelectorAll('[data-vehicle-type]');
    
    vehicleTypeLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const vehicleType = this.getAttribute('data-vehicle-type');
            
            // Add active class to clicked item
            vehicleTypeLinks.forEach(l => l.classList.remove('active'));
            this.classList.add('active');
            
            // Filter vehicles (placeholder for future functionality)
            console.log('Filtering by vehicle type:', vehicleType);
            
            // You can add actual filtering logic here
            // For now, just show a notification
            showNotification(`Showing ${vehicleType} vehicles`);
        });
    });

    // Notification system
    function showNotification(message) {
        const notification = document.createElement('div');
        notification.className = 'notification';
        notification.innerHTML = `
            <div class="notification-content">
                <i class="bi bi-info-circle"></i>
                <span>${message}</span>
                <button class="notification-close">&times;</button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Show notification
        setTimeout(() => {
            notification.classList.add('show');
        }, 100);
        
        // Auto hide after 3 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
        
        // Close button functionality
        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.addEventListener('click', () => {
            notification.classList.remove('show');
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        });
    }

    // Add CSS for notifications
    const notificationStyles = `
        <style>
            .notification {
                position: fixed;
                top: 20px;
                right: 20px;
                background: #007bff;
                color: white;
                padding: 15px 20px;
                border-radius: 10px;
                box-shadow: 0 5px 15px rgba(0,0,0,0.2);
                transform: translateX(400px);
                transition: transform 0.3s ease;
                z-index: 1000;
                max-width: 300px;
            }
            
            .notification.show {
                transform: translateX(0);
            }
            
            .notification-content {
                display: flex;
                align-items: center;
                gap: 10px;
            }
            
            .notification-close {
                background: none;
                border: none;
                color: white;
                font-size: 20px;
                cursor: pointer;
                margin-left: auto;
            }
            
            .notification-close:hover {
                opacity: 0.7;
            }
        </style>
    `;
    
    document.head.insertAdjacentHTML('beforeend', notificationStyles);

    // Add ripple effect to buttons
    const buttons = document.querySelectorAll('.btn');
    
    buttons.forEach(button => {
        button.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple');
            
            this.appendChild(ripple);
            
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // Add ripple effect CSS
    const rippleStyles = `
        <style>
            .btn {
                position: relative;
                overflow: hidden;
            }
            
            .ripple {
                position: absolute;
                border-radius: 50%;
                background: rgba(255, 255, 255, 0.3);
                transform: scale(0);
                animation: ripple-animation 0.6s linear;
                pointer-events: none;
            }
            
            @keyframes ripple-animation {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
        </style>
    `;
    
    document.head.insertAdjacentHTML('beforeend', rippleStyles);
});
