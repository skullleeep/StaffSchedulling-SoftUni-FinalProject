//Modal dialog functions
function confirmDelete(companyId) {
    let companyName = document.getElementById(`companyName-${companyId}`).innerHTML;

    // Show the confirmation modal
    showConfirmationModal(
        `Are you sure you want to delete company '${companyName}'?`,
        () => document.getElementById('deleteForm').submit(), // Resubmit the form upon confirmation
        "btn-danger",
        "Delete"
    );
}