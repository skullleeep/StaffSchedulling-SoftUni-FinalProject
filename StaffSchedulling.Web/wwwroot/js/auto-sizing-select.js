// Get all the select elements
const selects = document.querySelectorAll('select');

// Loop through each select element
selects.forEach(select => {
    // Get all the options inside this select
    const options = Array.from(select.options);

    // Find the longest option's text length
    const maxLength = Math.max(...options.map(option => option.innerHTML.length));


    let fontSize = 14;
    // Calculate the font size based on the longest option's length
    if (maxLength > 25) {
        fontSize = 13 - maxLength / 25 * 0.9;
    }

    // Apply the calculated font size to the select element
    select.style.fontSize = `${fontSize}px`;
});
