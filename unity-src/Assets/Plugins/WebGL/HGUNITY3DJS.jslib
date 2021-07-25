var HGUNITY3DJS = {
	//$UNITY3D__deps: [''],
	//$UNITY3D__postset: '',
	$UNITY3D: {
		log: {
			source: "HGUNITY3DJS",
			enabled: true
		}
	},
	Ready: function(gameObjectName) {
		__gameObjectName = Pointer_stringify(gameObjectName);
		CLOGGER(UNITY3D.log,"Dispatching 'onUnityReady' JS event.");

		var event = new CustomEvent('onUnityReady', { 
			detail: { 
				gameObjectName: __gameObjectName
			},
			bubbles: true,
			cancelable: false
		});

		return document.dispatchEvent(event);
	}
};

autoAddDeps(HGUNITY3DJS, '$UNITY3D');
mergeInto(LibraryManager.library, HGUNITY3DJS);