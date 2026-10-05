document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Hero background: slow cross-fade between the photographer's featured photos.
    var slides = document.querySelectorAll('.ab-slide');
    if (slides.length > 1 && !reduceMotion) {
        var current = 0;
        setInterval(function () {
            slides[current].classList.remove('on');
            current = (current + 1) % slides.length;
            slides[current].classList.add('on');
        }, 6000);
    }

    // Fade sections in as they scroll into view.
    var revealables = document.querySelectorAll('.reveal');
    if (!('IntersectionObserver' in window) || reduceMotion) {
        revealables.forEach(function (el) { el.classList.add('in'); });
    } else {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                entry.target.classList.add('in');
                io.unobserve(entry.target);
                entry.target.querySelectorAll('[data-count]').forEach(countUp);
            });
        }, { threshold: 0.15 });
        revealables.forEach(function (el) { io.observe(el); });
    }

    // Stats count up from zero.
    function countUp(el) {
        var target = parseInt(el.getAttribute('data-count'), 10);
        if (!target || reduceMotion) return;
        var start = null;
        function step(ts) {
            if (!start) start = ts;
            var p = Math.min(1, (ts - start) / 1200);
            el.textContent = Math.round(target * (1 - Math.pow(1 - p, 3)));
            if (p < 1) requestAnimationFrame(step);
        }
        el.textContent = '0';
        requestAnimationFrame(step);
    }
});
