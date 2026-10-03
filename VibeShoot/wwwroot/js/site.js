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

    // ------------------------------------------------------------------ Sounds

    /**
     * Plays /audio/{name}.mp3. Browsers only allow sound after the visitor has clicked
     * on the site; if it is blocked, it plays on their next click/tap instead.
     */
    window.vsSound = function (name) {
        var audio = new Audio('/audio/' + name + '.mp3');
        audio.volume = 0.9;
        var p = audio.play();
        if (p && p.catch) {
            p.catch(function () {
                var retry = function () {
                    audio.play().catch(function () { });
                    window.removeEventListener('pointerdown', retry, true);
                    window.removeEventListener('keydown', retry, true);
                };
                window.addEventListener('pointerdown', retry, true);
                window.addEventListener('keydown', retry, true);
            });
        }
    };

    // ------------------------------------------------------------------ Booking notifications

    var STORE = 'vs-my-bookings';
    var FINAL = ['Confirmed', 'Completed', 'Declined', 'Cancelled'];

    function load() {
        try { return JSON.parse(localStorage.getItem(STORE) || '[]'); } catch (e) { return []; }
    }
    function save(list) {
        try { localStorage.setItem(STORE, JSON.stringify(list.slice(-10))); } catch (e) { }
    }

    window.vsNotifySupported = function () { return 'Notification' in window; };

    /** Remembers a booking in this browser so we can tell the client when it gets confirmed. */
    window.vsRememberBooking = function (id, token, status) {
        if (!id || !token) return;
        var list = load();
        var existing = list.filter(function (b) { return b.id === id; })[0];
        if (existing) { existing.t = token; }
        else { list.push({ id: id, t: token, status: status || 'Pending' }); }
        save(list);
    };

    /** Asks for notification permission (must be called from a click). Resolves true when allowed. */
    window.vsAskNotifications = function () {
        if (!window.vsNotifySupported()) return Promise.resolve(false);
        if (Notification.permission === 'granted') { startWatching(); return Promise.resolve(true); }
        return Notification.requestPermission().then(function (result) {
            if (result !== 'granted') return false;
            window.vsSound('notify-enabled');
            startWatching();
            return true;
        });
    };

    /** Called by a page that already knows a booking's current status (the receipt page). */
    window.vsBookingStatusSeen = function (id, status) {
        var list = load();
        var b = list.filter(function (x) { return x.id === id; })[0];
        if (!b) return;
        if (status === 'Confirmed' && !b.announced) {
            window.vsSound('booking-confirmed');
            b.announced = true;
        }
        b.status = status;
        save(list);
    };

    function showNotification(title, body, url) {
        if (!window.vsNotifySupported() || Notification.permission !== 'granted') return;
        try {
            var n = new Notification(title, { body: body, icon: '/favicon.svg', tag: url });
            n.onclick = function () { window.focus(); window.location.href = url; n.close(); };
        } catch (e) { /* some mobile browsers only allow notifications from a service worker */ }
    }

    var watching = false;
    function startWatching() {
        if (watching || !window.vsNotifySupported() || Notification.permission !== 'granted') return;
        watching = true;
        check();
        setInterval(check, 60000);
    }

    // Ask the server about every booking that is still waiting for a decision.
    function check() {
        load().filter(function (b) { return FINAL.indexOf(b.status) < 0; }).forEach(function (b) {
            fetch('/booking/' + encodeURIComponent(b.id) + '/status?t=' + encodeURIComponent(b.t), { cache: 'no-store' })
                .then(function (r) { return r.ok ? r.json() : null; })
                .then(function (s) {
                    if (!s) return;
                    var all = load();
                    var cur = all.filter(function (x) { return x.id === b.id; })[0];
                    if (!cur || cur.status === s.status) return;

                    var receipt = '/booking/' + encodeURIComponent(b.id) + '/receipt?t=' + encodeURIComponent(b.t);
                    if (s.status === 'Confirmed' && !cur.announced) {
                        showNotification('Your session is confirmed!', s.photographer + ' - ' + s.date + ', ' + s.time, receipt);
                        window.vsSound('booking-confirmed');
                        cur.announced = true;
                    } else if (s.status === 'Declined' || s.status === 'Cancelled') {
                        showNotification('Booking update', 'Your booking with ' + s.photographer + ' was ' + s.status.toLowerCase() + '. Tap to see details.', receipt);
                    }
                    cur.status = s.status;
                    save(all);
                })
                .catch(function () { });
        });
    }

    if (window.vsNotifySupported() && Notification.permission === 'granted' && load().length) startWatching();
})();
