document.addEventListener("DOMContentLoaded", function () {

    let currentPage = 1;
    const pageSize = 5;
    let debounceTimer = null;

    loadUsers();

    // 🔎 Búsqueda automática con debounce (2 segundos)
    document.getElementById("searchInput").addEventListener("input", function () {

        clearTimeout(debounceTimer);

        debounceTimer = setTimeout(function () {
            currentPage = 1;
            loadUsers();
        }, 1000);

    });

    function loadUsers() {

        const search = document.getElementById("searchInput").value;

        $("#spinner").removeClass("d-none");

        $.ajax({
            url: '/Users/GetUsers',
            type: 'GET',
            data: {
                page: currentPage,
                pageSize: pageSize,
                search: search
            },
            success: function (response) {

                renderTable(response.data);
                renderPagination(response.totalRecords);

                $("#spinner").addClass("d-none");
            },
            error: function (error) {
                $("#spinner").addClass("d-none");
                showAlert("Error cargando usuarios", "danger");
                console.error(error);
            }
        });
    }

    function renderTable(users) {

        let rows = "";

        if (users.length === 0) {
            rows = `<tr><td colspan="12" class="text-muted">No se encontraron registros</td></tr>`;
        }

        users.forEach(user => {

            rows += `
                <tr>
                    <td>${user.id}</td>
                    <td>${user.email}</td>
                    <td>${user.username}</td>
                    <td>********</td>
                    <td>${user.name.firstname}</td>
                    <td>${user.name.lastname}</td>
                    <td>${user.address.city}</td>
                    <td>${user.address.street}</td>
                    <td>${user.address.number}</td>
                    <td>${user.address.zipcode}</td>
                    <td>${user.phone}</td>
                    <td>
                        <button class="btn btn-sm btn-warning me-1"
                                onclick="editUser(${user.id})">
                                Editar
                        </button>
                        <button class="btn btn-sm btn-danger"
                                onclick="deleteUser(${user.id})">
                                Eliminar
                        </button>
                    </td>
                </tr>
            `;
        });

        document.querySelector("#tablaUsuarios tbody").innerHTML = rows;
    }

    function renderPagination(totalRecords) {

        let totalPages = Math.ceil(totalRecords / pageSize);
        let paginationHtml = "";

        if (totalPages <= 1) {
            $("#paginacion").html("");
            return;
        }

        for (let i = 1; i <= totalPages; i++) {

            paginationHtml += `
                <li class="page-item ${i === currentPage ? "active" : ""}">
                    <a class="page-link" href="#" onclick="goToPage(${i})">${i}</a>
                </li>
            `;
        }

        $("#paginacion").html(paginationHtml);
    }

    window.goToPage = function (page) {
        currentPage = page;
        loadUsers();
    }

});

let editingUserId = null;
let deletingUserId = null;

function openCreateModal() {

    editingUserId = null;

    document.getElementById("modalTitle").innerText = "Nuevo Usuario";
    document.getElementById("userForm").reset();

    $("#idContainer").addClass("d-none");

    new bootstrap.Modal(document.getElementById('userModal')).show();
}

function editUser(id) {

    editingUserId = id;

    const row = document.querySelector(`button[onclick="editUser(${id})"]`).closest("tr");

    document.getElementById("modalTitle").innerText = "Editar Usuario";
    $("#idContainer").removeClass("d-none");

    document.getElementById("userId").value = row.children[0].innerText;
    document.getElementById("email").value = row.children[1].innerText;
    document.getElementById("username").value = row.children[2].innerText;

    new bootstrap.Modal(document.getElementById('userModal')).show();
}

function saveUser() {

    if (!validateForm()) return;

    const user = {
        email: email.value.trim(),
        username: username.value.trim(),
        password: password.value.trim(),
        name: {
            firstname: firstname.value.trim(),
            lastname: lastname.value.trim()
        },
        address: {
            city: city.value.trim(),
            street: street.value.trim(),
            number: parseInt(number.value),
            zipcode: zipcode.value.trim()
        },
        phone: phone.value.trim()
    };

    let url = editingUserId
        ? `/Users/Edit/${editingUserId}`
        : `/Users/Create`;

    let method = editingUserId ? "PUT" : "POST";

    $.ajax({
        url: url,
        type: method,
        contentType: "application/json",
        data: JSON.stringify(user),
        success: function () {
            loadUsers();
            bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
            showAlert("Operación exitosa", "success");
        },
        error: function (xhr) {
            showAlert("Error: " + xhr.responseText, "danger");
        }
    });
}

