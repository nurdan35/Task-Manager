function selectProfilePicture(img) {
    // Highlight the selected example profile picture
    document.querySelectorAll('.profile-thumbnail').forEach(thumbnail => {
        thumbnail.style.border = "2px solid transparent";
    });
    img.style.border = "2px solid #d6b8b1"; // Highlight the selected one

    document.getElementById('SelectedProfilePicture').value = img.getAttribute('data-image-path');
}