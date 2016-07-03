var http = require('http');
var url = require('url');

// Everything outside of any block is in the main.
// Does code from left to right:
// Anything without var is instant Global
// Interchangeable Strings: "inside 'string'" or '"Inside" string'

http.createServer(function(req, res) {
	var method = req.method;
	var path = url.parse(req.url).pathname;
	var pathParams = path.split('/');
	var pathname = pathParams[1];
	var id = pathParams[2];
}).listen(8082);

// Direct JSON
var myJson = { 'Content-Type' : 'application/json' };
str = JSON.stringify(myJson);

pathParams[1];
if (method === 'GET') { ... } // === Identity: Type AND Content must be equal
switch (...) {
	case '...': break;
	default: break;
}
	for(var i = 0; i < kunden.kunde.length;i++){
		bodyJSON +="Name: "+ kunden.kunde[i].name + " ID: " + kunden.kunde[i].id  +"\n";
        }	

kundenName = "<?xml version='1.0'>"+"<KundenName>"+kunden.kunde[id - 1].name+"</KundenName>";
kundenID = "<KundenID>"+kunden.kunde[id - 1].id+"</KundenID>";
custResult = kundenName+kundenID;
res.end(custResult);

// FileIO
var fs = require('fs');
fs.readFile('./Kunden.json', function(err, kundenJSON) {
	if (err) {
		console.log(err);
	}
	kunden = JSON.parse(kundenJSON);
	// Bsp f・ Zugriff auf Parameter ・er index:
	
});

// Very detailed info is here
http://www.jslint.com/help.html