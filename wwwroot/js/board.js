document.addEventListener('DOMContentLoaded', function () {
    console.log("DOMContentLoaded has been triggered");

    let tasks = document.querySelectorAll('.task');
    let columns = document.querySelectorAll('.column ul');
    let csrfTokenElement = document.querySelector('meta[name="csrf-token"]');

    if (!csrfTokenElement) {
        console.error("CSRF Token not found in the DOM.");
        return;
    }

    let csrfToken = csrfTokenElement.getAttribute('content');

    // Prevent form submission and send request via AJAX
    document.querySelectorAll('.task-actions form').forEach(form => {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            let formData = new FormData(form);
            let taskId = formData.get('id');
            let newStatus = formData.get('newStatus');

            updateTaskStatus(taskId, newStatus, csrfToken)
                .then(response => {
                    if (response.ok) {
                        console.log(`Task status updated: ${taskId}, new status: ${newStatus}`);
                        moveTaskToColumn(taskId, newStatus);
                    } else {
                        console.error('Failed to update task status.');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        });
    });

    // Drag and Drop Operations
    tasks.forEach(task => {
        task.draggable = true;

        task.addEventListener('dragstart', function (e) {
            console.log(`Drag started for task ID: ${task.dataset.taskId}`);
            e.dataTransfer.setData('text/plain', task.dataset.taskId);
        });
    });

    columns.forEach(column => {
        column.parentElement.addEventListener('dragover', function (e) {
            e.preventDefault();
            column.parentElement.classList.add('highlight'); // Add visual effect to droppable columns
            console.log(`Drag over on column: ${column.parentElement.id}`);
        });

        column.parentElement.addEventListener('dragleave', function () {
            column.parentElement.classList.remove('highlight'); // Remove visual effect
        });

        column.parentElement.addEventListener('drop', function (e) {
            e.preventDefault();
            column.parentElement.classList.remove('highlight');

            let taskId = e.dataTransfer.getData('text/plain');
            let newStatus = column.parentElement.id.split('-')[0]; // todo, doing, done

            updateTaskStatus(taskId, newStatus, csrfToken)
                .then(response => {
                    if (response.ok) {
                        console.log(`Dropped task ID: ${taskId} to column: ${column.parentElement.id}`);
                        moveTaskToColumn(taskId, newStatus);
                    } else {
                        console.error('Failed to update task status.');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        });
    });


    // Added: Delete functionality for shared boards
    document.querySelectorAll('form[asp-action="DeleteSharedBoard"] button').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault(); // Prevent default form submission

            let boardId = button.getAttribute('data-board-id'); // Button'dan Board ID'sini al
            console.log(`Board ID retrieved: ${boardId}`); // Debug için kontrol edin

            if (!boardId) {
                console.error("Board ID is missing for the Remove action.");
                return;
            }

            // Confirm before deletion
            if (!confirm("Are you sure you want to remove this shared board?")) {
                return;
            }

            // Send an AJAX request to delete the shared board
            fetch(`/Board/DeleteSharedBoard`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': csrfToken
                },
                body: JSON.stringify({ boardId: boardId })
            })
                .then(response => {
                    if (response.ok) {
                        console.log(`Shared board with ID ${boardId} has been successfully removed.`);
                        // Remove the board entry from the DOM
                        let parentLi = button.closest('li');
                        if (parentLi) {
                            parentLi.remove();
                        }
                    } else {
                        console.error(`Failed to remove shared board with ID ${boardId}.`);
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        });
    });



    // Function to update task status and make a request to the server
    function updateTaskStatus(taskId, newStatus, csrfToken) {
        return fetch('/Task/UpdateStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': csrfToken
            },
            body: `id=${taskId}&newStatus=${newStatus}`
        });
    }

    // Function to move the task to the new column
    function moveTaskToColumn(taskId, newStatus) {
        let taskElement = document.querySelector(`[data-task-id="${taskId}"]`);
        let targetColumn = document.getElementById(`${newStatus}-tasks`);

        if (taskElement && targetColumn) {
            targetColumn.appendChild(taskElement);
            console.log(`Task ID ${taskId} moved to ${newStatus} column.`);
        } else {
            console.error(`Unable to find task element or target column for task ID: ${taskId}`);
        }
    }
});
