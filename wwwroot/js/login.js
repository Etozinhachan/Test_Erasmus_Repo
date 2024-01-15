let loginForm = document.getElementById("login");
const jwt_token_Header = "erai-jwt-token";
const cookieDuration = 10;

var sleep = ms => new Promise(r => setTimeout(r, ms));

window.onload = function(){
    checkCookies();
}

var postFormDataAsJson = async ({
    url,
    formData
}) => {
    const plainFormData = Object.fromEntries(formData.entries());
    const formDataJsonString = JSON.stringify(plainFormData);

    let fetchOptions = {
        method: `POST`,
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
        },
        body: formDataJsonString,
    };
    //alert("about to post" + formDataJsonString)
    const response = await fetch(url, fetchOptions);

    if (!response.ok) {
        const errorMessage = await response.text();
        throw new Error(errorMessage);
    }
    //console.log(response.headers.get("erai-jwt-token"))
    if (url.indexOf("login") == -1 && url.indexOf("register") == -1){
        return response.json();
    }else{
        return response;
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

var getCookie = async (cname) => {
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
    let username = await getCookie("UserName");
    let pw = await getCookie("passHash");
    let erai_jwt = await getCookie(jwt_token_Header);
    console.log(decodeURIComponent(document.cookie));
    if (username != "" && pw != "" && erai_jwt != "") {
        var usernameInput = loginForm.querySelector('input[name="UserName"]');
        var pwInput = loginForm.querySelector('input[name="passHash"]');
        usernameInput.value = username;
        pwInput.value = pw;
        
        usernameInput.style.fontSize = "0px";
        usernameInput.style.minWidth ="170px";
        usernameInput.style.minHeight = "15px";

        pwInput.style.fontSize = "0px";
        pwInput.style.minWidth ="170px";
        pwInput.style.minHeight = "15px";

        loginForm.querySelector('#post_user_btn').click();
    }
}

var handleFormSubmit = async (event) => {
    event.preventDefault();
    const form = event.currentTarget;
    const url = form.action;

    try {
        const formData = new FormData(form);

        /*
        console.log(form.method)
  
        console.log("method " + method)
        console.log(method + " formData: " + formData + " form " + form)
        console.log(formData)
        */

        const responseData = await postFormDataAsJson({
            url,
            formData
        });
        console.log({
            responseData
        });
        
        /* responseData.headers.forEach(item => {
            console.log(item);
        }); */
        form.reset()
        if (responseData.headers.has(jwt_token_Header)){
            await setCookie("UserName", formData.get("UserName"), {minutes: cookieDuration})
            await setCookie("passHash", formData.get("passHash"), {minutes: cookieDuration})
            await setCookie(jwt_token_Header, responseData.headers.get(jwt_token_Header), {minutes: cookieDuration});
        }
        window.location.replace("http://localhost:5274/newChat.html");
    } catch (error) {
        console.error(error);
    }
}



document.querySelector("form[name='register']")
    .addEventListener("submit", handleFormSubmit)

document.querySelector("form[name='login']")
    .addEventListener("submit", handleFormSubmit)