function addRow(user) {

    const tbody = document.querySelector("#tablaUsuarios tbody");

    const row = `
        <tr>
            <td>${user.id}</td>
            <td>${user.email}</td>
            <td>${user.username}</td>
            <td>********</td>
            <td>${user.name.firstname}</td>
            <td>${user.name.lastname}</td>
            <td>${user.address.city}</td>
            <td>${user.address.street}</td>
            <td>${user.address.number}</td>
            <td>${user.address.zipcode}</td>
            <td>${user.phone}</td>
            <td>
                <button class="btn btn-sm btn-warning me-1"
                        onclick="editUser(${user.id})">Editar</button>
                <button class="btn btn-sm btn-danger"
                        onclick="deleteUser(${user.id})">Eliminar</button>
            </td>
        </tr>
    `;

    tbody.insertAdjacentHTML("afterbegin", row);
}

function updateRow(user) {

    const row = document.querySelector(`button[onclick="editUser(${user.id})"]`).closest("tr");

    row.children[1].innerText = user.email;
    row.children[2].innerText = user.username;
}

function deleteUser(id) {

    deletingUserId = id;

    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

function confirmDelete() {

    $.ajax({
        url: `/Users/Delete/${deletingUserId}`,
        type: "DELETE",
        success: function () {
            loadUsers();
            bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
            showAlert("Usuario eliminado", "success");
        },
        error: function () {
            showAlert("Error eliminando usuario", "danger");
        }
    });
}

function validateForm() {

    const emailVal = email.value.trim();
    const usernameVal = username.value.trim();
    const passwordVal = password.value.trim();
    const firstnameVal = firstname.value.trim();
    const lastnameVal = lastname.value.trim();
    const cityVal = city.value.trim();
    const streetVal = street.value.trim();
    const numberVal = number.value.trim();
    const zipcodeVal = zipcode.value.trim();
    const phoneVal = phone.value.trim();

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const usernameRegex = /^[a-zA-Z0-9._-]{4,30}$/;
    const passwordRegex = /^(?=.*[A-Za-z])(?=.*\d).{8,64}$/;
    const zipcodeRegex = /^[A-Za-z0-9 -]{3,12}$/;
    const phoneRegex = /^[0-9()+ -]{7,20}$/;

    if (!emailVal) {
        showAlert("Email es requerido.", "danger");
        return false;
    }

    if (emailVal.length > 100) {
        showAlert("Email máximo 100 caracteres.", "danger");
        return false;
    }

    if (!emailRegex.test(emailVal)) {
        showAlert("Email debe tener formato válido (ej: usuario@email.com).", "danger");
        return false;
    }

    if (!usernameRegex.test(usernameVal)) {
        showAlert("Username debe tener 4-30 caracteres. Solo letras, números y ._- sin espacios.", "danger");
        return false;
    }

    if (!passwordRegex.test(passwordVal)) {
        showAlert("Password debe tener 8-64 caracteres, al menos 1 letra y 1 número.", "danger");
        return false;
    }

    if (firstnameVal.length < 2 || firstnameVal.length > 50) {
        showAlert("Nombre debe tener entre 2 y 50 caracteres.", "danger");
        return false;
    }

    if (lastnameVal.length < 2 || lastnameVal.length > 50) {
        showAlert("Apellido debe tener entre 2 y 50 caracteres.", "danger");
        return false;
    }

    if (cityVal.length < 2 || cityVal.length > 60) {
        showAlert("Ciudad debe tener entre 2 y 60 caracteres.", "danger");
        return false;
    }

    if (streetVal.length < 2 || streetVal.length > 100) {
        showAlert("Calle debe tener entre 2 y 100 caracteres.", "danger");
        return false;
    }

    if (!numberVal || parseInt(numberVal) < 1) {
        showAlert("Número debe ser un entero mayor o igual a 1.", "danger");
        return false;
    }

    if (!zipcodeRegex.test(zipcodeVal)) {
        showAlert("Zipcode debe tener entre 3 y 12 caracteres alfanuméricos.", "danger");
        return false;
    }

    if (!phoneRegex.test(phoneVal)) {
        showAlert("Phone debe tener entre 7 y 20 caracteres. Solo números y símbolos (+ - ()).", "danger");
        return false;
    }

    return true;
}

function showAlert(message, type) {

    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show position-fixed top-0 end-0 m-3 shadow"
             role="alert" style="z-index:9999;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $("body").append(alertHtml);

    setTimeout(() => $(".alert").alert('close'), 3000);
}