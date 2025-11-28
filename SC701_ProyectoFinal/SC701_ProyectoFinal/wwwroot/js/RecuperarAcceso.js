$(function () {
    $("#formRecuperarAcceso").validate({
        rules: {
            Correo: {
                required: true
            }
        },
        messages: {
            Correo: {
                required: "* Requerido"
            }
        }
    });
});