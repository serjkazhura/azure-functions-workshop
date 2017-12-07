function showSuccessMessage(message) {
    var el = document.querySelector('#eventCreated');
    el.classList.remove('hide');
    el.classList.remove('alert-danger');
    el.classList.add('alert-success');
    el.innerText = message;
}

function showErrorMessage(message) {
    var el = document.querySelector('#eventCreated');
    el.classList.remove('hide');
    el.classList.remove('alert-success');
    el.classList.add('alert-danger');
    el.innerText = message;
}

function createEvent() {
    var createEventUrl = 'https://sk-workshop-functions-admin.azurewebsites.net/api/CreateEvent';
    var invitees = Array.from(document.querySelectorAll('#invited>option'))
                        .filter(o=>o.selected)
                        .map(o => { return { 'Name': o.text, 'Email': o.value }});    
    var body ={
        "EventDateAndTime": document.querySelector('#eventDateTime').value,
        "Location": document.querySelector('#eventLocation').value,
        "Invitees": invitees
    };
    //console.log();

    /*fetch(createEventUrl + "?detailsJson=" + encodeURIComponent(JSON.stringify(body)), { 
            method: 'get', 
            credentials: 'include',
            //body: JSON.stringify(body),
    	    headers: new Headers({'Content-Type': 'text/json'})
     })*/
    fetch(createEventUrl, { 
            method: 'post', 
            credentials: 'include',
            body: JSON.stringify(body),
    	    headers: new Headers({'Content-Type': 'text/json'})
     })
    .then(function(resp) {
        if (resp.status === 200)
        {
            showSuccessMessage("Event created successfully");
        }
        else
        {
            showErrorMessage("Failed to create event: " + resp.statusText);
            console.warn(resp.status, resp);
        }        
    })
    .catch(err => showErrorMessage("Error creating event: " + err.message));
}
// set up some defaults
document.querySelector('#eventDateTime').value ="2017-07-08T17:00";
document.querySelector('#eventLocation').value ="Wildern";
document.querySelector('#submitButton').addEventListener('click', createEvent);