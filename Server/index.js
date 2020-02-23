var express = require('express');
var app = express();
var server = app.listen(3000);

app.use(express.static('public'));

app.get('/getlevel', getLevel);
function getLevel(req, res) {

}
