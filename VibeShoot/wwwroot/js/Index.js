document.addEventListener('DOMContentLoaded', function () {
    var redirectUrl = '/photographers';
    var iris = document.getElementById('visIris');
    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var HOLD_MS = 4000;
    var CLOSE_MS = 900;

    setTimeout(function () {
        if (reduceMotion) {
            window.location.href = redirectUrl;
            return;
        }

        iris.classList.add('vs-iris-closing');

        setTimeout(function () {
            window.location.href = redirectUrl;
        }, CLOSE_MS);

    }, HOLD_MS);
});