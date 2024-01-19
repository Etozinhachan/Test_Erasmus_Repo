const submitButton = document.querySelector("#submit")
const outputElement = document.querySelector('#output')
const inputElement = document.querySelector('input')
const historyElement = document.querySelector('.history')
const buttonElement = document.querySelector('#createChatBtn')
const chatBoxElement = document.querySelector('.chat-box')
const logOutElement = document.querySelector('#logOutBtn')
const jwt_token_Header = "erai-jwt-token";
var in_chat = false;
var enabled = true;
var current_chat_clicked;
var response_finished_generating = true;
const deleteInnerHtlm = '<img src="../assets/images/svg/bx-trash.svg" class="icon" style="width: 15px; height: 15px; display: none; position: absolute; left: 8.6%;">'

var sleep = ms => new Promise(r => setTimeout(r, ms));

window.onload = async function () {
    await checkCookies();
    await loadChats();
    pageAccessedByReload = (
        (window.performance.navigation && window.performance.navigation.type === 1) ||
          window.performance
            .getEntriesByType('navigation')
            .map((nav) => nav.type)
            .includes('reload')
    )
    if(pageAccessedByReload){
        //console.log('rawrz')
        const current_chat_id = await getCookie('current_chat')

        const chats = historyElement.querySelectorAll('p');

        chats.forEach(element => {
            if (`${element.id}` == `${current_chat_id}`){
                changeChat(element)
                deleteCookie('current_chat')
            }
        });
    }
    //console.log(await isAdmin(await getCookie(jwt_token_Header)));
    if (await isAdmin(await getCookie(jwt_token_Header))){
        const navSide = document.querySelector('.sidebar nav')
        const adminElement = document.createElement('button')
        adminElement.style.padding = '10px'
        adminElement.style.margin = '0'
        adminElement.style.border = '0'
        adminElement.addEventListener('click', () => { window.location.replace(`${window.location.href}`.replace('newChat.html', 'dashboard.html')); })

        navSide.append(adminElement)
    }
}

