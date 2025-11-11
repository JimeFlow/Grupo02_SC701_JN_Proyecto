$(function () {

    $("#FormRegistrarUsuario").validate({
        rules: {
            Nombre: { required: true },
            Apellidos: { required: true },
            Identificacion: { required: true },
            Correo: { required: true, email: true },
            Telefono: { required: true },
            Id_Rol: { required: true }
        },
        messages: {
            Nombre: { required: "* Requerido" },
            Apellidos: { required: "* Requerido" },
            Identificacion: { required: "* Requerido" },
            Correo: {
                required: "* Requerido",
                email: "* Ingrese un correo válido"
            },
            Telefono: { required: "* Requerido" },
            Id_Rol: { required: "* Requerido"}
        },
        errorClass: "text-danger",
        errorElement: "span"
    });

});
