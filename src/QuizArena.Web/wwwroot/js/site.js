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
