$(function () {

    $(document).ready(function () {
        let repetir = setInterval(changeErrorText, 20);
    });
});

function changeErrorText() {
    if ($('#Input_UserName-error').text() == 'The Usuário field is required.') {
        $('#Input_UserName-error').text('Campo usuário obrigatório');
    }

    if ($('#Input_Password-error').text() == 'The Password field is required.') {
        $('#Input_Password-error').text('Campo senha obrigatório');
    }
}
