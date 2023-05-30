$(function () {

    $(document).ready(function () {
        let repetir = setInterval(getErrorText, 50);
    });
});

function getErrorText() {
    if ($('#Input_Email-error').text() == 'The E-mail field is required.') {
        $('#Input_Email-error').text('Campo email obrigatório');
    }

    else if ($('#Input_Email-error').text() == 'The E-mail field is not a valid e-mail address.') {
        $('#Input_Email-error').text('O email não é valido');
    }

    if ($('#Input_UserName-error').text() == 'The Usuário field is required.') {
        $('#Input_UserName-error').text('Campo usuário obrigatório');
    }

    if ($('#Input_Password-error').text() == 'The Senha field is required.') {
        $('#Input_Password-error').text('Campo senha obrigatório');
    }

    if ($('#Input_FullName-error').text() == 'The Nome Completo field is required.') {
        $('#Input_FullName-error').text('Campo nome completo obrigatório');
    }

    //if ($('div.validacaoTexto ul li').text() == 'Passwords must have at least one non alphanumeric character.') {
       
    //}

    //$('.validacaoTexto li').text() = 'Senha deve conter pelo menos uma letra';
}
