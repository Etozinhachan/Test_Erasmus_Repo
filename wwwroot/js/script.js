
var postFormDataAsJson = async({
    url,
    formData,
    method
  }) => {
    const plainFormData = Object.fromEntries(formData.entries());
    const formDataJsonString = JSON.stringify(plainFormData);
  
    const fetchOptions = {
      method: `${method}`,
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: formDataJsonString,
    };
    let response;
    switch(method){
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

         if (!response.ok){
          const errorMessage = await response.text();
          throw new Error(errorMessage)
         }
         return response;
        break;
      
      default:
        return;
    }
    
    
    
  }
  

  var handleFormSubmit = async(event) => {
    event.preventDefault();
    const form = event.currentTarget;
    const url = form.action;
  
    try {
      const formData = new FormData(form);
      let method = typeof form.method == typeof String ? form.method : form.method.value
      
      if (form.querySelector('input#method') != null){
        formData.delete("_METHOD")
      }

      console.log("method " + method)
      console.log(method + " formData: " + formData + " form " + form)
      console.log(formData)
      
      const responseData = await postFormDataAsJson({
        url,
        formData,
        method
      });
      console.log({
        responseData
      });
    } catch (error) {
      console.error(error);
    }
  }

   
    var handleGetByIdFormSubmit = async(event) => {
        event.preventDefault();
        const form = event.currentTarget;
        const action = form.action
        const formElements = form.querySelectorAll('input');
        //console.log(formElements)
        //const userId = formElements.querySelector('#get_userbyid_txt').value
        let userId;
        formElements.forEach(element => {
          if (element.id == "get_userbyid_txt"){
            userId = element.value;
          }
        });
        //console.log(userId)
        
        const url = `${action}/${userId}`;
        form.action = url;
        form.submit()

        form.action = action
      }
  
  document.querySelector("form[name='addUser']")
    .addEventListener("submit", handleFormSubmit)
  document.querySelector("form[name='updateUser']")
    .addEventListener("submit", handleFormSubmit)
  document.querySelector("form[name='getUserById']")
    .addEventListener("submit", handleGetByIdFormSubmit)