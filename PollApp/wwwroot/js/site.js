// wwwroot/js/site.js
// Custom JavaScript for the application
// Includes AJAX helpers, form enhancements, etc.

$(document).ready(function () {
    // Example: Global AJAX error handling
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        console.error("AJAX Error: " + thrownError);
        alert("An error occurred. Please try again.");
    });

    // If needed, add more global behaviors
});

// Function to add option (can be used in Create/Edit if not inline in views)
function addOption(containerId) {
    let optionIndex = $(`#${containerId} .input-group`).length;
    const newOption = `
        <div class="input-group mb-2">
            <input type="text" name="Options[${optionIndex}]" class="form-control" placeholder="New Option" required />
            <button type="button" class="btn btn-danger" onclick="removeOption(this)">Remove</button>
        </div>`;
    $(`#${containerId}`).append(newOption);
}

function removeOption(button) {
    $(button).parent().remove();
}

// If separating SignalR logic, but since it's in views, optional