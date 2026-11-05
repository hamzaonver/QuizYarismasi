// QuizArena - küçük arayüz yardımcıları

document.addEventListener("DOMContentLoaded", () => {
    // Silme gibi geri alınamaz işlemlerde onay iste: <form data-confirm="Emin misiniz?">
    document.querySelectorAll("form[data-confirm]").forEach(form => {
        form.addEventListener("submit", e => {
            if (!window.confirm(form.dataset.confirm)) e.preventDefault();
        });
    });

    // Soru formu: "Şık ekle" butonu gizli şık satırlarını sırayla açar
    const addBtn = document.getElementById("add-option");
    if (addBtn) {
        const updateButton = () => {
            const hidden = document.querySelectorAll(".option-row.is-hidden");
            addBtn.disabled = hidden.length === 0;
        };
        addBtn.addEventListener("click", () => {
            const next = document.querySelector(".option-row.is-hidden");
            if (next) {
                next.classList.remove("is-hidden");
                next.querySelector("input[type=text]")?.focus();
            }
            updateButton();
        });
        updateButton();
    }

    // Kayıt formu: rol "Öğretmen" seçilince davet kodu alanını göster
    const roleSelect = document.getElementById("Role");
    const inviteGroup = document.getElementById("invite-code-group");
    if (roleSelect && inviteGroup) {
        const toggle = () => inviteGroup.classList.toggle("d-none", roleSelect.value !== "Teacher");
        roleSelect.addEventListener("change", toggle);
        toggle();
    }
});

// ===== Hafta 7: Canlı oturum ekranları =====

// Oturum ekranı 3 saniyede bir sunucuya "durum ne?" diye soruyor.
// Durum (bekliyor/aktif/bitti) ya da soru değiştiyse sayfayı yeniliyor.
// Hafta 9'da bunun yerine WebSocket (SignalR) ile anlık bildirim gelecek.
function startStatePolling(stateUrl, currentStatus, currentQuestionIndex, onUpdate) {
    setInterval(async function () {
        try {
            const response = await fetch(stateUrl, { cache: "no-store" });
            if (!response.ok) return;

            const data = await response.json();

            if (data.status !== currentStatus || data.currentQuestionIndex !== currentQuestionIndex) {
                location.reload();
                return;
            }

            if (onUpdate) onUpdate(data);
        } catch (e) {
            // Bağlantı anlık koptuysa bir sonraki denemeyi bekle
        }
    }, 3000);
}

// Öğretmen ekranındaki katılımcı listesini günceller
function updateParticipantList(names) {
    const list = document.getElementById("participant-list");
    const count = document.getElementById("participant-count");
    const empty = document.getElementById("no-participants");
    if (!list) return;

    list.innerHTML = "";
    names.forEach(function (name) {
        const li = document.createElement("li");
        li.textContent = name; // textContent: isimdeki HTML kodu çalışmasın (XSS önlemi)
        list.appendChild(li);
    });

    if (count) count.textContent = names.length;
    if (empty) empty.classList.toggle("d-none", names.length > 0);
}
