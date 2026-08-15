document.addEventListener('DOMContentLoaded', function () {

    const currentPhotographerSlug = window.currentPhotographerSlug || "ginger-snaps";

    const qrCodePaths = {
        "ginger-snaps": "/Uploads/QRCodes/GingerSnaps/GSGCash.png",
        "chiyos-folder": "/Uploads/QRCodes/ChiyosFolder/CFGCash.png",
        "sulyap-films": "/Uploads/QRCodes/SulyapFilms/SFGCash.png"
    };

    const phoneInput = document.getElementById('contactNumberInput');
    phoneInput.addEventListener('input', function (e) {
        let numbers = e.target.value.replace(/\D/g, '');
        let match = numbers.match(/(\d{0,4})(\d{0,3})(\d{0,4})/);

        if (!match[2]) {
            e.target.value = match[1];
        } else {
            e.target.value = match[1] + '-' + match[2] + (match[3] ? '-' + match[3] : '');
        }
    });

    const monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    const dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    const holidays = {
        "01-01": "New Year's",
        "02-25": "EDSA Revolution",
        "04-09": "Araw ng Kagitingan",
        "05-01": "Labor Day",
        "06-12": "Independence Day",
        "08-21": "Ninoy Aquino Day",
        "11-01": "All Saints' Day",
        "11-30": "Bonifacio Day",
        "12-25": "Christmas Day",
        "12-30": "Rizal Day",
        "12-31": "New Year's Eve"
    };

    const today = new Date();
    let currentMonth = today.getMonth();
    let currentYear = today.getFullYear();

    const grid = document.getElementById('calendarGrid');
    const monthYearDisp = document.getElementById('monthYearDisplay');

    function renderCalendar(month, year) {
        grid.innerHTML = "";
        monthYearDisp.innerText = monthNames[month] + " " + year;

        dayNames.forEach(day => {
            const dayHeader = document.createElement('div');
            dayHeader.className = 'vs-day-name';
            dayHeader.innerText = day;
            grid.appendChild(dayHeader);
        });

        const firstDay = new Date(year, month, 1).getDay();
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        for (let i = 0; i < firstDay; i++) {
            const empty = document.createElement('div');
            grid.appendChild(empty);
        }

        for (let i = 1; i <= daysInMonth; i++) {
            const cell = document.createElement('div');
            cell.className = 'vs-day-cell';

            const cellDate = new Date(year, month, i);

            const isPast = cellDate.setHours(0, 0, 0, 0) < new Date().setHours(0, 0, 0, 0);
            if (isPast) {
                cell.classList.add('disabled');
            } else {
                cell.addEventListener('click', () => openModal(new Date(year, month, i)));
            }

            const monthStr = String(month + 1).padStart(2, '0');
            const dayStr = String(i).padStart(2, '0');
            const holidayKey = `${monthStr}-${dayStr}`;

            let contentHTML = `<span class="vs-day-number">${i}</span>`;
            if (holidays[holidayKey]) {
                cell.classList.add('holiday');
                contentHTML += `<span class="vs-holiday-label">${holidays[holidayKey]}</span>`;
            }

            cell.innerHTML = contentHTML;
            grid.appendChild(cell);
        }
    }

    document.getElementById('prevMonth').addEventListener('click', () => {
        currentMonth--;
        if (currentMonth < 0) { currentMonth = 11; currentYear--; }
        renderCalendar(currentMonth, currentYear);
    });

    document.getElementById('nextMonth').addEventListener('click', () => {
        currentMonth++;
        if (currentMonth > 11) { currentMonth = 0; currentYear++; }
        renderCalendar(currentMonth, currentYear);
    });

    renderCalendar(currentMonth, currentYear);

    const modal = document.getElementById('bookingModal');
    const selectedDateDisplay = document.getElementById('selectedDateDisplay');
    const selectedDateInput = document.getElementById('selectedDateInput');
    const categorySelect = document.getElementById('categorySelect');
    const packagesWrapper = document.getElementById('packagesWrapper');
    const packagesContainer = document.getElementById('packagesContainer');

    const tcCheckbox = document.getElementById('tcCheckbox');
    const submitBtn = document.getElementById('submitBtn');
    const tcModal = document.getElementById('tcModal');

    const allPackages = {
        "ginger-snaps": {
            "Birthday": [
                { id: "bday-basic", name: "Basic Package", price: "₱2,999", details: ["1 Photographer", "1 Assistant", "2-3hrs. Photo Coverage", "Unlimited Shots", "150 minimum Photos", "7-9 Days Editing Process"] },
                { id: "bday-combo", name: "Combo Package", price: "₱6,499", details: ["1 Photographer, 1 Videographer, 1 Assistant", "2-3hrs. Photo & Video Coverage", "Unlimited Shots", "3-5 min. Video Highlights", "150 minimum Photos", "7-9 Days Editing Process"] }
            ],
            "Baptism": [
                { id: "bap-basic", name: "Basic Package", price: "₱2,999", details: ["1 Photographer", "1 Assistant", "2-3hrs. Photo Coverage", "Unlimited Shots", "150 minimum Photos", "7-9 Days Editing Process"] },
                { id: "bap-combo", name: "Combo Package", price: "₱6,499", details: ["1 Photographer, 1 Videographer, 1 Assistant", "2-3hrs. Photo & Video Coverage", "Unlimited Shots", "3-5 min. Video Highlights", "150 minimum Photos", "7-9 Days Editing Process"] }
            ],
            "Photoshoot": [
                { id: "photo-classic", name: "Classic Package", price: "₱3,499", details: ["1 Photographer", "1 Assistant", "1 Location", "2hrs. Photo Session", "Unlimited Shots", "50 minimum Composed Edited Photos", "1-2 Weeks Editing Process"] },
                { id: "photo-deluxe", name: "Deluxe Package", price: "₱7,499", details: ["1 Photographer, 1 Videographer, 1 Assistant", "1 Location", "2hrs. Photo & Video Session", "Unlimited Shots", "2-4 min. Video Shoot", "50 minimum Composed Edited Photos", "1-2 Weeks Editing Process"] }
            ],
            "Wedding": [
                { id: "wed-tbd", name: "Wedding Package", price: "Custom", details: ["Packages for Weddings are currently custom tailored.", "Please submit this form and we will contact you for a quote!"] }
            ]
        },
        "chiyos-folder": {
            "Birthday": [{ id: "cf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Chiyos Folder coming soon!"] }],
            "Baptism": [{ id: "cf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Chiyos Folder coming soon!"] }],
            "Photoshoot": [{ id: "cf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Chiyos Folder coming soon!"] }],
            "Wedding": [{ id: "cf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Chiyos Folder coming soon!"] }]
        },
        "sulyap-films": {
            "Birthday": [{ id: "sf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Sulyap Films coming soon!"] }],
            "Baptism": [{ id: "sf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Sulyap Films coming soon!"] }],
            "Photoshoot": [{ id: "sf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Sulyap Films coming soon!"] }],
            "Wedding": [{ id: "sf-tbd", name: "Package Details", price: "TBD", details: ["Package details for Sulyap Films coming soon!"] }]
        }
    };

    const currentPackages = allPackages[currentPhotographerSlug] || allPackages["ginger-snaps"];

    function openModal(dateObj) {
        const formattedDate = dateObj.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
        selectedDateDisplay.innerText = "Target Date: " + formattedDate;
        selectedDateInput.value = formattedDate;

        document.getElementById('bookingForm').reset();
        packagesWrapper.style.display = 'none';
        packagesContainer.innerHTML = '';
        tcCheckbox.checked = false;
        submitBtn.disabled = true;

        modal.classList.add('open');
    }

    document.getElementById('closeModal').addEventListener('click', () => {
        modal.classList.remove('open');
    });

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
                            <input type="radio" name="SelectedPackage" value="${pkg.name}" data-price="${pureNumberPrice}" required ${index === 0 ? 'checked' : ''} style="width: auto; margin:0;" />
                            <span class="vs-package-name">${pkg.name}</span>
                        </div>
                        <span class="vs-package-price">${pkg.price}</span>
                    </div>
                    <ul class="vs-package-bullets">
                        ${bulletsHTML}
                    </ul>
                `;

                card.querySelector('input').addEventListener('change', function () {
                    document.querySelectorAll('.vs-package-card').forEach(c => c.classList.remove('selected'));
                    if (this.checked) card.classList.add('selected');
                });

                if (index === 0) card.classList.add('selected');
                packagesContainer.appendChild(card);
            });
            packagesWrapper.style.display = 'block';
        }
    });

    tcCheckbox.addEventListener('click', function (e) {
        if (this.checked) {
            e.preventDefault();
            tcModal.classList.add('open');
        } else {
            submitBtn.disabled = true;
        }
    });

    document.getElementById('openTcModal').addEventListener('click', function (e) {
        e.preventDefault();
        tcModal.classList.add('open');
    });

    document.getElementById('btnAgree').addEventListener('click', function () {
        tcCheckbox.checked = true;
        submitBtn.disabled = false;
        tcModal.classList.remove('open');
    });

    document.getElementById('btnDecline').addEventListener('click', function () {
        tcCheckbox.checked = false;
        submitBtn.disabled = true;
        tcModal.classList.remove('open');
    });

    const bookingForm = document.getElementById('bookingForm');
    const notifModal = document.getElementById('paymentNotifModal');
    const qrModal = document.getElementById('qrModal');
    const uploadModal = document.getElementById('uploadModal');
    const receiptModal = document.getElementById('receiptModal');

    let timerInterval;

    bookingForm.addEventListener('submit', function (e) {
        e.preventDefault();
        modal.classList.remove('open');
        notifModal.classList.add('open');
    });

    document.getElementById('btnProceedToQr').addEventListener('click', function () {
        notifModal.classList.remove('open');
        qrModal.classList.add('open');

        const selectedRadio = document.querySelector('input[name="SelectedPackage"]:checked');
        if (selectedRadio) {
            const fullPrice = parseFloat(selectedRadio.getAttribute('data-price')) || 0;
            if (fullPrice > 0) {
                const halfPrice = fullPrice / 2;
                document.getElementById('qrAmountDisplay').innerText = "₱" + halfPrice.toLocaleString('en-US', { minimumFractionDigits: 2 });
            } else {
                document.getElementById('qrAmountDisplay').innerText = "Custom / TBD";
            }
        }

        const qrImage = document.getElementById('gcashQrImage');
        qrImage.src = qrCodePaths[currentPhotographerSlug] || "/GCash/GingerSnaps/GSGCash.png";

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

        const formReceiptFile = document.getElementById('formReceiptFile');
        formReceiptFile.files = receiptFile.files;

        const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '');
        const randomDigits = Math.floor(10000000 + Math.random() * 90000000);
        const txId = dateStr + randomDigits;

        document.getElementById('recTxId').innerText = txId;
        document.getElementById('formTxId').value = txId;

        document.getElementById('recName').innerText = document.getElementById('inputFullName').value;
        document.getElementById('recVenue').innerText = document.getElementById('inputVenue').value;
        document.getElementById('recCategory').innerText = categorySelect.value;

        const selectedRadio = document.querySelector('input[name="SelectedPackage"]:checked');
        if (selectedRadio) {
            document.getElementById('recPackage').innerText = selectedRadio.value;
            const fullPrice = parseFloat(selectedRadio.getAttribute('data-price')) || 0;

            if (fullPrice > 0) {
                const halfPrice = fullPrice / 2;
                document.getElementById('recAmount').innerText = "₱" + halfPrice.toLocaleString('en-US', { minimumFractionDigits: 2 });
                document.getElementById('formAmountPaid').value = halfPrice;
            } else {
                document.getElementById('recAmount').innerText = "Custom / TBD";
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
        const receiptModalContent = document.querySelector('#receiptModal .vs-modal-content');
        const receiptModal = document.getElementById('receiptModal');

        receiptModal.style.alignItems = 'flex-start';
        receiptModal.style.paddingTop = '5px';
        receiptModalContent.style.maxHeight = 'none';
        receiptModalContent.style.overflow = 'visible';

        const pdfPage = document.createElement('div');
        pdfPage.id = 'receiptPdfPage';
        pdfPage.style.cssText = [
            'width: 816px',
            'height: 1056px',
            'box-sizing: border-box',
            'display: flex',
            'align-items: center',
            'justify-content: center',
            'background: #ffffff',
            'margin: 0',
            'padding: 40px'
        ].join(';');

        const receiptClone = elementToPrint.cloneNode(true);
        const receiptWidth = Math.round(elementToPrint.getBoundingClientRect().width);
        receiptClone.style.width = receiptWidth + 'px';
        receiptClone.style.maxWidth = '100%';
        receiptClone.style.boxSizing = 'border-box';
        receiptClone.style.margin = '0';

        pdfPage.appendChild(receiptClone);
        document.body.appendChild(pdfPage);

        const opt = {
            margin: 0,
            filename: 'VibeShoot_Booking_Receipt.pdf',
            image: { type: 'jpeg', quality: 1 },
            html2canvas: {
                scale: 2,
                backgroundColor: '#ffffff',
                scrollX: 0,
                scrollY: 0,
                useCORS: true
            },
            jsPDF: {
                unit: 'in',
                format: 'letter',
                orientation: 'portrait'
            }
        };

        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                html2pdf().set(opt).from(pdfPage).save().then(() => {
                    pdfPage.remove();

                    receiptModal.style.alignItems = 'center';
                    receiptModal.style.paddingTop = '0';
                    receiptModalContent.style.maxHeight = '85vh';
                    receiptModalContent.style.overflowY = 'auto';

                    btn.innerText = "Receipt Downloaded!";

                    setTimeout(() => {
                        document.getElementById('bookingForm').submit();
                    }, 1500);
                }).catch((error) => {
                    pdfPage.remove();

                    receiptModal.style.alignItems = 'center';
                    receiptModal.style.paddingTop = '0';
                    receiptModalContent.style.maxHeight = '85vh';
                    receiptModalContent.style.overflowY = 'auto';

                    btn.innerText = "Download Receipt";
                    btn.disabled = false;

                    console.error('Receipt PDF generation failed:', error);
                    alert('Unable to generate the receipt PDF. Please try again.');
                });
            });
        });
    });
});