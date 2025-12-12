const modalExtender = document.getElementById('modalExtender');

modalExtender.addEventListener('show.bs.modal', event => {
    const button = event.relatedTarget;

    const movId = button.getAttribute('data-id');
    const fechaVenc = button.getAttribute('data-fecha');

    const idInput = document.getElementById('movIdExtender');
    const fechaInput = document.getElementById('fechaExtender');

    idInput.value = movId;
    //validacion de la fecha
    fechaInput.min = fechaVenc;
    fechaInput.value = fechaVenc;
});

const modalCancelar = document.getElementById('modalCancelar');

modalCancelar.addEventListener('show.bs.modal', event => {
    const button = event.relatedTarget;

    const movId = button.getAttribute('data-id');

    const idInput = document.getElementById('movIdCancelar');
    idInput.value = movId;
});