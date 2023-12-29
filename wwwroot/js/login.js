let loginForm = document.getElementsByName("login");

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
    alert("about to post" + formDataJsonString)
    const response = await fetch(url, fetchOptions);

    if (!response.ok) {
        const errorMessage = await response.text();
        throw new Error(errorMessage);
    }
    return response.json();

}

var setCookie = async (cname, cvalue, exdays) => {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
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
    if (username != "" && pw != "") {
        var usernameInput = loginForm.querySelector('input[name="UserName"]');
        var pwInput = loginForm.querySelector('input[name="passHash"]"');
        usernameInput.value = username;
        pwInput.value = pw;
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
        form.reset()
        if (form != loginForm){
            setCookie("UserName", formData.get("UserName"), 365)
            setCookie("PassHash", formData.get("passHash"), 365)
        }
    } catch (error) {
        console.error(error);
    }
}



document.querySelector("form[name='register']")
    .addEventListener("submit", handleFormSubmit)

document.querySelector("form[name='login']")
    .addEventListener("submit", handleFormSubmit)