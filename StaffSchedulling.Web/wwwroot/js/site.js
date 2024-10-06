function copyInviteCode(companyId) {
    // Get the invite link value from the hidden input field
    var inviteLink = document.getElementById("inviteCode_" + companyId).value;

    //Get button's span child
    var buttonSpanElement = document.querySelector('#copyBtn_' + companyId + ' span')

    // Use the Clipboard API to copy the invite code to the clipboard
    navigator.clipboard.writeText(inviteLink).then(function () {
        // Change the button text to provide feedback
        buttonSpanElement.textContent = 'Copied Successfully!';

        // After 1 second, change the text back to the original
        setTimeout(function () {
            buttonSpanElement.textContent = "Copy Invite Link";
        }, 1000);
    }).catch(function (error) {
        console.error('Error copying text: ', error);
    });
}