document.addEventListener('DOMContentLoaded', function () {
    var iris = document.getElementById('visIris');
    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var CLOSE_MS = 900;
    var cards = document.querySelectorAll('.vs-photog-card');

    cards.forEach(function (card) {
        card.addEventListener('click', function (e) {
            var href = card.getAttribute('href');
            if (!href) { return; }

            e.preventDefault();

            if (reduceMotion) {
                window.location.href = href;
                return;
            }

            iris.classList.add('vs-iris-closing');

            setTimeout(function () {
                window.location.href = href;
            }, CLOSE_MS);
        });
    });
});