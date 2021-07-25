console.log("[browser-js] Loading client.js...");

window.addEventListener('onUnityReady', function (e) { 
	console.log("[browser-js] Received 'onUnityReady' JS event.");
	
	var requestLocation = function() {
		$.ajax({
			url: "https://geoip-db.com/jsonp",
			jsonpCallback: "callback",
			dataType: "jsonp",
			success: function( location, textStatus, xhr ) {
		    	var geolocatorResponse = { error: { code: xhr.status, message: textStatus }, location: location };
		    	gameInstance.SendMessage(e.detail.gameObjectName, 'HGGEOLOCATORJS_OnLocation', JSON.stringify(geolocatorResponse)); 
			},
			error: function( xhr, textStatus, error ) {
				var geolocatorResponse = { error: { status: xhr.status, code: error, message: textStatus }, location: {} };
		    	gameInstance.SendMessage(e.detail.gameObjectName, 'HGGEOLOCATORJS_OnLocation', JSON.stringify(geolocatorResponse));
			}
		});	
	}	

	var toggleConsoleButton = document.getElementById('btnToggleConsole');

	toggleConsoleButton.addEventListener('click', function() {
		gameInstance.SendMessage(e.detail.gameObjectName, 'HGGENERICJS_OnToggleConsole', 0);
	});

	var requestLocationButton = document.getElementById('btnRequestLocation');

	requestLocationButton.addEventListener('click', function() {
		requestLocation();
	});

	requestLocation();	
});