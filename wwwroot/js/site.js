
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

        var statusBox = document.getElementById("colander-status-box");
        var statusData = doc.querySelector('div#colander-status-box');
        statusBox.innerHTML = statusData.innerHTML;

        var teamsBox = document.getElementById("teams-box");
        var teamsData = doc.querySelector('div#teams-box');
        teamsBox.innerHTML = teamsData.innerHTML;

        spinner.style.display = "none";

    }).catch(function (err) {
        // There was an error
        console.warn('Something went wrong.', err);
    });
	var xhr = new XMLHttpRequest();
	xhr.onreadystatechange=function()
	{
		if(xhr.readyState == 4)
		{
			if(xhr.status == 200)
			{
				storage.innerHTML = getBody(xhr.responseText);
			}
		}
	};

	xhr.open("GET", url , true);
	xhr.send(null); 
} 

var gameArea = document.getElementById("game-area");
var round = gameArea.dataset.round;
var gameId = gameArea.dataset.gameId;

if (round == "0")
{
    var refreshTimer = setInterval(function(){
        //if (timeleft <= 0) {
        //    clearInterval(refreshTimer);
        //    setTimeout(function(){ location.reload(); }, 10500);
        //}
        loadStatusHtml("/status/" + gameId);
    }, 5000);
}