var setCookie = async (cname, cvalue, duration) => {
    const d = new Date();
    var days = duration.days,
        hours = duration.hours,
        minutes = duration.minutes,
        seconds = duration.seconds,
        miliseconds = duration.miliseconds
    if (days){
        d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
    }
    if (hours){
        d.setTime(d.getTime() + (hours * 60 * 60 * 1000));
    }
    if (minutes){
        d.setTime(d.getTime() + (minutes * 60 * 1000));
    }
    if (seconds){
        d.setTime(d.getTime() + (seconds * 1000));
    }
    if (miliseconds){
        d.setTime(d.getTime() + (miliseconds));
    }
    
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

var getCookie = async (cname, ignoreCheck) => {
    if (ignoreCheck -= undefined) {
        checkCookies();
    }
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

var checkCookies = async () => {
    let username = await getCookie("UserName", true);
    let pw = await getCookie("passHash", true);
    let erai_jwt = await getCookie(jwt_token_Header, true);
    //console.log(decodeURIComponent(document.cookie));
    //console.log(await userExists(erai_jwt))
    if (username == "" || pw == "" || erai_jwt == "" || !(await userExists(erai_jwt))) {
        deleteCookie('UserName')
        deleteCookie('passHash')
        deleteCookie(jwt_token_Header)
        window.location.replace(`${window.location.href}`.replace('newChat.html', ''));
    }
}

var deleteCookie = name => {
    document.cookie = name + "=;expires=" + new Date(0).toUTCString()
}

async function changeChat(chat) {
    await checkCookies();
    if (!response_finished_generating) {
        return;
    }
    const inputElement = document.querySelector('input')
    inputElement.value = ''
    chatBoxElement.innerHTML = ''

    in_chat = true;
    current_chat_clicked = chat;
    await load_chat(chat)
    chatBoxElement.scrollTo(0, chatBoxElement.scrollHeight);
}

async function generateAiResponse(chat_id, aiResponseElementRef) {
    await setCookie('current_chat', `${chat_id}`, {minutes: 10})
    const aiResponse = await getAiResponse(`${chat_id}`)
    //console.log(`${aiResponse.is_final}`)
    let aiResponseElement;
    if (aiResponseElementRef == undefined) {
        aiResponseElement = document.createElement('div')
    } else {
        aiResponseElement = aiResponseElementRef
    }
    response_finished_generating = aiResponse.is_final

    const closingPTagLength = '</p>'.length;
    const actualPElement = aiResponseElementRef == undefined ? '<p></p>' : aiResponseElementRef.innerHTML.substring(aiResponseElementRef.innerHTML.indexOf('<p>'), aiResponseElementRef.innerHTML.indexOf('</p>') + closingPTagLength)

    aiResponseElement.className = 'msg-row'
    aiResponseElement.innerHTML = `<img src="./assets/images/svg/bx-male.svg" alt="Ai's icon" class="msg-img">
            <div class="msg-text">
                <h2>Ai</h2>
                ${actualPElement}
            </div>`
    if (aiResponseElementRef == undefined) {
        chatBoxElement.append(aiResponseElement)
        chatBoxElement.scrollTo(0, chatBoxElement.scrollHeight);
    }
    await generateText(aiResponseElement.querySelector('p'), aiResponse)
    if (!aiResponse.is_final) {
        await generateAiResponse(chat_id, aiResponseElement)
    } else if (aiResponse.is_final) {
        response_finished_generating = aiResponse.is_final
        deleteCookie('current_chat');
    }

}

async function getAiResponse(chat_id) {
    const options = {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
        },
    }
    await sleep(250)
    try {
        const response = await fetch(`/api/chat/conversation/${chat_id}`, options)
        return await response.json()
    } catch (error) {
        console.error(error)
    }
}

async function generateText(textElement, aiResponse) {
    const text = textElement.textContent;
    await typeContent(textElement, aiResponse.response.substring(text.length));
    //const pElement = document.createElement('p');
    //pElement.textContent = meowDiv.getAttribute('data-content')
    textElement.removeAttribute('data-content')
    //meowDiv.append(pElement)
}

async function getMessage() {
    await checkCookies();
    if (!enabled) {
        return;
    }
    //console.log('clicked')
    const inputElement = document.querySelector('input')
    if (!in_chat) {
        const options = {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "prompt": `${inputElement.value}`
            })
        }
        try {
            inputValue = inputElement.value.replace(" ", "");
            if (inputValue == "") {
                return;
            }

            response_finished_generating = false;

            createUserPrompt();
            chatBoxElement.scrollTo(0, chatBoxElement.scrollHeight);

            disable_send_button();
            var response = await fetch('/api/chat/conversation', options)
            var data = await response.json();
            //outputElement.textContent = data.userPrompts[0].prompt;
            //console.log(data)
            if (data.userPrompts[0].prompt) {
                if (data.userPrompts[0].prompt_number == 0) {

                    if (data.id, data.userPrompts[0].prompt.length > 14){
                        cur_chat = updateHistory(data.id, data.userPrompts[0].prompt.substring(0, 14) + '...')
                    }else{
                        cur_chat = updateHistory(data.id, data.userPrompts[0].prompt)
                    }
                    in_chat = true;
                    current_chat_clicked = cur_chat;
                }
                await generateAiResponse(`${data.id}`)
                await sleep(1000)
                enable_send_button();
            }
        } catch (error) {
            console.error(error)
        }
    } else {
        const options = {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                "prompt": `${inputElement.value}`
            })
        }
        try {
            inputValue = inputElement.value.replace(" ", "");
            if (inputValue == "") {
                return;
            }

            response_finished_generating = false;

            createUserPrompt();
            chatBoxElement.scrollTo(0, chatBoxElement.scrollHeight);

            disable_send_button();

            await sleep(5000)

            var response = await fetch(`/api/chat/conversation/${current_chat_clicked.id}`, options)
            var data = await response.json();
            //outputElement.textContent = data.userPrompts[0].prompt;
            //console.log(data)
            //console.log(`/api/chat/conversation/${current_chat_clicked.id}`)
            /* if (data.userPrompts[0].prompt) {
                if (data.userPrompts[0].prompt_number == 0) {
                    const pElement = document.createElement('p');
                    pElement.textContent = data.userPrompts[0].prompt
                    pElement.addEventListener('click', () => changeInput(pElement.textContent))
                    historyElement.append(pElement)
                }

            } */
            //await sleep(3000)
            await generateAiResponse(`${data.conversation_id}`)
            await sleep(5000)
            enable_send_button();
        } catch (error) {
            console.error(error)
        }
    }

}

