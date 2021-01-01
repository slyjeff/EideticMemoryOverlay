var websocket = null,
    uuid = null,
    actionInfo = {},
    coordinates;

function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inUUID;
    actionInfo = JSON.parse(inActionInfo); // cache the info
    websocket = new WebSocket('ws://localhost:' + inPort);

    // if connection was established, the websocket sends
    // an 'onopen' event, where we need to register our PI
    websocket.onopen = function () {
        var json = {
            event:  inRegisterEvent,
            uuid:   inUUID
        };
        // register property inspector to Stream Deck
        websocket.send(JSON.stringify(json));
    }

    coordinates = actionInfo.payload.coordinates;

    var valueToSelect = actionInfo.payload.settings['deck'];
    if (!valueToSelect) {
        valueToSelect = 'player1';
    }

    //if we have a saved value, initialize the deck drop down
    let element = document.getElementById('selectDeck');
    element.value = valueToSelect;


    websocket.onmessage = function (evt) {
        // Received message from Stream Deck
        var jsonObj = JSON.parse(evt.data);
        var event = jsonObj['event'];
        var jsonPayload = jsonObj['payload'];

        if(event == "didReceiveSettings") {
            sendSettingsToPlugin(['settings']);
        }
    };
}

function sendValueToPlugin(value, param) {
    if (websocket) {
    const json = {
            "action": "arkhamoverlay.cardbutton",
            "event": "sendToPlugin",
            "context": uuid, 
            "payload": {
                [param] : value,
                "coordinates" : coordinates
            }
        };
        websocket.send(JSON.stringify(json));
    }
}