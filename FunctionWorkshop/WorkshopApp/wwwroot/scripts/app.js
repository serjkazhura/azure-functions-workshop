function showError (err) {
    document.querySelector('#loadingMessage').style.display = "block";
    document.querySelector('#loadingMessage').innerText = err;    
}

function updateEventDetails(data) {
    if (!data) return;
    document.querySelector('#loadingMessage').style.display = "none";
    document.querySelector('.eventDetails').style.display = "block";
    var d = moment(data.EventDateAndTime);
    const matchDate = document.querySelector('#matchDate');
    matchDate.textContent = d.format('LL');
    const matchTime = document.querySelector('#matchTime');
    matchTime.textContent = d.format('LT');
    const matchLocation = document.querySelector('#matchLocation');
    matchLocation.textContent = data.Location;
    var playing = 0;
    var notPlaying = 0;
    var maybePlaying = 0;
    var notResponded = 0;
    Array.from(document.querySelectorAll('.players')).forEach(el => el.innerText = "");

    document.querySelector('.playing').querySelector('.players').innerText = "";
    document.querySelector('.notPlaying').querySelector('.players').innerText = "";
    document.querySelector('.notPlaying').querySelector('.players').innerText = "";
    data.Responses.forEach(function(resp) {
        let targetDiv = "notResponded";
        if (resp.Playing === "yes") {
            //div.innerText += " ⚽";
            targetDiv = "playing";
            playing++;
        }
        else if (resp.Playing === "no") {
            //div.innerText += " 💩";
            targetDiv = "notPlaying";
            notPlaying++;
        }
        else if (resp.Playing === "maybe") {
            targetDiv = "maybe";
            maybePlaying++;
        }
        else {
            targetDiv = "notResponded";
            notResponded++;
        }
        const playersDiv = document.querySelector('.' + targetDiv).querySelector('.players');
        playersDiv.innerText += ((playersDiv.innerText ? ", " : "") + resp.Name);
    });
    document.querySelector('#playingCount').innerText = playing;
    document.querySelector('#maybeCount').innerText = maybePlaying;
    document.querySelector('#notPlayingCount').innerText = notPlaying;
    document.querySelector('#notRespondedCount').innerText = notResponded;


    var selectedResponse = document.querySelector('button[data-response=' + data.MyResponse + ' ]');
    if (selectedResponse) {
        selectedResponse.classList.add('selected');
    }
}

function loadEventDetails(eventId, responseCode) {
    var getEventDetailsUrl = 
        `https://sk-workshop-functions.azurewebsites.net/api/event/${eventId}/responseCode/${responseCode}`;
    fetch(getEventDetailsUrl)
    .then(function(resp) {
        if (resp.status === 200)
        {
            return resp.json();
        }
        else
        {
            document.querySelector('#loadingMessage').innerText = resp;
            console.warn(resp.status, resp);
        }        
    })
    .then(updateEventDetails)
    .catch(err => showError(err.message));
}

// URLSearchParams not available in Edge
function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

var eventId = getParameterByName('event');
var responseCode = getParameterByName('code');
/*if (!eventId) {
    showError("Need an event id");
}
else if (!responseCode) {
    showError("Missing response code");
}
else {
    loadEventDetails(eventId, responseCode);
}*/
loadEventDetails(eventId, responseCode);

var responseButtons = Array.from(document.querySelectorAll('button[data-response]'));

function respond(e) {
    responseButtons.forEach(r => r.classList.remove('selected'));
    e.target.classList.add('selected');
    var selectedResponse = e.target.dataset.response;
    var updateResponseUrl = 'https://sk-workshop-functions.azurewebsites.net/api/event/' 
                                + eventId + '/response/' + responseCode;
    var body = JSON.stringify({ isPlaying: selectedResponse});
    console.log(`responding to ${updateResponseUrl} with ${body}`);
    fetch(updateResponseUrl, { 
            method: 'PUT', 
            body: body,
    	headers: new Headers({'Content-Type': 'text/json'})    
     })
        .then(function(resp) {
            if (resp.status != 200) {
                showError("failed to update response");
                console.error(resp);
            }
            else {
                console.log("updated response OK");
                loadEventDetails(eventId, responseCode);
            }
        })
        .catch(err => showError(err.message));

}

responseButtons.forEach(r => r.addEventListener('click', respond));
