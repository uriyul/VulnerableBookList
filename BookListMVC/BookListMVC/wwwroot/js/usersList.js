var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#DT_users').DataTable({
        "ajax": {
            "url": "/users/getall/",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "username", "width": "20%" },
            { "data": "firstName", "width": "20%" },
            { "data": "lastName", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    if (document.getElementById("isAdmin").value != "true") {
                        return `<div class="text-center">
                        <a href="/users/Display?id=${data}" class='btn btn-success text-white' style='cursor:pointer; width:70px;'>
                            View
                        </a>
                        </div>`;
                    } else {
                        return `<div class="text-center">
                        <a href="/users/Update?id=${data}" class='btn btn-success text-white' style='cursor:pointer; width:70px;'>
                            Edit
                        </a>
                        &nbsp;
                        <a class='btn btn-danger text-white' style='cursor:pointer; width:70px;'
                            onclick=Delete('/users/Delete?id='+${data})>
                            Delete
                        </a>
                        </div>`;
                    };
                }, "width": "40%"
            }
        ],
        "language": {
            "emptyTable": "no data found"
        },
        "width": "100%"
    });
}

function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

function Update(url) {
    $.ajax({
        type: "Get",
        url: url,
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    });
}
