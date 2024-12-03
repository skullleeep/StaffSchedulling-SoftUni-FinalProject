//Handle automatic scrolling
document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    let scrollToTableText = urlParams.get("scrollToTable");
    if (scrollToTableText !== null) {
        if (urlParams.get("scrollToTable").toLowerCase() === "true") {
            const table = document.getElementById("departmentsTable");
            if (table) {
                table.scrollIntoView({ behavior: "instant" });
                // Remove the scrollToTable parameter
                urlParams.delete("scrollToTable");
                const newUrl = `${window.location.pathname}?${urlParams.toString()}`;
                history.replaceState(null, "", newUrl);
            }
        }
    }
});

//Modal dialog functions
function confirmDelete(departmentId) {
    let departmentName = document.getElementById(`departmentName-${departmentId}`).innerHTML;

    // Show the confirmation modal
    showConfirmationModal(
        `Are you sure you want to delete department '${departmentName}'?`,
        () => document.getElementById(`deleteForm-${departmentId}`).submit(), // Resubmit the form upon confirmation
        "btn-danger",
        "Delete"
    );
}

function confirmDeleteAll() {
    // Show the confirmation modal
    showConfirmationModal(
        `Are you sure you want to delete all of your company's departments?`,
        () => document.getElementById("deleteAllForm").submit(), // Resubmit the form upon confirmation
        "btn-danger",
        "Delete"
    );
}