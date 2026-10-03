(function () {
    'use strict';

    // Start downloading the next page as soon as the shutter starts closing,
    // so it is ready by the time the close animation finishes.
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a[href^="/"]');
        if (!link || link.target === '_blank') return;
        var hint = document.createElement('link');
        hint.rel = 'prefetch';
        hint.href = link.getAttribute('href');
        document.head.appendChild(hint);
    }, true);

    // Coming back with the browser's Back button can restore the page with the
    // shutter still closed (a black screen). Re-open it.
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) return;
        document.querySelectorAll('.vs-iris-closing, .vs-nav-iris.closing').forEach(function (el) {
            el.classList.remove('vs-iris-closing', 'closing');
        });
    });
})();
