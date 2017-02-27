var https = require('https');
var options = { 'hostname': 'collectors.au.sumologic.com',
                'protocol':'https:',
                'port':'443',
                'path': '',
                'method': 'POST'
            };

function logToSumo(record, context) {
    
    var req = https.request(options, function(res) {
                var body = '';
                body += JSON.stringify(record);
                res.setEncoding('utf8');
                res.on('data', function(body) { body });
                res.on('end', function() {
                    console.log('Successfully processed HTTPS response');
                    context.succeed(); 
                });
            });

    
    var finalData = '';
    var isCompressed = false;

    var finishFnc = function() {
            console.log("End of stream");
            req.end();
            context.succeed(); 
    };
    
    req.write('Record: ' + record);
    req.end();
}

exports.handler = function(event, context, callback) {
    if(!event.Records) {
        console.log('no record in event')
    } else {
        event.Records.forEach((record) => {
            logToSumo(record.dynamodb, context);
        });
        callback(null, `Successfully processed ${event.Records.length} records.`);
    }
};