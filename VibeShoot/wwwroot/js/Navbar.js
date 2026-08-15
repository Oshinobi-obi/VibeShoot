document.addEventListener('DOMContentLoaded', function () {
    var navLinks = document.querySelectorAll('.vs-nav-link');
    var navIris = document.getElementById('navIris');
    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var CLOSE_MS = 900;

    navLinks.forEach(function (link) {
        link.addEventListener('click', function (e) {
            var href = link.getAttribute('href');

            if (href && !link.classList.contains('active')) {
                e.preventDefault();

                if (reduceMotion) {
                    window.location.href = href;
                    return;
                }

                navIris.classList.add('closing');

                setTimeout(function () {
                    window.location.href = href;
                }, CLOSE_MS);
            }
        });
    });
});