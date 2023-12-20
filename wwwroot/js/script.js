/**
 * Helper function for POSTing data as JSON with fetch.
 *
 * @param {Object} options
 * @param {string} options.url - URL to POST data to
 * @param {FormData} options.formData - `FormData` instance
 * @return {Object} - Response body from URL that was POSTed to
 */
var postFormDataAsJson = async({
    url,
    formData
  }) => {
    const plainFormData = Object.fromEntries(formData.entries());
    const formDataJsonString = JSON.stringify(plainFormData);
  
    const fetchOptions = {
      method: "POST",
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
  /**
   * Event handler for a form submit event.
   * @see https://developer.mozilla.org/en-US/docs/Web/API/HTMLFormElement/submit_event
   * @example const exampleForm = document.getElementById("example-form");
   *          exampleForm.addEventListener("submit", handleFormSubmit);
   * @param {SubmitEvent} event
   */
  var handleFormSubmit = async(event) => {
    event.preventDefault();
    const form = event.currentTarget;
    const url = form.action;
  
    try {
      const formData = new FormData(form);
      const responseData = await postFormDataAsJson({
        url,
        formData
      });
      console.log({
        responseData
      });
    } catch (error) {
      console.error(error);
    }
  }

    /**
   * Event handler for a form submit event.
   * @see https://developer.mozilla.org/en-US/docs/Web/API/HTMLFormElement/submit_event
   * @example const exampleForm = document.getElementById("example-form");
   *          exampleForm.addEventListener("submit", handleFormSubmit);
   * @param {SubmitEvent} event
   */
    var handleGetByIdFormSubmit = async(event) => {
        event.preventDefault();
        const form = event.currentTarget;
        const formElements = form.querySelectorAll('input');
        console.log(formElements)
        const userId = formElements.querySelector('#get_userbyid_txt').value
        console.log(userId)
        /*
        const url = `${form.action}/${userId}`;
        form.submit()*/
      }
  
  document.querySelector("form[name='addUser']")
    .addEventListener("submit", handleFormSubmit)
  document.querySelector("form[name='updateUser']")
    .addEventListener("submit", handleFormSubmit)
  document.querySelector("form[name='getUserById']")
    .addEventListener("submit", handleGetByIdFormSubmit)