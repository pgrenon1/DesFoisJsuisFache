var express = require('express');
var app = express();
var port = 5500;
var server = app.listen(port);

console.log("Server started on port " + port);

app.use(express.static('public'));

app.get('/getlevel/:guid', GetLevel);
function GetLevel(req, res) {
    console.log(req);

    var guid = req.params.guid;

    // res.send("Sending level " + guid);
}

app.get('/savelevel/')