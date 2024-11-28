//Handle automatic scrolling
document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get("scrollToTable").toLowerCase() === "true") {
        const table = document.getElementById("employeesTable");
        if (table) {
            table.scrollIntoView({ behavior: "instant" });
            // Remove the scrollToTable parameter
            urlParams.delete("scrollToTable");
            const newUrl = `${window.location.pathname}?${urlParams.toString()}`;
            history.replaceState(null, "", newUrl);
        }
    }
});

function confirmDelete(employeeId) {
    let employeeEmail = document.getElementById(`employeeEmail-${employeeId}`).innerHTML;

    // Show the confirmation modal
    showConfirmationModal(
        `Are you sure you want to delete employee with email ${employeeEmail}?`,
        () => document.getElementById(`deleteForm-${employeeId}`).submit(), // Resubmit the form upon confirmation
        "btn-danger",
        "Delete"
    );
}

function confirmDeleteSelected() {
    // Show the confirmation modal
    showConfirmationModal(
        `Are you sure you want to delete selected employees?`,
        () => {
            const checkboxes = document.querySelectorAll(".employee-checkbox:checked");
            checkboxes.forEach((checkbox) => {
                let employeeRow = checkbox.parentElement.parentElement;
                let employeeDeleteForm = employeeRow.querySelector("[id^='deleteForm']");

                employeeDeleteForm.submit();
            });
        }, // Resubmit the form upon confirmation
        "btn-danger",
        "Delete"
    );
}