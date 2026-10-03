document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var items = Array.prototype.slice.call(document.querySelectorAll('.al-item'));

    // Keep the sticky filter bar just below the navbar (its height changes on phones).
    var nav = document.querySelector('.vs-navbar');
    function syncNavHeight() {
        if (nav) document.documentElement.style.setProperty('--nav-h', nav.offsetHeight + 'px');
    }
    syncNavHeight();
    window.addEventListener('resize', syncNavHeight);

    // ---------------------------------------------------------- Reveal on scroll
    var io = null;
    if ('IntersectionObserver' in window && !reduceMotion) {
        io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                entry.target.classList.add('in');
                io.unobserve(entry.target);
            });
        }, { rootMargin: '0px 0px -40px 0px', threshold: 0.05 });
    }
    function reveal(list) {
        list.forEach(function (el, i) {
            el.style.setProperty('--d', ((i % 12) * 55) + 'ms');
            if (io) { el.classList.remove('in'); io.observe(el); }
            else el.classList.add('in');
        });
    }
    // Let the shutter open before the photos start appearing.
    setTimeout(function () { reveal(items); }, reduceMotion ? 0 : 700);

    // ---------------------------------------------------------- Filters
    var chips = document.querySelectorAll('.al-chip');
    var visible = items.slice();
    chips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            if (chip.classList.contains('on')) return;
            chips.forEach(function (c) { c.classList.toggle('on', c === chip); });
            var filter = chip.getAttribute('data-filter');
            visible = items.filter(function (el) { return filter === 'all' || el.getAttribute('data-cat') === filter; });
            items.forEach(function (el) { el.classList.toggle('hide', visible.indexOf(el) < 0); });
            reveal(visible);
            var grid = document.getElementById('alGrid');
            var page = document.getElementById('albumPage');
            if (grid && page && grid.getBoundingClientRect().top < 0) {
                page.scrollTo({ top: grid.offsetTop - 140, behavior: reduceMotion ? 'auto' : 'smooth' });
            }
        });
    });

    // ---------------------------------------------------------- Lightbox
    var lb = document.getElementById('alLightbox');
    if (!lb || !items.length) return;
    var lbImg = document.getElementById('alLbImg');
    var lbCount = document.getElementById('alLbCount');
    var lbCat = document.getElementById('alLbCat');
    var current = 0;
    var lastFocus = null;

    function show(index) {
        current = (index + visible.length) % visible.length;
        var img = visible[current].querySelector('img');
        lbImg.classList.add('swap');
        var next = new Image();
        next.onload = next.onerror = function () {
            lbImg.src = img.getAttribute('src');
            lbImg.alt = img.alt;
            requestAnimationFrame(function () { lbImg.classList.remove('swap'); });
        };
        next.src = img.getAttribute('src');
        lbCount.textContent = (current + 1) + ' / ' + visible.length;
        lbCat.textContent = visible[current].querySelector('figcaption span').textContent;
        // Warm up the neighbours so arrowing through feels instant.
        [current + 1, current - 1].forEach(function (n) {
            var el = visible[(n + visible.length) % visible.length];
            if (el) new Image().src = el.querySelector('img').getAttribute('src');
        });
    }

    function open(el) {
        lastFocus = document.activeElement;
        lb.hidden = false;
        requestAnimationFrame(function () { lb.classList.add('open'); });
        show(visible.indexOf(el));
        document.getElementById('alLbClose').focus();
    }

    function close() {
        lb.classList.remove('open');
        setTimeout(function () { lb.hidden = true; lbImg.src = ''; }, 300);
        if (lastFocus) lastFocus.focus();
    }

    items.forEach(function (el) {
        el.querySelector('button').addEventListener('click', function () { open(el); });
    });
    document.getElementById('alLbClose').addEventListener('click', close);
    document.getElementById('alLbPrev').addEventListener('click', function () { show(current - 1); });
    document.getElementById('alLbNext').addEventListener('click', function () { show(current + 1); });
    lb.addEventListener('click', function (e) { if (e.target === lb || e.target.classList.contains('al-lb-stage')) close(); });

    document.addEventListener('keydown', function (e) {
        if (lb.hidden) return;
        if (e.key === 'Escape') close();
        else if (e.key === 'ArrowRight') show(current + 1);
        else if (e.key === 'ArrowLeft') show(current - 1);
    });

    // Swipe left/right on phones.
    var touchX = null;
    lb.addEventListener('touchstart', function (e) { touchX = e.touches[0].clientX; }, { passive: true });
    lb.addEventListener('touchend', function (e) {
        if (touchX === null) return;
        var dx = e.changedTouches[0].clientX - touchX;
        if (Math.abs(dx) > 50) show(current + (dx < 0 ? 1 : -1));
        touchX = null;
    });
});
