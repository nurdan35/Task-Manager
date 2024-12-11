// Dynamisk søkefunksjon
function searchTasks(query) {
    // Bruk fetch for å sende en GET-forespørsel med søkespørringen
    fetch(`/Task/Index?searchQuery=${encodeURIComponent(query)}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest', // Indikerer AJAX-forespørsel
            'Accept': 'text/html' // Forventer HTML som svar
        }
    })
        .then(response => {
            // Sjekk om forespørselen er vellykket
            if (!response.ok) {
                console.error('Failed to fetch tasks');
                return;
            }
            return response.text(); // Returner HTML som tekst
        })
        .then(html => {
            if (html) {
                // Oppdater #taskListContainer med nytt innhold
                document.getElementById('taskListContainer').innerHTML = html;
            }
        })
        .catch(error => {
            // Håndter feil
            console.error('Error:', error);
        });
}

// Legg til event listener for søkefeltet
document.getElementById('searchInput').addEventListener('input', function () {
    const query = this.value; // Få søketeksten
    searchTasks(query); // Kall søkefunksjonen
});


/*DARK MODE FUNCTIONALITY*/
document.addEventListener("DOMContentLoaded", function () {
    const themeToggleButton = document.getElementById("toggleTheme");
    const currentTheme = localStorage.getItem("theme") || "light";

    if (currentTheme === "dark") {
        document.body.classList.add("dark-mode");
        themeToggleButton.textContent = "Switch to Light Mode";
    } else {
        themeToggleButton.textContent = "Switch to Dark Mode";
    }

    themeToggleButton.addEventListener("click", () => {
        document.body.classList.toggle("dark-mode");
        const isDarkMode = document.body.classList.contains("dark-mode");
        localStorage.setItem("theme", isDarkMode ? "dark" : "light");
        themeToggleButton.textContent = isDarkMode
            ? "Switch to Light Mode"
            : "Switch to Dark Mode";
    });
});
