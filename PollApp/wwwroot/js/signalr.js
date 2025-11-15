// wwwroot/js/signalr.js
// Optional separate file for SignalR client setup, but since integrated in views, can be omitted or used globally if needed
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/resultsHub")
    .withAutomaticReconnect()
    .build();

connection.start().catch(err => console.error(err));

// Export if modular, but for simplicity, include in views