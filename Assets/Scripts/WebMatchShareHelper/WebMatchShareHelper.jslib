

var LibraryMatchShareHelper = {
	
	// copy a shareable url which will join the current match
	JS_ShareHelper_Share: function(matchCodeUTF8) {
		const matchCode = UTF8ToString(matchCodeUTF8);
		console.log("share match code: ", matchCode);
		
		var url = new URL(window.location.href);
		url.searchParams.set('match', matchCode);
		var shareUrl = url.toString();
		
		var type = "text/plain";
		var blob = new Blob([shareUrl], { type: type });
		var item = new ClipboardItem({ [type]: blob });

		navigator.clipboard
			.write([item])
			.then(function() {
				console.log("Copied share URL to clipboard:", shareUrl);
			})
			.catch(function(error) {
				console.error("Failed to copy text:", error);
			});
	},
	
	// returns a match code if this session was shared from somewhere else
	JS_ShareHelper_GetShared: function() {
		function returnStr(str){
			if(!str) return null;

			var bufferSize = lengthBytesUTF8(str) + 1;
			var buffer = _malloc(bufferSize);
			stringToUTF8(str, buffer, bufferSize);
			return buffer;
		}
		
		const urlParams = new URLSearchParams(window.location.search);
		const matchCode = urlParams.get('match');
		return returnStr(matchCode);
	}
};

mergeInto(LibraryManager.library, LibraryMatchShareHelper);
