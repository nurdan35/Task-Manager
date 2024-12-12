if ('serviceWorker' in navigator && 'PushManager' in window) {
    console.log('Service Worker and Push are supported');

    navigator.serviceWorker.register('/sw.js') // Path to your service worker file
        .then(function (swReg) {
            console.log('Service Worker is registered', swReg);

            swReg.pushManager.getSubscription()
                .then(function (subscription) {
                    if (subscription === null) {
                        // User is not subscribed, let's subscribe them
                        subscribeUser(swReg);
                    } else {
                        console.log('User is already subscribed', subscription);
                    }
                });
        })
        .catch(function (error) {
            console.error('Service Worker Error', error);
        });
}

function subscribeUser(swReg) {
    const applicationServerPublicKey = 'YOUR_PUBLIC_KEY'; // Replace with your VAPID public key
    const applicationServerKey = urlBase64ToUint8Array(applicationServerPublicKey);

    swReg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: applicationServerKey
    })
        .then(function (subscription) {
            console.log('User is subscribed:', subscription);

            // Send subscription details to the server
            sendSubscriptionToServer(subscription);
        })
        .catch(function (error) {
            console.error('Failed to subscribe the user:', error);
        });
}

function sendSubscriptionToServer(subscription) {
    // Send a POST request to your server with the subscription details
    fetch('/api/push/subscribe', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(subscription)
    })
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Failed to store subscription on the server.');
            }
            return response.json();
        })
        .then(function (data) {
            console.log('Server subscription saved:', data);
        })
        .catch(function (error) {
            console.error('Failed to send subscription to server:', error);
        });
}

// Utility function to convert base64 string to Uint8Array
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding)
        .replace(/-/g, '+')
        .replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
