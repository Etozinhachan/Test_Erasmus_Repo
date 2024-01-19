const tableDiv = document.querySelector('.user-list')
const table = document.querySelector('.user-list-table')
const tableBody = document.querySelector('.user-list-table-body')
const userForm = document.querySelector('#userForm')
const jwt_token_Header = "erai-jwt-token";


const editModal = document.querySelector(".edit_modal");
const dismissSucessMsgBtn = document.querySelector(".dismiss__btn");

var sleep = ms => new Promise(r => setTimeout(r, ms));

window.onload = async () => {
    checkCookies();
    /*
        need to implement a function to load all users with this element structure
        <tr>
            <td>rawr</td>
            <td>1251</td>
            <td><button class="EditButton">Edit</button><button class="DeleteButton">Delete</button></td>
        </tr>
    */
    await loadUsers();
}

var setCookie = async (cname, cvalue, duration) => {
    const d = new Date();
    var days = duration.days,
        hours = duration.hours,
        minutes = duration.minutes,
        seconds = duration.seconds,
        miliseconds = duration.miliseconds
    if (days) {
        d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
    }
    if (hours) {
        d.setTime(d.getTime() + (hours * 60 * 60 * 1000));
    }
    if (minutes) {
        d.setTime(d.getTime() + (minutes * 60 * 1000));
    }
    if (seconds) {
        d.setTime(d.getTime() + (seconds * 1000));
    }
    if (miliseconds) {
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
    if (username == "" || pw == "" || erai_jwt == "" || !(await userExists(erai_jwt))) {
        deleteCookie('UserName')
        deleteCookie('passHash')
        deleteCookie(jwt_token_Header)
        window.location.replace(`${window.location.href}`.replace('dashboard.html', ''));
    }
}

var deleteCookie = name => {
    document.cookie = name + "=;expires=" + new Date(0).toUTCString()
}

async function loadUsers() {
    const users = await getAllUsers();
    /*
        need to implement a function to load all users with this element structure
        <tr>
            <td>rawr</td>
            <td>1251</td>
            <td><button class="EditButton">Edit</button><button class="DeleteButton">Delete</button></td>
        </tr>
    */

    users.forEach(user => {
        const row = buildRow(user)
        tableBody.append(row)
    });
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

function buildRow(user) {
    const row = document.createElement('tr');
    const cellUsername = document.createElement('td')
    const cellId = document.createElement('td')
    const cellButons = document.createElement('td')
    const editButton = document.createElement('button')
    const deleteButton = document.createElement('button')


    cellUsername.textContent = `${user.userName}`
    cellId.textContent = `${user.id}`

    editButton.className = 'EditButton'
    editButton.textContent = 'Edit'

    deleteButton.className = 'DeleteButton'
    deleteButton.textContent = 'Delete'

    row.id = `_${cellId.textContent}`

    editButton.addEventListener('click', async () => await editButtonHandler(row.id))
    deleteButton.addEventListener('click', async () => await deleteButtonHandler(row.id))

    cellButons.append(editButton)
    cellButons.append(deleteButton)
    row.append(cellId)
    row.append(cellUsername)
    row.append(cellButons)
    return row;
}


async function editButtonHandler(id) {
    const userId = id.substring(id.indexOf('_') + 1)
    /*     const modalTitle = document.querySelector('.edit_modal_head p')
        modalTitle.textContent += `for id ${id}` */
    const hasUpdatePerm = await checkUserPerms(await getCookie(jwt_token_Header), userId)
    if (hasUpdatePerm) {
        editModal.showModal();
        const userUpdateForm = document.querySelector('#update_user_form')
        userUpdateForm.querySelector('#update_user_submit').addEventListener('click', async () => {
            const formData = new FormData(userUpdateForm)
            await updateUser(userId, formData.get("UserName"), formData.get("passHash"))
            editModal.close();
            await checkCookies();
            window.location.href = `${window.location.href}`
        })
    }
}

async function updateUser(userId, username, password) {
    const options = {
        method: 'PUT',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            "UserName": `${username}`,
            "passHash": `${password}`
        })
    }
    try {
        const response = await fetch(`/api/users/${userId}`, options)
        if (response.status != 204) {
            throw new Error()
        }
    } catch (error) {
        console.error(error)
    }

}

async function deleteButtonHandler(id) {
    var result = confirm("Want to delete?");
    if (result) {
        const userRowToDelete = tableBody.querySelector(`#${id}`)
        userRowToDelete.remove()
        const userId = id.substring(id.indexOf('_') + 1)
        const hasDeletePerm = await checkUserPerms(await getCookie(jwt_token_Header), userId)
        if (hasDeletePerm) {
            await deleteUser(userId)
            await checkCookies()
        }
    }
}

function toBool(value_string) {
    if (value_string === "1" || value_string.toLowerCase() === "true") {
        return true
    }
    return false
}

async function checkUserPerms(jwt_user_token, id) {
    return (await isThisUser(id, jwt_user_token)) || (await checkAdmin(jwt_user_token))
}

async function getUserById(id, jwt_user_token) {
    const options = {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${jwt_user_token}`
        }
    }

    try {
        const response = await fetch(`/api/users/${id}`, options)
        const result = await response.json()
        return result
    } catch (error) {
        console.error(error)
    }
}

async function isThisUser(id, jwt_user_token) {
    return await getUserById(id, jwt_user_token).id == id
}

async function checkAdmin(jwt_user_token) {
    const options = {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${jwt_user_token}`
        }
    }

    try {
        const response = await fetch('/api/users/is_admin', options)
        const result = await response.text()
        return toBool(result)
    } catch (error) {
        console.error(error)
        return false
    }
}

async function deleteUser(id) {
    const options = {
        method: 'DELETE',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
        },
    }
    const response = await fetch(`/api/users/${id}`, options)
    if (response.status != 204) {
        throw new Error()
    }
}


async function userExists(jwt_user_token) {
    const options = {
        method: "GET",
        headers: {
            'Authorization': `Bearer ${jwt_user_token}`
        }
    }

    try {
        const id = await getUserId(await getCookie('UserName'))
        console.log(id)
        const response = await fetch(`/api/users/${id}`, options)
        console.log(await response.json())
        if (response.status != 200) {
            throw new Error("user doesn't exist in some way, shape or form")
        }
        return true
    } catch (error) {
        console.error(error)
        return false
    }
}

async function getUserId(username) {
    const users = await getAllUsers()
    for (let index = 0; index < users.length; index++) {
        if (users[index].userName == username) {
            return users[index].id;
        }
    }
}

async function createAccount() {
    event.preventDefault()
    const options = {
        method: `POST`,
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            "UserName": `${userForm.querySelector('#name').value}`,
            "passHash": `${userForm.querySelector('#pass').value}`
        }),
    };
    //alert("about to post" + formDataJsonString)
    const response = await fetch('/api/users/register', options);
    window.location.replace(`${window.location.href}`);
}




dismissSucessMsgBtn.addEventListener("click", function () {
    editModal.close();
});