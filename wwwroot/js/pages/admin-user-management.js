document.addEventListener('DOMContentLoaded', function () {
    // Show toast if success message exists
    const successToast = document.getElementById('successToast');
    const toastMessage = document.getElementById('toastMessage');
    if (toastMessage && toastMessage.textContent.trim()) {
        const toast = new bootstrap.Toast(successToast);
        toast.show();
    }
});

// Search users function
function searchUsers() {
    const input = document.getElementById('searchInput');
    const filter = input.value.toLowerCase();
    const table = document.getElementById('usersTable');

    if (!table) return;

    const rows = table.getElementsByTagName('tbody')[0].getElementsByTagName('tr');

    for (let i = 0; i < rows.length; i++) {
        const nameData = rows[i].getAttribute('data-name').toLowerCase();
        const emailData = rows[i].getAttribute('data-email').toLowerCase();

        if (nameData.includes(filter) || emailData.includes(filter)) {
            rows[i].style.display = '';
        } else {
            rows[i].style.display = 'none';
        }
    }
}

function showRoleModal(userId, fullName, currentRole) {
    document.getElementById('roleUserId').value = userId;
    document.getElementById('roleUserName').textContent = fullName;
    document.getElementById('roleSelect').value = currentRole;

    const roleModal = new bootstrap.Modal(document.getElementById('roleModal'));
    roleModal.show();
}

function submitRoleForm() {
    document.getElementById('roleForm').submit();
    showLoading();
}

function filterUsersByRole(role) {
    const table = document.getElementById('usersTable');
    if (!table) return;

    const rows = table.getElementsByTagName('tbody')[0].getElementsByTagName('tr');

    for (let i = 0; i < rows.length; i++) {
        const roleData = rows[i].getAttribute('data-role');

        if (role === 'all' || roleData === role) {
            rows[i].style.display = '';
        } else {
            rows[i].style.display = 'none';
        }
    }
}

function confirmDelete(userId, fullName) {
    document.getElementById('deleteUserId').value = userId;
    document.getElementById('deleteUserName').textContent = fullName;

    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}

function showLoading() {
    document.getElementById('loadingBar').style.display = 'block';
}

function confirmDelete(userId, fullName) {
    document.getElementById('deleteUserId').value = userId;
    document.getElementById('deleteUserName').textContent = fullName;
    document.getElementById('deleteUserId').dataset.userId = userId; // Store userId for later use

    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}

function submitDeleteForm() {
    const userId = document.getElementById('deleteUserId').dataset.userId;

    // Use Fetch API with DELETE method
    fetch(`/admin/users/${userId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Show success message
                showToastMessage('Success', data.message);

                // Hide modal
                const deleteModal = bootstrap.Modal.getInstance(document.getElementById('deleteModal'));
                deleteModal.hide();

                // Reload page after 1 second
                setTimeout(() => {
                    location.reload();
                }, 1000);
            } else {
                showToastMessage('Error', data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToastMessage('Error', 'An error occurred while deleting the user.');
        });
}

function showToastMessage(title, message) {
    const toast = document.getElementById('successToast');
    const toastHeader = toast.querySelector('.toast-header strong');
    const toastBody = toast.querySelector('.toast-body');

    toastHeader.textContent = title;
    toastBody.textContent = message;

    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}