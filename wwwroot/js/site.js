
function loadStatusHtml(url)
{
    var spinner = document.getElementById("ajax-spinner");
    spinner.style.display = "block";

    fetch(url).then(function (response) {
        return response.text();
    }).then(function (html) {    
        // Convert the HTML string into a document object
        var parser = new DOMParser();
        var doc = parser.parseFromString(html, 'text/html');
        
        if (doc.documentElement.dataset.round != "0")
        {
            // Game has started - reload the page to move on
            window.location.reload();
        }

        var statusBox = document.getElementById('colander-status-box');
        var statusData = doc.querySelector('div#colander-status-box');
        statusBox.innerHTML = statusData.innerHTML;

        var teamsBox = document.getElementById('teams-box');
        var teamsData = doc.querySelector('div#teams-box');
        teamsBox.innerHTML = teamsData.innerHTML;

        var startBox = document.getElementById('game-start-box');
        if (startBox != null)
        {
            var startData = doc.querySelector('div#game-start-box');
            startBox.innerHTML = startData.innerHTML;
        }

        spinner.style.display = "none";

    }).catch(function (err) {
        console.warn('Ooops', err);
    });
} 

var gameArea = document.getElementById("game-area");
var round = gameArea.dataset.round;
var gameId = gameArea.dataset.gameId;

if (round == "0")
{
    var refreshTimer = setInterval(function() {
        loadStatusHtml("/data/pregame/" + gameId);
    }, 5000);
}