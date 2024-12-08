//Handle automatic scrolling
document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    let scrollToTableText = urlParams.get("scrollToTable");
    if (scrollToTableText !== null) {
        if (urlParams.get("scrollToTable").toLowerCase() === "true") {
            const table = document.getElementById("vacationsTable");
            if (table) {
                table.scrollIntoView({ behavior: "instant" });
                //Remove the scrollToTable parameter
                urlParams.delete("scrollToTable");
                const newUrl = `${window.location.pathname}?${urlParams.toString()}`;
                history.replaceState(null, "", newUrl);
            }
        }
    }
});

//Modal dialog functions
function confirmDelete(vacationId) {
    let totalDays = document.getElementById(`vacationTotalDays-${vacationId}`).innerHTML;

    // Show the confirmation modal
    showConfirmationModal(
        `Are you sure you want to delete vacation request with Total Days: ${totalDays}?`,
        () => document.getElementById(`deleteForm-${vacationId}`).submit(), //Resubmit the form upon confirmation
        "btn-danger",
        "Delete"
    );
}

function confirmDeleteAll(vacationStatus) {
    // Show the confirmation modal
    showConfirmationModal(
        `Are you sure you want to delete all of your vacation requests with Status: '${vacationStatus}'?`,
        () => document.getElementById(`deleteAll${vacationStatus}Form`).submit(), //Resubmit the form upon confirmation
        "btn-danger",
        "Delete"
    );
}