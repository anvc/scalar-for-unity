mergeInto(LibraryManager.library, {
	
	ReturnPosition3D: function(str) {
		console.log("ReturnPosition3D "+Pointer_stringify(str));
		window.parent.postMessage(Pointer_stringify(str), window.location);
	}

});