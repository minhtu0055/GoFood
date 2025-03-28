function confirmDeactivate(formId) {
    Swal.fire({
        title: 'Bạn có chắc chắn?',
        text: "Bạn có muốn vô hiệu hóa người dùng này không?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Có, vô hiệu hóa!',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            document.getElementById(formId).submit();
        }
    });
    return false;
}

function confirmActivate(formId) {
    Swal.fire({
        title: 'Bạn có chắc chắn?',
        text: "Bạn có muốn kích hoạt người dùng này không?",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Có, kích hoạt!',
        cancelButtonText: 'Hủy'
    }).then((result) => {
        if (result.isConfirmed) {
            document.getElementById(formId).submit();
        }
    });
    return false;
} 