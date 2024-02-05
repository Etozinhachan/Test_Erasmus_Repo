const jwt_token_Header = "erai-jwt-token";

var sleep = ms => new Promise(r => setTimeout(r, ms));


window.onload = async() =>{
    await checkCookies()
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
        window.location.replace(`${window.location.href}`.replace('tefasf.html', ''));
    }
}

var deleteCookie = name => {
    document.cookie = name + "=;expires=" + new Date(0).toUTCString()
}

const form = document.querySelector('form').addEventListener('submit', async (event) => await submitHandle(event))

async function submitHandle(event){
    event.preventDefault()
    const form = document.querySelector('form')
    const formData = new FormData(form)
    const users = await loadUsers()
    for (const [name,value] of formData) {
        console.log(name, ":", value)
      }
     callAdmin(users[0].id, formData.get("admin")) 
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
   return users;
}

async function callAdmin(userId, isAdmin) {
    const options2 = {
        method: 'PATCH',
        headers: {
            'Authorization': `Bearer ${await getCookie(jwt_token_Header)}`, // por ai o jwt token guardado no cookie
            'Content-Type': 'application/json'
        }
    }
    console.log(isAdmin)
     if (isAdmin == 'on') {
        isAdmin = true;
    } else {
        isAdmin = false;
    } 
    console.log(`/api/Users/set_admin/${userId}?admin=${isAdmin}`);
    const response2 = await fetch(`/api/Users/set_admin/${userId}?admin=${isAdmin}`, options2)

    console.log(await response2.json())

    if (response2.status != 200) {
        throw new Error()
    }
}

