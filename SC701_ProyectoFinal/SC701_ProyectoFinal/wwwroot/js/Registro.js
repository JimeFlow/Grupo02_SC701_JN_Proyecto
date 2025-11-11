$(function () {
    $("#formRegister").validate({
        rules: {
            nombre: {
                required: true
            },
            apellidos: {
                required: true
            },
            identificacion: {
                required: true
            },
            correo: {
                required: true
            },
            contrasena: {
                required: true
            },
            confirmPassword: {
                required: true,
                equalTo: "#Contrasena"
            },
            telefono: {
                required: true
            }
        },
        messages: {
            identificacion: {
                required: "* Requerido"
            },
            nombre: {
                required: "* Requerido"
            },
            apellidos: {
                required: "* Requerido"
            },
            correo: {
                required: "* Requerido"
            },
            contrasena: {
                required: "* Requerido"
            },
            confirmPassword: {
                required: "* Requerido",
                equalTo: "* La confirmación no coincide"
            },
            telefono: {
                required: "* Requerido"
            }
        },
        errorClass: "text-danger",
        errorElement: "span"
    });
});