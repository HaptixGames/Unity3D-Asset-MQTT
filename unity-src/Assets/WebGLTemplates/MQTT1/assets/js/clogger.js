console.log("[browser-js] Loading clogger.js...");

var CLOGGER = function(cfg,message) {
	if(!cfg.enabled) return;
	console.log('[' + cfg.source + '] ' + message);
};