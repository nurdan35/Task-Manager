document.addEventListener('DOMContentLoaded', function () {
    let tasks = document.querySelectorAll('.task');
    let columns = document.querySelectorAll('.column ul');

    tasks.forEach(task => {
        task.draggable = true;

        task.addEventListener('dragstart', function (e) {
            e.dataTransfer.setData('text/plain', task.dataset.taskId);
        });
    });

    columns.forEach(column => {
        column.addEventListener('dragover', function (e) {
            e.preventDefault();
        });

        column.addEventListener('drop', function (e) {
            let taskId = e.dataTransfer.getData('text/plain');
            let taskElement = document.querySelector(`[data-task-id='${taskId}']`);
            column.appendChild(taskElement);

            let newStatus = column.id.split('-')[0]; // todo, doing, done

            // AJAX call to update task status
            fetch(`/Board/UpdateTaskStatus`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: `id=${taskId}&newStatus=${newStatus}`
            });
        });
    });
});
