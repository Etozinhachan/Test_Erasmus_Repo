const inputs = document.querySelectorAll('input')
const usernameInputs = document.querySelectorAll('input[type="text"]')
const passwordInputs = document.querySelectorAll('input[type="password"]')

function checkPasswordCharacterValidity(event){
    console.log(event.keyCode)
    if (!isAvailableCharacter(event.keyCode) && !isAvailableNumber(event.keyCode) && !isAvailableSpecialCharacter(event.keyCode)){
        event.preventDefault()
    }
}

function checkUsernameCharacterValidity(event){
    if (!isAvailableCharacter(event.keyCode) && !isAvailableNumber(event.keyCode) && !isAvailableSpecialCharacter(event.keyCode)){
        event.preventDefault()
    }
}

function isAvailableCharacter(keyCode){
    return ((keyCode >= 65 && keyCode <= 90) || (keyCode >= 97 && keyCode <= 122))
}

function isAvailableNumber(keyCode){
    return (keyCode >= 48 && keyCode <= 57)
}

function isAvailableSpecialCharacter(keyCode){
    return (keyCode == 95 || keyCode == 35)
}

function pasteEventHandler(){
    return false
}

function preventBadPasting(pasteEvent){
    /* clipboardData = pasteEvent.clipboardData
    clipboardTextData = clipboardData.getData('Text')
     */
    pasteEvent.preventDefault();
}

inputs.forEach((input) => {
    input.addEventListener('paste', (event) => { preventBadPasting(event) })
});

usernameInputs.forEach((input) => {
    if (input.getAttribute('required') == null){
        input.setAttribute('required')
    }
    if (input.getAttribute('minlength') == null){
        input.setAttribute('minlength', '3')
    }
    if (input.getAttribute('maxlength') == null){
        input.setAttribute('maxlength', '64')
    }
    input.addEventListener('keypress', (event) => { checkUsernameCharacterValidity(event) })
})

passwordInputs.forEach((input) => {
    if (input.getAttribute('required') == null){
        input.setAttribute('required')
    }
    if (input.getAttribute('minlength') == null){
        input.setAttribute('minlength', '8')
    }
    if (input.getAttribute('maxlength') == null){
        input.setAttribute('maxlength', '32')
    }
    input.addEventListener('keypress', (event) => { checkPasswordCharacterValidity(event) })
})