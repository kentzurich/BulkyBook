var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblUser').DataTable({
        "ajax": {
            "url":"/Admin/User/GetAll"
        },
        "columns": [
            {"data": "name", "width": "15%"},
            {"data": "email", "width": "15%"},
            {"data": "phoneNumber", "width": "15%"},
            {"data": "company.name", "width": "15%"},
            {"data": "role", "width": "15%"},
            {
                "data": { id:"id", lockoutEnd:"lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        return `
                        <div class="text-center">
                            <a onclick="return LockUnlock('${data.id}')" class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                                <i class="bi bi-lock-fill"></i> &nbspLock
                            </a>
                             <a href="/Admin/User/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:150px;">
                                <i class="bi bi-pencil-square"></i> &nbspPermission
                            </a>
                        </div>
                    `;
                    }
                    else {
                        return `
                        <div class="text-center">
                            <a onclick="return LockUnlock('${data.id}')" class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                                <i class="bi bi-unlock-fill"></i> &nbspUnlock
                            </a>
                             <a href="/Admin/User/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:150px;">
                                <i class="bi bi-pencil-square"></i> &nbspPermission
                            </a>
                        </div>
                    `;
                    }
                },
                "width": "50%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: "/Admin/User/LockUnlockAcc",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
}