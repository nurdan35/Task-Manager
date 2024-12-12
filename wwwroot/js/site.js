document.addEventListener("DOMContentLoaded", function () {
    var toggleButton = document.getElementById("sidebarToggle");
    var sidebar = document.getElementById("sidebar");
    var mainContent = document.getElementById("mainContent");


    console.log(toggleButton, sidebar, mainContent);

    if (toggleButton && sidebar && mainContent) {
        toggleButton.addEventListener("click", function () {
            sidebar.classList.toggle("active");
            mainContent.classList.toggle("sidebar-open");
        });
    } else {
        console.error("HTML öğeleri bulunamadı!");
    }
});