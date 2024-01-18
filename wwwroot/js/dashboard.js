const tableDiv = document.querySelector('.user-list')
const table = document.querySelector('.user-list-table')
const tableBody = document.querySelector('.user-list-table-body')
const jwt_token_Header = "erai-jwt-token";


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
    if (username == "" || pw == "" || erai_jwt == "") {
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

    editButton.addEventListener('click', () => editButtonHandler(row.id))
    deleteButton.addEventListener('click', () => deleteButtonHandler(row.id))

    cellButons.append(editButton)
    cellButons.append(deleteButton)
    row.append(cellId)
    row.append(cellUsername)
    row.append(cellButons)
    return row;
}


function editButtonHandler(id){

}

function deleteButtonHandler(id){
    var result = confirm("Want to delete?");
    if (result) {
        const userRowToDelete = tableBody.querySelector(`#${id}`)
        userRowToDelete.remove()
        const userId = id.substring(id.indexOf('_') + 1)
        console.log(userId)
    }
}