function clearInput() {
    inputElement.value = '';
    outputElement.value = '';
}

async function createNewChat() {
    if (!response_finished_generating) {
        return;
    }
    await checkCookies();
    clearInput();
    in_chat = false;
    chatBoxElement.innerHTML = ''
}

async function load_chat(chat) {
    const options = {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
        },
    }
    const userPromptsResponse = await fetch(`/api/chat/conversation/UserPrompts/${chat.id}`, options);
    const aiResponsesResponse = await fetch(`/api/chat/conversation/ChatResponses/${chat.id}`, options);

    const userPrompts = await userPromptsResponse.json();
    //console.log(userPrompts)
    const aiResponses = await aiResponsesResponse.json();
    //console.log(aiResponses)

    for (let index = 0; index < aiResponses.length; index++) {
        const cUserPrompt = userPrompts[index];
        const cAiResponse = aiResponses[index];

        const userPromptElement = document.createElement('div')
        userPromptElement.className = 'msg-row msg-row2'
        userPromptElement.innerHTML = `<div class="msg-text">
                    <h2>You</h2>
                    <p>${cUserPrompt.prompt}</p>
                </div>
                <img src="./assets/images/svg/bx-female.svg" alt="our icon" class="msg-img">`
        chatBoxElement.append(userPromptElement)

        const aiResponseElement = document.createElement('div')
        aiResponseElement.className = 'msg-row'
        aiResponseElement.innerHTML = `<img src="./assets/images/svg/bx-male.svg" alt="Ai's icon" class="msg-img">
                <div class="msg-text">
                    <h2>Ai</h2>
                    <p>${cAiResponse.response}</p>
                </div>`
        chatBoxElement.append(aiResponseElement)
    }
    clearInput();
}

async function getChats() {
    const options = {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
        },
    }
    const response = await fetch('/api/chat/conversation/Chats', options)
    return await response.json()
}

function removeChatFromHistory(chat) {
    /* console.log(historyElement.innerHTML)
    const startDelete = historyElement.innerHTML.indexOf(`<p id="${chat.id}">${chat.textContent}${deleteInnerHtlm}</p>`)
    const endDelete = `<p id="${chat.id}">${chat.textContent}${deleteInnerHtlm}</p>`.length
    console.log(startDelete + ' ' + endDelete) */
    /* const historyElements = historyElement.innerHTML.split(`<p id="${chat.id}">${chat.textContent}${deleteInnerHtlm}</p>`)
    historyElement.innerHTML = `${historyElements[0] + historyElements[1]}` 
    console.log(historyElements) */
    const historyChats = historyElement.querySelectorAll(`p`)
    historyChats.forEach(element => {
        const id = element.id || null
        if (element.id == chat.id) {
            element.remove()
        }
    });
    //console.log(historyChatToDelete)
}

async function deleteChat(chat) {
    await checkCookies();
    if (!response_finished_generating) {
        return;
    }
    const options = {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
        },
    }
    const response = await fetch(`/api/chat/conversation/${chat.id}`, options)
    if (response.status != 204) {
        throw new Error()
    }
    if (chat == current_chat_clicked) {
        chatBoxElement.innerHTML = '';
        in_chat = false;
        enabled = true;
        current_chat_clicked = undefined;
        response_finished_generating = true;
    }
    removeChatFromHistory(chat)
    //console.log('deleted sucessfully')
}

