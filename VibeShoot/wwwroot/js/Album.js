document.addEventListener('DOMContentLoaded', function () {
    var tabs = document.querySelectorAll('.vs-tab');
    var panels = document.querySelectorAll('.vs-panel');

    setTimeout(function () {
        var firstPanel = document.querySelector('.vs-panel.active');
        if (firstPanel) {
            firstPanel.classList.add('vs-panel-visible');
            firstPanel.querySelectorAll('.vs-carousel-track').forEach(function (track) {
                activateCarousel(track);
            });
        }
    }, 1300);

    tabs.forEach(function (tab) {
        tab.addEventListener('click', function () {
            if (tab.classList.contains('active')) return;

            var target = tab.getAttribute('data-target');

            tabs.forEach(function (t) { t.classList.remove('active'); });
            tab.classList.add('active');

            var oldPanel = document.querySelector('.vs-panel.vs-panel-visible');
            if (oldPanel) {
                oldPanel.classList.remove('vs-panel-visible');

                setTimeout(function () {
                    oldPanel.classList.remove('active');

                    var newPanel = document.querySelector('.vs-panel[data-category="' + target + '"]');
                    if (newPanel) {
                        newPanel.classList.add('active');
                        requestAnimationFrame(function () {
                            newPanel.classList.add('vs-panel-visible');
                            newPanel.querySelectorAll('.vs-carousel-track').forEach(function (track) {
                                activateCarousel(track);
                            });
                        });
                    }
                }, 450);
            }
        });
    });

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var carouselStates = new Map();

    function prepareCarousel(track) {
        if (carouselStates.has(track)) { return carouselStates.get(track); }

        var inner = track.querySelector('.vs-carousel-inner');
        var originalThumbs = Array.from(inner.children);
        if (originalThumbs.length === 0) { return null; }

        originalThumbs.forEach(function (thumb) {
            var clone = thumb.cloneNode(true);
            clone.setAttribute('aria-hidden', 'true');
            clone.setAttribute('tabindex', '-1');
            inner.appendChild(clone);
        });

        var state = {
            inner: inner,
            firstOriginal: originalThumbs[0],
            cloneStartIndex: originalThumbs.length,
            setWidth: 0,
            offset: 0,
            speed: 0.6,
            paused: false,
            resumeTimer: null,
            started: false
        };
        carouselStates.set(track, state);
        return state;
    }

    function applyOffset(state) {
        state.inner.style.transform = 'translateX(' + (-state.offset) + 'px)';
    }

    function stepCarousel(state) {
        if (!state.paused && state.setWidth > 0) {
            state.offset += state.speed;
            if (state.offset >= state.setWidth) {
                state.offset -= state.setWidth;
            }
            applyOffset(state);
        }
        requestAnimationFrame(function () { stepCarousel(state); });
    }

    function activateCarousel(track) {
        var state = prepareCarousel(track);
        if (!state || state.started) { return; }

        var firstClone = state.inner.children[state.cloneStartIndex];
        state.setWidth = firstClone.offsetLeft - state.firstOriginal.offsetLeft;
        if (state.setWidth <= 0) { return; }

        state.started = true;

        track.addEventListener('mouseenter', function () { state.paused = true; });
        track.addEventListener('mouseleave', function () { state.paused = false; });
        track.addEventListener('focusin', function () { state.paused = true; });
        track.addEventListener('focusout', function () { state.paused = false; });

        var carousel = track.closest('.vs-carousel');
        carousel.querySelectorAll('.vs-carousel-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var sampleWidth = state.firstOriginal.offsetWidth + 18;
                var dir = btn.getAttribute('data-dir') === 'next' ? 1 : -1;

                state.offset += dir * sampleWidth * 2;
                state.offset = ((state.offset % state.setWidth) + state.setWidth) % state.setWidth;

                state.inner.classList.add('vs-carousel-jump');
                applyOffset(state);
                setTimeout(function () { state.inner.classList.remove('vs-carousel-jump'); }, 460);

                state.paused = true;
                clearTimeout(state.resumeTimer);
                state.resumeTimer = setTimeout(function () { state.paused = false; }, 2500);
            });
        });

        if (!reduceMotion) {
            requestAnimationFrame(function () { stepCarousel(state); });
        }
    }

    document.querySelectorAll('.vs-carousel-track').forEach(function (track) {
        prepareCarousel(track);
    });

    document.querySelectorAll('.vs-panel.active .vs-carousel-track').forEach(function (track) {
        activateCarousel(track);
    });

    var lightbox = document.getElementById('vsLightbox');
    var lightboxImg = document.getElementById('vsLightboxImg');
    var currentCategory = null;
    var currentIndex = 0;

    function updateLightboxImage() {
        var arr = window.vsGalleryData[currentCategory] || [];
        if (!arr.length) { return; }
        lightboxImg.src = arr[currentIndex];
        lightboxImg.alt = currentCategory + ' photo ' + (currentIndex + 1);
    }

    function openLightbox(category, index) {
        currentCategory = category;
        currentIndex = index;
        updateLightboxImage();
        lightbox.classList.add('open');
    }

    function closeLightbox() {
        lightbox.classList.remove('open');
    }

    function nextImage() {
        var arr = window.vsGalleryData[currentCategory] || [];
        if (!arr.length) { return; }
        currentIndex = (currentIndex + 1) % arr.length;
        updateLightboxImage();
    }

    function prevImage() {
        var arr = window.vsGalleryData[currentCategory] || [];
        if (!arr.length) { return; }
        currentIndex = (currentIndex - 1 + arr.length) % arr.length;
        updateLightboxImage();
    }

    document.querySelectorAll('.vs-photo-thumb').forEach(function (thumb) {
        thumb.addEventListener('click', function () {
            openLightbox(thumb.getAttribute('data-category'), parseInt(thumb.getAttribute('data-index'), 10));
        });
        thumb.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                thumb.click();
            }
        });
    });

    document.getElementById('vsLightboxClose').addEventListener('click', closeLightbox);
    document.getElementById('vsLightboxNext').addEventListener('click', nextImage);
    document.getElementById('vsLightboxPrev').addEventListener('click', prevImage);

    lightbox.addEventListener('click', function (e) {
        if (e.target === lightbox) { closeLightbox(); }
    });

    document.addEventListener('keydown', function (e) {
        if (!lightbox.classList.contains('open')) { return; }
        if (e.key === 'Escape') { closeLightbox(); }
        if (e.key === 'ArrowRight') { nextImage(); }
        if (e.key === 'ArrowLeft') { prevImage(); }
    });
});