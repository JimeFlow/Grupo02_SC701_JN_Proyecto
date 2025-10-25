$(function () {
    $("#FormInicioSesion").validate({
        rules: {
            correo: {
                required: true
            },
            contrasena: {
                required: true
            },
        },
        messages: {
            correo: {
                required: "* Requerido"
            },
            contrasena: {
                required: "* Requerido"
            }
        }
    });
});