function updateHistory(chat_id, title) {
    const iconElement = document.createElement('img')
    iconElement.src = '../assets/images/svg/bx-trash.svg'
    iconElement.className = 'icon'
    iconElement.style.width = '15px'
    iconElement.style.height = '15px'

    iconElement.style.display = 'none'
    const pElement = document.createElement('p');
    pElement.textContent = title
    //pElement.setAttribute('id', chat.id)
    pElement.id = chat_id
    iconElement.style.position = 'absolute'
    iconElement.style.left = '8.6%'
    pElement.addEventListener('click', async () => await changeChat(pElement))
    pElement.addEventListener('mouseenter', async () => {iconElement.style.display = 'inline-block'})
    pElement.addEventListener('mouseleave', () => iconElement.style.display = 'none')
    iconElement.addEventListener('click', async () => { event.stopPropagation(), await deleteChat(pElement) })

    historyElement.append(pElement)
    pElement.append(iconElement)
    return pElement
}

function createUserPrompt() {
    const userPromptElement = document.createElement('div')
    userPromptElement.className = 'msg-row msg-row2'
    userPromptElement.innerHTML = `<div class="msg-text">
                    <h2>You</h2>
                    <p>${inputElement.value}</p>
                </div>
                <img src="./assets/images/svg/bx-female.svg" alt="our icon" class="msg-img">`
    chatBoxElement.append(userPromptElement)
}

async function hasCurrentChatPerm(jwt_user_token){
    return await hasChatPerm(jwt_user_token, current_chat_clicked.id)
}

async function hasChatPerm(jwt_user_token, chat_id){
    const options = {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${jwt_user_token}`
        }
    }

    try{
        const response = await fetch(`/api/chat/conversation/chat/${chat_id}`, options)
        if (response.ok){
            return true
        }
        return false        
    }catch(error){
        console.error(error)
        return false
    }
}

function toBool(value_string){
    if (value_string === "1" || value_string.toLowerCase() === "true"){
        return true
    }
    return false
}

async function isAdmin(jwt_user_token){
    const options = {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${jwt_user_token}`
        }
    }

    try{
        const response = await fetch('/api/users/is_admin', options)
        const result = await response.text()
        return toBool(result)
    }catch(error){
        console.error(error)
        return false
    }
}

async function loadChats() {
    const chats = await getChats()
    //console.log('meow')
    chats.forEach(chat => {
        if(chat.userPrompts[0].prompt.length > 14){
            updateHistory(chat.id, chat.userPrompts[0].prompt.substring(0, 14) + '...')
        }else{
            updateHistory(chat.id, chat.userPrompts[0].prompt)
        }
    });
}

async function userExists(jwt_user_token){
    const options = {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${jwt_user_token}`
        }
    }

    try {
        const id = await getUserId()
        const response = await fetch(`/api/users/${id}`, options)
        if (response.status != 200){
            throw new Error("user doesn't exist in some way, shape or form")
        }
        return true
    } catch (error) {
        console.error(error)
        return false
    }
}

async function getUserId() {
    const options = {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`
        }
    }
    try {
        const response = await fetch(`/api/users/id`, options)
        const data = await response.json();

        return data.id;
    } catch (error) {
        console.error(error)
    }
}

async function getAllUsers() {
    const options = {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`
        }
    }
    try {
        const response = await fetch('/api/users', options)
        const data = await response.json();

        return data;
    } catch (error) {
        console.error(error)
    }
}

var logout = async () => {
    deleteCookie('UserName')
    deleteCookie('passHash')
    deleteCookie(jwt_token_Header)
    await checkCookies();
}

function disable_send_button() {
    enabled = false;
    submitButton.style.pointerEvents = 'none'
    inputElement.value = ''
}

function enable_send_button() {
    enabled = true;
    submitButton.style.pointerEvents = 'all'
}

submitButton.addEventListener('click', getMessage)

buttonElement.addEventListener('click', createNewChat)

logOutElement.addEventListener('click', logout)

onkeyup = async (event) => {
    if (event.keyCode == 13) {
        submitButton.click();
    }
}