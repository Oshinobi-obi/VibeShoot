document.addEventListener('DOMContentLoaded', function () {

    const currentPhotographerSlug = window.currentPhotographerSlug || "ginger-snaps";
    let scheduleData = { bookings: [], blockedDates: [] };

    const qrCodePaths = {
        "ginger-snaps": "/Uploads/QRCodes/GingerSnaps/GSGCash.png",
        "chiyos-folder": "/Uploads/QRCodes/ChiyosFolder/CFGCash.png",
        "sulyap-films": "/Uploads/QRCodes/SulyapFilms/SFGCash.png"
    };

    // 1. Generate 1-Hour Time Slots (With explicit dark styling to prevent white backgrounds)
    const timeSlotInput = document.getElementById('timeSlotInput');
    const endTimeDisplay = document.getElementById('endTimeDisplay');
    const endTimeInput = document.getElementById('endTimeInput');

    if (timeSlotInput) {
        for (let h = 8; h <= 20; h++) {
            let ampm = h >= 12 ? 'PM' : 'AM';
            let dispH = h % 12 || 12;
            let valH = String(h).padStart(2, '0');

            let option = document.createElement('option');
            option.value = `${valH}:00`;
            option.text = `${dispH}:00 ${ampm}`;
            option.style.background = "#161619"; // Forces dark background on dropdown items
            option.style.color = "white";
            timeSlotInput.appendChild(option);
        }
        timeSlotInput.addEventListener('change', calculateEndTime);
    }

    // 2. Package Definitions
    const allPackages = {
        "ginger-snaps": {
            "Birthday": [
                { id: "bday-basic", name: "Basic Package", price: "₱2,999", duration: 3, details: ["1 Photographer", "1 Assistant", "2-3hrs. Photo Coverage", "Unlimited Shots", "150 minimum Photos", "7-9 Days Editing Process"] },
                { id: "bday-combo", name: "Combo Package", price: "₱6,499", duration: 3, details: ["1 Photographer, 1 Videographer, 1 Assistant", "2-3hrs. Photo & Video Coverage", "Unlimited Shots", "3-5 min. Video Highlights", "150 minimum Photos", "7-9 Days Editing Process"] }
            ],
            "Baptism": [
                { id: "bap-basic", name: "Basic Package", price: "₱2,999", duration: 3, details: ["1 Photographer", "1 Assistant", "2-3hrs. Photo Coverage", "Unlimited Shots", "150 minimum Photos", "7-9 Days Editing Process"] },
                { id: "bap-combo", name: "Combo Package", price: "₱6,499", duration: 3, details: ["1 Photographer, 1 Videographer, 1 Assistant", "2-3hrs. Photo & Video Coverage", "Unlimited Shots", "3-5 min. Video Highlights", "150 minimum Photos", "7-9 Days Editing Process"] }
            ],
            "Photoshoot": [
                { id: "photo-classic", name: "Classic Package", price: "₱3,499", duration: 2, details: ["1 Photographer", "1 Assistant", "1 Location", "2hrs. Photo Session", "Unlimited Shots", "50 minimum Composed Edited Photos", "1-2 Weeks Editing Process"] },
                { id: "photo-deluxe", name: "Deluxe Package", price: "₱7,499", duration: 2, details: ["1 Photographer, 1 Videographer, 1 Assistant", "1 Location", "2hrs. Photo & Video Session", "Unlimited Shots", "2-4 min. Video Shoot", "50 minimum Composed Edited Photos", "1-2 Weeks Editing Process"] }
            ],
            "Wedding": [
                { id: "wed-tbd", name: "Wedding Package", price: "Custom", duration: 5, details: ["Packages for Weddings are currently custom tailored.", "Please submit this form and we will contact you for a quote!"] }
            ]
        },
        "chiyos-folder": {
            "Birthday": [{ id: "cf-tbd", name: "Package Details", price: "TBD", duration: 3, details: ["Details coming soon!"] }],
            "Baptism": [{ id: "cf-tbd", name: "Package Details", price: "TBD", duration: 3, details: ["Details coming soon!"] }],
            "Photoshoot": [{ id: "cf-tbd", name: "Package Details", price: "TBD", duration: 3, details: ["Details coming soon!"] }],
            "Wedding": [{ id: "cf-tbd", name: "Package Details", price: "TBD", duration: 5, details: ["Details coming soon!"] }]
        },
        "sulyap-films": {
            "Birthday": [{ id: "sf-tbd", name: "Package Details", price: "TBD", duration: 3, details: ["Details coming soon!"] }],
            "Baptism": [{ id: "sf-tbd", name: "Package Details", price: "TBD", duration: 3, details: ["Details coming soon!"] }],
            "Photoshoot": [{ id: "sf-tbd", name: "Package Details", price: "TBD", duration: 3, details: ["Details coming soon!"] }],
            "Wedding": [{ id: "sf-tbd", name: "Package Details", price: "TBD", duration: 5, details: ["Details coming soon!"] }]
        }
    };
    const currentPackages = allPackages[currentPhotographerSlug] || allPackages["ginger-snaps"];

    // 3. Dynamic End Time Calculator
    function calculateEndTime() {
        const timeVal = timeSlotInput.value;
        const selectedRadio = document.querySelector('input[name="SelectedPackage"]:checked');

        if (timeVal && selectedRadio) {
            const duration = parseInt(selectedRadio.getAttribute('data-duration')) || 2;
            const [h, m] = timeVal.split(':').map(Number);

            let endH = h + duration;

            // Database formatting (24 hr)
            endTimeInput.value = `${String(endH).padStart(2, '0')}:00`;

            // Visual formatting (12 hr)
            let ampm = endH >= 12 && endH < 24 ? 'PM' : 'AM';
            let dispH = endH % 12 || 12;
            endTimeDisplay.value = `${dispH}:00 ${ampm}`;
        } else {
            endTimeInput.value = "";
            endTimeDisplay.value = "";
        }
    }

    const phoneInput = document.getElementById('contactNumberInput');
    phoneInput.addEventListener('input', function (e) {
        let numbers = e.target.value.replace(/\D/g, '');
        let match = numbers.match(/(\d{0,4})(\d{0,3})(\d{0,4})/);
        if (!match[2]) e.target.value = match[1];
        else e.target.value = match[1] + '-' + match[2] + (match[3] ? '-' + match[3] : '');
    });

    const monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    const dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    const holidays = { "01-01": "New Year's", "12-25": "Christmas", "12-31": "NYE" };

    const today = new Date();
    let currentMonth = today.getMonth();
    let currentYear = today.getFullYear();
    const grid = document.getElementById('calendarGrid');
    const monthYearDisp = document.getElementById('monthYearDisplay');

    async function fetchClientSchedule() {
        try {
            const response = await fetch(`/photographer/${currentPhotographerSlug}/api/schedule`);
            scheduleData = await response.json();
            renderCalendar(currentMonth, currentYear);
        } catch (error) { console.error("Error fetching schedule:", error); }
    }

    function getStatusClass(year, month, day) {
        const m = String(month + 1).padStart(2, '0');
        const d = String(day).padStart(2, '0');
        const dateString = `${year}-${m}-${d}`;

        if (scheduleData.blockedDates.some(b => b.date.startsWith(dateString))) return 'status-blocked';

        const dayBookings = scheduleData.bookings.filter(b => b.targetDate.startsWith(dateString));
        if (dayBookings.length >= 2) return 'status-blocked';

        if (dayBookings.length === 1) {
            if (dayBookings[0].status === 'Pending') return 'status-pending';
            if (dayBookings[0].status === 'Confirmed') return 'status-confirmed';
        }
        return 'status-available';
    }

    function renderCalendar(month, year) {
        grid.innerHTML = "";
        monthYearDisp.innerText = monthNames[month] + " " + year;

        dayNames.forEach(day => {
            const header = document.createElement('div');
            header.className = 'vs-day-name';
            header.innerText = day;
            grid.appendChild(header);
        });

        const firstDay = new Date(year, month, 1).getDay();
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        for (let i = 0; i < firstDay; i++) grid.appendChild(document.createElement('div'));

        for (let i = 1; i <= daysInMonth; i++) {
            const cell = document.createElement('div');
            const cellDate = new Date(year, month, i);
            const isPast = cellDate.setHours(0, 0, 0, 0) < new Date().setHours(0, 0, 0, 0);

            let contentHTML = `<span class="vs-day-number">${i}</span>`;
            const holidayKey = `${String(month + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;

            if (isPast) {
                cell.className = 'vs-day-cell disabled';
            } else {
                const statusClass = getStatusClass(year, month, i);
                cell.className = `vs-day-cell ${statusClass}`;

                if (statusClass === 'status-available' || statusClass === 'status-pending' || statusClass === 'status-confirmed') {
                    cell.addEventListener('click', () => openModal(new Date(year, month, i)));
                } else {
                    cell.style.pointerEvents = 'none';
                    cell.style.opacity = '0.5';
                }
            }

            if (holidays[holidayKey]) {
                cell.classList.add('holiday');
                contentHTML += `<span class="vs-holiday-label">${holidays[holidayKey]}</span>`;
            }

            cell.innerHTML = contentHTML;
            grid.appendChild(cell);
        }
    }

    document.getElementById('prevMonth').addEventListener('click', () => { currentMonth--; if (currentMonth < 0) { currentMonth = 11; currentYear--; } renderCalendar(currentMonth, currentYear); });
    document.getElementById('nextMonth').addEventListener('click', () => { currentMonth++; if (currentMonth > 11) { currentMonth = 0; currentYear++; } renderCalendar(currentMonth, currentYear); });

    fetchClientSchedule();

    const modal = document.getElementById('bookingModal');
    const selectedDateDisplay = document.getElementById('selectedDateDisplay');
    const selectedDateInput = document.getElementById('selectedDateInput');
    const categorySelect = document.getElementById('categorySelect');
    const packagesWrapper = document.getElementById('packagesWrapper');
    const packagesContainer = document.getElementById('packagesContainer');
    const tcCheckbox = document.getElementById('tcCheckbox');
    const submitBtn = document.getElementById('submitBtn');
    const tcModal = document.getElementById('tcModal');

    function openModal(dateObj) {
        const formattedDate = dateObj.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
        selectedDateDisplay.innerText = "Target Date: " + formattedDate;
        selectedDateInput.value = formattedDate;

        document.getElementById('bookingForm').reset();
        timeSlotInput.value = '';
        endTimeDisplay.value = '';
        endTimeInput.value = '';
        packagesWrapper.style.display = 'none';
        packagesContainer.innerHTML = '';
        tcCheckbox.checked = false;
        submitBtn.disabled = true;

        modal.classList.add('open');
    }

    document.getElementById('closeModal').addEventListener('click', () => modal.classList.remove('open'));

    categorySelect.addEventListener('change', function () {
        const cat = this.value;
        packagesContainer.innerHTML = '';

        if (currentPackages[cat]) {
            currentPackages[cat].forEach((pkg, index) => {
                const bulletsHTML = pkg.details.map(d => `<li>${d}</li>`).join('');
                const pureNumberPrice = pkg.price.replace(/[^\d]/g, '');

                const card = document.createElement('label');
                card.className = 'vs-package-card';
                card.innerHTML = `
                    <div class="vs-package-header">
                        <div style="display:flex; align-items:center; gap: 8px;">
                            <input type="radio" name="SelectedPackage" value="${pkg.name}" data-price="${pureNumberPrice}" data-duration="${pkg.duration}" required ${index === 0 ? 'checked' : ''} style="width: auto; margin:0;" />
                            <span class="vs-package-name">${pkg.name}</span>
                        </div>
                        <span class="vs-package-price">${pkg.price}</span>
                    </div>
                    <ul class="vs-package-bullets">${bulletsHTML}</ul>
                `;

                card.querySelector('input').addEventListener('change', function () {
                    document.querySelectorAll('.vs-package-card').forEach(c => c.classList.remove('selected'));
                    if (this.checked) card.classList.add('selected');
                    calculateEndTime();
                });

                if (index === 0) card.classList.add('selected');
                packagesContainer.appendChild(card);
            });
            packagesWrapper.style.display = 'block';
            calculateEndTime(); // Recalculate immediately when category changes
        }
    });

    tcCheckbox.addEventListener('click', function (e) {
        if (this.checked) { e.preventDefault(); tcModal.classList.add('open'); }
        else { submitBtn.disabled = true; }
    });

    document.getElementById('openTcModal').addEventListener('click', function (e) { e.preventDefault(); tcModal.classList.add('open'); });
    document.getElementById('btnAgree').addEventListener('click', function () { tcCheckbox.checked = true; submitBtn.disabled = false; tcModal.classList.remove('open'); });
    document.getElementById('btnDecline').addEventListener('click', function () { tcCheckbox.checked = false; submitBtn.disabled = true; tcModal.classList.remove('open'); });

    const bookingForm = document.getElementById('bookingForm');
    const notifModal = document.getElementById('paymentNotifModal');
    const qrModal = document.getElementById('qrModal');
    const uploadModal = document.getElementById('uploadModal');
    const receiptModal = document.getElementById('receiptModal');
    let timerInterval;

    document.getElementById('btnProceedToQr').addEventListener('click', function () {
        const timeVal = timeSlotInput.value;
        if (!timeVal) {
            alert("Please select your preferred Start Time.");
            return;
        }

        notifModal.classList.remove('open');
        qrModal.classList.add('open');

        const selectedRadio = document.querySelector('input[name="SelectedPackage"]:checked');
        if (selectedRadio) {
            const fullPrice = parseFloat(selectedRadio.getAttribute('data-price')) || 0;
            if (fullPrice > 0) {
                document.getElementById('qrAmountDisplay').innerText = "₱" + (fullPrice / 2).toLocaleString('en-US', { minimumFractionDigits: 2 });
            } else {
                document.getElementById('qrAmountDisplay').innerText = "Custom / TBD";
            }
        }

        document.getElementById('gcashQrImage').src = qrCodePaths[currentPhotographerSlug] || "/GCash/GingerSnaps/GSGCash.png";

        const btnAlreadyPaid = document.getElementById('btnAlreadyPaid');
        const qrTimerText = document.getElementById('qrTimerText');
        const qrTimeDisplay = document.getElementById('qrTime');

        btnAlreadyPaid.style.display = 'none';
        qrTimerText.style.display = 'block';
        let timeLeft = 5;
        qrTimeDisplay.innerText = timeLeft;

        timerInterval = setInterval(() => {
            timeLeft--;
            qrTimeDisplay.innerText = timeLeft;
            if (timeLeft <= 0) {
                clearInterval(timerInterval);
                qrTimerText.style.display = 'none';
                btnAlreadyPaid.style.display = 'inline-block';
            }
        }, 1000);
    });

    bookingForm.addEventListener('submit', function (e) {
        e.preventDefault();
        modal.classList.remove('open');
        notifModal.classList.add('open');
    });

    document.getElementById('btnAlreadyPaid').addEventListener('click', function () {
        qrModal.classList.remove('open');
        uploadModal.classList.add('open');
    });

    document.getElementById('btnSubmitReceipt').addEventListener('click', function () {
        const receiptFile = document.getElementById('receiptFile');
        if (!receiptFile.files || receiptFile.files.length === 0) {
            alert("Please upload your GCash receipt screenshot first.");
            return;
        }

        document.getElementById('formReceiptFile').files = receiptFile.files;

        const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '');
        const randomDigits = Math.floor(10000000 + Math.random() * 90000000);
        const txId = dateStr + randomDigits;

        document.getElementById('recTxId').innerText = txId;
        document.getElementById('formTxId').value = txId;

        document.getElementById('recName').innerText = document.getElementById('inputFullName').value;
        document.getElementById('recVenue').innerText = document.getElementById('inputVenue').value;
        document.getElementById('recCategory').innerText = categorySelect.value;

        const timeVal = timeSlotInput.value;
        const [h, m] = timeVal.split(':').map(Number);
        let ampm = h >= 12 ? 'PM' : 'AM';
        let dispH = h % 12 || 12;
        let finalStartTime = `${dispH}:00 ${ampm}`;

        document.getElementById('recTime').innerText = `${finalStartTime} to ${endTimeDisplay.value}`;

        const selectedRadio = document.querySelector('input[name="SelectedPackage"]:checked');
        if (selectedRadio) {
            document.getElementById('recPackage').innerText = selectedRadio.value;
            const fullPrice = parseFloat(selectedRadio.getAttribute('data-price')) || 0;
            if (fullPrice > 0) {
                document.getElementById('recAmount').innerText = "₱" + (fullPrice / 2).toLocaleString('en-US', { minimumFractionDigits: 2 });
                document.getElementById('formAmountPaid').value = fullPrice / 2;
            } else {
                document.getElementById('recAmount').innerText = "Custom";
                document.getElementById('formAmountPaid').value = 0;
            }
        }

        uploadModal.classList.remove('open');
        receiptModal.classList.add('open');
    });

    document.getElementById('btnDownloadReceipt').addEventListener('click', function () {
        const btn = this;
        btn.innerText = "Downloading Receipt...";
        btn.disabled = true;

        const elementToPrint = document.getElementById('receiptContent');
        const opt = {
            margin: 0, filename: 'VibeShoot_Booking_Receipt.pdf',
            image: { type: 'jpeg', quality: 1 }, html2canvas: { scale: 2 },
            jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
        };

        html2pdf().set(opt).from(elementToPrint).save().then(() => {
            btn.innerText = "Receipt Downloaded!";
            setTimeout(() => { document.getElementById('bookingForm').submit(); }, 1500);
        });
    });
});