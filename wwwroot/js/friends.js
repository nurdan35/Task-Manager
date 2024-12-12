document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('sendFriendRequestForm');

    if (!form) {
        console.error('Form element not found!');
        return;
    }

    form.addEventListener('submit', async function (event) {
        event.preventDefault(); // Formun varsayılan gönderimini durdurun
        
        const formData = new FormData(form);
        const response = await fetch(form.action, {
            method: form.method,
            body: formData
        });
        const result = await response.json();

        // Hata veya başarı mesajını göster
        const messageBox = document.getElementById('sendRequestMessage');
        if (messageBox) {
            messageBox.innerText = result.message;
            messageBox.style.color = result.success ? 'green' : 'red';
        }
    });
});