document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    var cfg = window.vsBooking || { slug: '', packages: [] };
    var OPEN_HOUR = 8, CLOSE_HOUR = 20, PREP_HOURS = 2;
    var MONTHS = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    var DOWS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    var HOLIDAYS = { '01-01': "New Year's", '12-25': 'Christmas', '12-31': 'NYE' };
    var CATEGORY_ORDER = ['Birthday', 'Baptism', 'Photoshoot', 'Wedding'];

    var $ = function (id) { return document.getElementById(id); };
    var peso = function (n) { return '₱' + Number(n).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); };
    var iso = function (d) { return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0'); };
    var fmtHour = function (h) { return (h % 12 || 12) + ':00 ' + (h >= 12 ? 'PM' : 'AM'); };
    var toHours = function (hhmm) { var p = hhmm.split(':'); return Number(p[0]) + Number(p[1]) / 60; };

    // ------------------------------------------------------------ Calendar
    var schedule = { days: {}, blocked: [], maxPerDay: 2 };
    var view = new Date(); view.setDate(1);
    var minMonth = new Date(view);
    var todayIso = iso(new Date());

    function load() {
        fetch('/photographer/' + encodeURIComponent(cfg.slug) + '/api/schedule')
            .then(function (r) { return r.json(); })
            .then(function (json) { schedule = json; renderCalendar(); })
            .catch(function () { renderCalendar(); });
    }

    function dayState(key) {
        if (key <= todayIso) return 'past';
        if (schedule.blocked.indexOf(key) >= 0) return 'off';
        var d = schedule.days[key];
        if (!d) return 'avail';
        if (d.count >= schedule.maxPerDay) return 'full';
        return 'few';
    }

    function renderCalendar() {
        var grid = $('calGrid');
        $('monthTitle').textContent = MONTHS[view.getMonth()] + ' ' + view.getFullYear();
        $('prevMonth').disabled = view <= minMonth;
        grid.innerHTML = '';
        DOWS.forEach(function (d) { var el = document.createElement('div'); el.className = 'vs-cal-dow'; el.textContent = d; grid.appendChild(el); });

        for (var i = 0; i < view.getDay(); i++) grid.appendChild(document.createElement('span'));

        var days = new Date(view.getFullYear(), view.getMonth() + 1, 0).getDate();
        for (var day = 1; day <= days; day++) {
            var date = new Date(view.getFullYear(), view.getMonth(), day);
            var key = iso(date);
            var state = dayState(key);
            var cell = document.createElement('button');
            cell.type = 'button';
            cell.className = 'vs-day ' + state + (key === todayIso ? ' today' : '');
            var tag = { avail: 'Open', few: '1 left', full: 'Full', off: 'Off', past: '' }[state];
            cell.innerHTML = '<span class="n">' + day + '</span><span class="dot"></span><span class="tag">' + tag + '</span>';
            var hol = HOLIDAYS[key.slice(5)];
            if (hol) { cell.classList.add('holiday'); cell.setAttribute('data-holiday', hol); }
            cell.setAttribute('aria-label', date.toDateString() + ' — ' + ({ avail: 'available', few: 'one slot left', full: 'fully booked', off: 'unavailable', past: 'not bookable' }[state]));
            if (state === 'avail' || state === 'few') {
                (function (k) { cell.addEventListener('click', function () { openWizard(k); }); })(key);
            } else {
                cell.disabled = true;
            }
            grid.appendChild(cell);
        }
    }

    $('prevMonth').addEventListener('click', function () { view.setMonth(view.getMonth() - 1); renderCalendar(); });
    $('nextMonth').addEventListener('click', function () { view.setMonth(view.getMonth() + 1); renderCalendar(); });

    // ------------------------------------------------------------ Wizard state
    var wizard = $('wizard');
    var form = $('bookingForm');
    var state = { date: null, category: null, pkg: null, start: null, step: 0, submitted: false };
    var steps = document.querySelectorAll('.vs-step');
    var categories = CATEGORY_ORDER.filter(function (c) { return cfg.packages.some(function (p) { return p.category === c; }); });

    function openWizard(dateKey) {
        if (state.submitted) resetWizard();
        state.date = dateKey;
        $('fDate').value = dateKey;
        if (!state.category && categories.length) state.category = categories[0];
        renderCategories();
        renderPackages();
        renderSlots();
        goTo(0);
        updateSummary();
        wizard.classList.add('open');
        document.body.style.overflow = 'hidden';
    }

    function closeWizard() {
        wizard.classList.remove('open');
        document.body.style.overflow = '';
        if (state.submitted) { resetWizard(); load(); }
    }

    function resetWizard() {
        form.reset();
        state = { date: null, category: categories[0] || null, pkg: null, start: null, step: 0, submitted: false };
        $('dropContent').innerHTML = '<b>Upload GCash receipt *</b>Tap to choose a screenshot, or drop it here';
        $('wzFoot').style.display = '';
    }

    wizard.addEventListener('click', function (e) {
        if (e.target === wizard || e.target.closest('[data-close]')) closeWizard();
    });
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        if ($('termsModal').classList.contains('open')) $('termsModal').classList.remove('open');
        else if (wizard.classList.contains('open')) closeWizard();
    });

    function renderCategories() {
        var host = $('catChips');
        host.innerHTML = '';
        categories.forEach(function (c) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'vs-chip' + (c === state.category ? ' on' : '');
            b.textContent = c;
            b.addEventListener('click', function () {
                state.category = c;
                state.pkg = null;
                renderCategories(); renderPackages(); renderSlots(); updateSummary();
            });
            host.appendChild(b);
        });
    }

    function renderPackages() {
        var host = $('pkgList');
        host.innerHTML = '';
        var list = cfg.packages.filter(function (p) { return p.category === state.category; });
        if (!state.pkg && list.length) state.pkg = list[0];
        list.forEach(function (p) {
            var card = document.createElement('button');
            card.type = 'button';
            card.className = 'vs-pkg' + (state.pkg && state.pkg.id === p.id ? ' on' : '');
            var items = p.inclusions.map(function (i) { var li = document.createElement('li'); li.textContent = i; return li.outerHTML; }).join('');
            var name = document.createElement('b'); name.textContent = p.name;
            card.innerHTML = name.outerHTML +
                '<div class="price">' + (p.price > 0 ? peso(p.price).replace('.00', '') : 'Custom quote') + '</div>' +
                '<div class="dur">' + p.hours + '-hour coverage' + (p.price > 0 ? ' · ' + peso(p.price / 2) + ' down' : '') + '</div>' +
                '<ul>' + items + '</ul>';
            card.addEventListener('click', function () {
                state.pkg = p;
                renderPackages(); renderSlots(); updateSummary();
            });
            host.appendChild(card);
        });
        $('fPackage').value = state.pkg ? state.pkg.id : '';
    }

    function renderSlots() {
        var host = $('slotList');
        host.innerHTML = '';
        if (!state.pkg) return;
        var dur = state.pkg.hours;
        var sessions = (schedule.days[state.date] && schedule.days[state.date].sessions) || [];
        var anyFree = false;

        for (var h = OPEN_HOUR; h + dur <= CLOSE_HOUR; h++) {
            var start = h, end = h + dur;
            var clash = sessions.some(function (s) {
                var ss = toHours(s.start), se = toHours(s.end);
                return !(start >= se + PREP_HOURS || end + PREP_HOURS <= ss);
            });
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'vs-slot';
            var value = String(h).padStart(2, '0') + ':00';
            if (state.start === value && !clash) b.classList.add('on');
            b.innerHTML = fmtHour(h) + '<small>until ' + fmtHour(end) + '</small>';
            b.disabled = clash;
            if (!clash) anyFree = true;
            (function (v) {
                b.addEventListener('click', function () { state.start = v; renderSlots(); updateSummary(); });
            })(value);
            host.appendChild(b);
        }
        if (state.start && !host.querySelector('.vs-slot.on')) state.start = null;
        $('fStart').value = state.start || '';
        $('slotHelp').textContent = anyFree
            ? 'Operating hours 8:00 AM – 8:00 PM. Crossed-out times are too close to another session (2-hour prep interval).'
            : 'No start times fit this package on this date. Try a shorter package or another day.';
    }

    function updateSummary() {
        var d = state.date ? new Date(state.date + 'T00:00:00') : null;
        $('sumDate').firstChild.textContent = d ? d.toLocaleDateString('en-US', { weekday: 'short', month: 'long', day: 'numeric', year: 'numeric' }) : '—';
        $('sumTime').textContent = state.start && state.pkg ? fmtHour(toHours(state.start)) + ' – ' + fmtHour(toHours(state.start) + state.pkg.hours) : 'Choose a time';
        $('sumCat').textContent = state.category || '—';
        $('sumPkg').textContent = state.pkg ? state.pkg.name : '—';
        $('sumDur').textContent = state.pkg ? state.pkg.hours + ' hours' : '—';
        var price = state.pkg ? state.pkg.price : 0;
        $('sumPrice').textContent = state.pkg ? (price > 0 ? peso(price) : 'Custom quote') : '—';
        $('sumDue').textContent = state.pkg ? (price > 0 ? peso(price / 2) : '₱0.00') : '—';
        $('payAmount').textContent = peso(price / 2);
        $('wzTitle').textContent = d ? 'Book ' + d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) : 'Book your session';
    }

    // ------------------------------------------------------------ Step navigation
    function isQuote() { return state.pkg && state.pkg.price <= 0; }

    function goTo(step) {
        state.step = step;
        steps.forEach(function (s) { s.classList.toggle('active', Number(s.getAttribute('data-step')) === step); });
        document.querySelectorAll('#stepper > div').forEach(function (el, i) {
            el.classList.toggle('done', i < step);
            el.classList.toggle('cur', i === step);
        });
        $('btnBack').style.visibility = step === 0 ? 'hidden' : 'visible';
        $('btnNext').innerHTML = step === 2 ? (isQuote() ? 'Submit request' : 'Submit booking') : 'Continue';
        $('payOnline').style.display = isQuote() ? 'none' : '';
        $('payQuote').style.display = isQuote() ? '' : 'none';
        $('wzError').textContent = '';
        $('wzFoot').style.display = step === 3 ? 'none' : '';
        document.querySelector('.vs-wz-body').scrollTop = 0;
    }

    function fail(msg, fieldId) {
        $('wzError').textContent = msg;
        if (fieldId) {
            var f = $(fieldId);
            f.closest('.vs-field') && f.closest('.vs-field').classList.add('invalid');
            f.focus();
        }
        return false;
    }

    function validate(step) {
        document.querySelectorAll('.vs-field.invalid').forEach(function (el) { el.classList.remove('invalid'); });
        if (step === 0) {
            if (!state.pkg) return fail('Please choose a package.');
            if (!state.start) return fail('Please choose a start time.');
        }
        if (step === 1) {
            if (!$('fName').value.trim()) return fail('Please enter your full name.', 'fName');
            if (!/^09\d{9}$/.test($('fMobile').value.replace(/\D/g, ''))) return fail('Enter a valid mobile number (09XX-XXX-XXXX).', 'fMobile');
            if ($('fEmail').value && !$('fEmail').checkValidity()) return fail('Please check your email address.', 'fEmail');
            if (!$('fVenue').value.trim()) return fail('Please enter the venue.', 'fVenue');
            if (!$('fTerms').checked) return fail('Please accept the Terms & Conditions.');
        }
        if (step === 2 && !isQuote()) {
            if (!/^\d{8,20}$/.test($('fRef').value.replace(/\s/g, ''))) return fail('Enter the GCash reference number (digits only).', 'fRef');
            if (!$('fReceipt').files.length) return fail('Please upload your GCash receipt screenshot.');
        }
        return true;
    }

    $('btnBack').addEventListener('click', function () { if (state.step > 0) goTo(state.step - 1); });
    $('btnNext').addEventListener('click', function () {
        if (!validate(state.step)) return;
        if (state.step < 2) goTo(state.step + 1);
        else submit();
    });

    function submit() {
        var btn = $('btnNext');
        btn.disabled = true;
        btn.innerHTML = '<span class="spin"></span>Sending…';

        var data = new FormData(form);
        data.set('AcceptedTerms', $('fTerms').checked ? 'true' : 'false');
        if (isQuote()) { data.delete('ReceiptImage'); data.delete('ReferenceNumber'); }

        // Shrink the GCash screenshot first so the upload (and the database) stays small.
        var receipt = !isQuote() && $('fReceipt').files[0];
        var ready = receipt && window.vsShrinkImage
            ? window.vsShrinkImage(receipt).then(function (f) { data.set('ReceiptImage', f, f.name); })
            : Promise.resolve();

        ready.then(function () {
            return fetch('/photographer/' + encodeURIComponent(cfg.slug) + '/api/book', {
                method: 'POST',
                body: data,
                headers: { 'RequestVerificationToken': form.querySelector('input[name="__RequestVerificationToken"]').value }
            });
        })
            .then(function (r) { return r.json().catch(function () { return { ok: false, error: 'Something went wrong. Please try again.' }; }); })
            .then(function (res) {
                btn.disabled = false;
                if (!res.ok) {
                    goTo(state.step);
                    $('wzError').textContent = res.error || 'Booking failed. Please try again.';
                    return;
                }
                state.submitted = true;
                $('doneTx').textContent = res.transactionId;

                // Sound + remember the booking so this browser can announce the confirmation later.
                if (window.vsSound) window.vsSound(isQuote() ? 'quote-sent' : 'booking-sent');
                var token = decodeURIComponent((res.receiptUrl.split('t=')[1] || '').split('&')[0]);
                if (window.vsRememberBooking) window.vsRememberBooking(res.transactionId, token, 'Pending');
                var canAsk = window.vsNotifySupported && window.vsNotifySupported() && Notification.permission === 'default';
                $('notifyCard').hidden = !canAsk;
                if (window.vsNotifySupported && window.vsNotifySupported() && Notification.permission === 'granted') window.vsAskNotifications();
                $('receiptLink').href = res.receiptUrl;
                $('doneText').textContent = isQuote()
                    ? 'Your request is in! The photographer will reach out with a quotation for your custom package.'
                    : 'Your booking is now pending. The photographer will verify your GCash payment and confirm your slot.';
                goTo(3);
            })
            .catch(function () {
                btn.disabled = false;
                goTo(state.step);
                $('wzError').textContent = 'Network error — please check your connection and try again.';
            });
    }

    // ------------------------------------------------------------ Inputs
    $('fMobile').addEventListener('input', function (e) {
        var n = e.target.value.replace(/\D/g, '').slice(0, 11);
        e.target.value = n.length > 7 ? n.slice(0, 4) + '-' + n.slice(4, 7) + '-' + n.slice(7) : n.length > 4 ? n.slice(0, 4) + '-' + n.slice(4) : n;
    });

    var drop = $('dropZone');
    var fileInput = $('fReceipt');
    function showReceipt() {
        var f = fileInput.files[0];
        if (!f) return;
        if (!/^image\//.test(f.type)) { fileInput.value = ''; $('wzError').textContent = 'Please choose an image file.'; return; }
        var img = document.createElement('img');
        img.src = URL.createObjectURL(f);
        img.alt = 'Receipt preview';
        var label = document.createElement('b');
        label.textContent = f.name;
        $('dropContent').innerHTML = '';
        $('dropContent').appendChild(img);
        $('dropContent').appendChild(label);
        $('dropContent').appendChild(document.createTextNode('Tap to change'));
    }
    fileInput.addEventListener('change', showReceipt);
    ['dragenter', 'dragover'].forEach(function (ev) { drop.addEventListener(ev, function (e) { e.preventDefault(); drop.classList.add('drag'); }); });
    ['dragleave', 'drop'].forEach(function (ev) { drop.addEventListener(ev, function (e) { e.preventDefault(); drop.classList.remove('drag'); }); });
    drop.addEventListener('drop', function (e) { if (e.dataTransfer.files.length) { fileInput.files = e.dataTransfer.files; showReceipt(); } });

    function copy(text, btn) {
        if (navigator.clipboard) navigator.clipboard.writeText(text);
        var old = btn.textContent;
        btn.textContent = 'Copied!';
        setTimeout(function () { btn.textContent = old; }, 1400);
    }
    $('copyAmount').addEventListener('click', function () { copy(state.pkg ? (state.pkg.price / 2).toFixed(2) : '', this); });
    $('copyTx').addEventListener('click', function () { copy($('doneTx').textContent, this); });
    $('btnNotify').addEventListener('click', function () {
        var btn = this;
        window.vsAskNotifications().then(function (ok) {
            $('notifyCard').innerHTML = ok
                ? '<span><b>Notifications are on.</b> Keep VibeShoot open in a tab and we will let you know.</span>'
                : '<span>Notifications are blocked in your browser. You can still check your status anytime on Track Booking.</span>';
        });
    });

    // ------------------------------------------------------------ Terms
    var terms = $('termsModal');
    $('openTerms').addEventListener('click', function (e) { e.preventDefault(); terms.classList.add('open'); });
    $('fTerms').addEventListener('click', function (e) {
        if (this.checked) { e.preventDefault(); terms.classList.add('open'); }
    });
    $('termsAgree').addEventListener('click', function () { $('fTerms').checked = true; terms.classList.remove('open'); $('wzError').textContent = ''; });
    $('termsDecline').addEventListener('click', function () { $('fTerms').checked = false; terms.classList.remove('open'); });
    terms.addEventListener('click', function (e) { if (e.target === terms) terms.classList.remove('open'); });

    load();
});
