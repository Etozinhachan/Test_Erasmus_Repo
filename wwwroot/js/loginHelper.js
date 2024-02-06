const inputs = document.querySelectorAll('input')

function checkPasswordCharacterValidity(event){
    console.log(event.keyCode)
    if (!isAvailableCharacter(event.keyCode) && !isAvailableNumber(event.keyCode) && !isAvailableSpecialCharacter(event.keyCode)){
        return false
    }
    return true
}

function checkUsernameCharacterValidity(event){
    if (!isAvailableCharacter(event.keyCode) && !isAvailableNumber(event.keyCode) && !isAvailableSpecialCharacter(event.keyCode)){
        return false
    }
    return true
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
})