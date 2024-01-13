import { jwtDecode } from "jwt-decode";

const jwt_token_Header = "erai-jwt-token";

window.onload = function(){
  checkCookies();
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
  let erai_jwt = await getCookie(jwt_token_Header);
  console.log(decodeURIComponent(document.cookie));
  if (username == "" || pw == "" || erai_jwt == "") {
    window.location.replace("http://localhost:5274");
  }
}

var deleteCookie = async (name) => {
  document.cookie = name + "=;expires=" + new Date(0).toUTCString()
}



var postFormDataAsJson = async ({
  url,
  formData,
  method
}) => {
  const plainFormData = Object.fromEntries(formData.entries());
  const formDataJsonString = JSON.stringify(plainFormData);

  let fetchOptions = {
    method: `${method}`,
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      "Authorization": `Bearer ${await getCookie(jwt_token_Header)}`
    },
    body: formDataJsonString,
  };
  console.log(fetchOptions.headers)
  let response;
  method = method.toUpperCase();
  switch (method) {
    case "POST":
      alert("about to post" + formDataJsonString)
      response = await fetch(url, fetchOptions);

      if (!response.ok) {
        const errorMessage = await response.text();
        throw new Error(errorMessage);
      }
      return response.json();
      break;

    case "PUT":
      url = `${url}/${formData.get("Id")}`;

      alert("about to put" + formDataJsonString)
      console.log(formData)
      console.log(formDataJsonString)
      response = await fetch(url, fetchOptions)

      if (!response.ok) {
        const errorMessage = await response.text();
        throw new Error(errorMessage)
      }
      return response;
      break;

    case "DELETE":
      url = `${url}/${formData.get("id")}`;

      alert('about to delete ' + formDataJsonString)
      response = await fetch(url, fetchOptions)

      if (!response.ok) {
        const errorMessage = await response.text();
        throw new Error(errorMessage)
      }
      return response;
      break;

    default:
      return;
  }
}


var handleFormSubmit = async (event) => {
  event.preventDefault();
  const form = event.currentTarget;
  const url = form.action;

  try {
    const formData = new FormData(form);
    let method = typeof form.method == typeof "" ? form.method : form.method.value
    if (form.querySelector('input#method') != null) {
      formData.delete("_METHOD")
    }
    if (formData.has("passHash")) {

    }

    /*
    console.log(form.method)

    console.log("method " + method)
    console.log(method + " formData: " + formData + " form " + form)
    console.log(formData)
    */

    const responseData = await postFormDataAsJson({
      url,
      formData,
      method
    });
    console.log({
      responseData
    });
    form.reset()
  } catch (error) {
    console.error(error);
  }
}


var handleGetByIdFormSubmit = async (event) => {
  event.preventDefault();
  const form = event.currentTarget;
  const action = form.action
  const formElements = form.querySelectorAll('input');
  //console.log(formElements)
  //const userId = formElements.querySelector('#get_userbyid_txt').value
  let userId;
  formElements.forEach(element => {
    if (element.id == "get_userbyid_txt") {
      userId = element.value;
    }
  });
  //console.log(userId)

  const url = `${action}/${userId}`;

  let fetchOptions = {
    method: `GET`,
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
      "Authorization": `Bearer ${await getCookie(jwt_token_Header)}`
    }
  };
  response = await fetch(url, fetchOptions);
  //form.submit()

  form.reset()
  if (!response.ok) {
    const errorMessage = await response.text();
    throw new Error(errorMessage)
  }
  console.log(response);
  console.log(jwtDecode(await getCookie(jwt_token_Header)))
}

document.querySelectorAll("form[class='generalForms']").forEach(element => {
  element.addEventListener("submit", handleFormSubmit)
});
document.querySelector("form[name='getUserById']")
  .addEventListener("submit", handleGetByIdFormSubmit)