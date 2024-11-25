//Handle the "select all" functionality
document.getElementById("selectAll").addEventListener("change", function () {
    const checkboxes = document.querySelectorAll(".employee-checkbox");
    checkboxes.forEach((checkbox) => {
        checkbox.checked = this.checked;
    });
});

//Handle delete selected employees
document.getElementById("deleteSelectedBtn").addEventListener("click", function () {
    const selectedIds = [];
    const checkboxes = document.querySelectorAll(".employee-checkbox:checked");
    checkboxes.forEach((checkbox) => {
        selectedIds.push(checkbox.value);
    });

    if (selectedIds.length > 0) {
        if (confirm("Are you sure you want to delete the selected employees?")) {
            // You can submit the deletion via AJAX or form submission
            document.getElementById("manageEmployeesForm").action = "/Employee/DeleteSelectedEmployees";
            document.getElementById("manageEmployeesForm").submit(); // Submit the form with the selected employees
        }
    } else {
        alert("Please select at least one employee.");
    }
});