document.addEventListener("DOMContentLoaded", () => {
    const confirmationModal = new bootstrap.Modal(document.getElementById("confirmationModal"));
    const confirmationMessage = document.getElementById("confirmationMessage");
    const confirmActionButton = document.getElementById("confirmActionButton");

    window.showConfirmationModal = (message, onConfirm, confirmButtonClass = "btn-danger", confirmButtonText = "Confirm") => {
        confirmationMessage.textContent = message;

        confirmActionButton.className = `btn ${confirmButtonClass}`;
        confirmActionButton.textContent = confirmButtonText;

        confirmActionButton.onclick = null;
        confirmActionButton.onclick = () => {
            confirmationModal.hide();
            if (typeof onConfirm === "function") {
                onConfirm();
            }
        };

        confirmationModal.show();
    };
});
