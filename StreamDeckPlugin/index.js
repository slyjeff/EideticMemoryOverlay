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

    let selectDeckGroup = document.getElementById('selectDeckGroup');
    let selectPlayerGroup = document.getElementById('selectPlayerGroup');

    if (actionInfo.action === "arkhamoverlay.cardbutton") {
        //if we have a saved value, initialize the deck drop down
        var valueToSelect = actionInfo.payload.settings['deck'];
        if (!valueToSelect) {
            valueToSelect = 'player1';
        }

        let element = document.getElementById('selectDeck');
        element.value = valueToSelect;

        selectPlayerGroup.style.display = "none";
    } else if (actionInfo.action === "arkhamoverlay.showdecklist") {
        //if we have a saved value, initialize the deck drop down
        var valueToSelect = actionInfo.payload.settings['deck'];
        if (!valueToSelect) {
            valueToSelect = 'player1';
        }

        let element = document.getElementById('selectPlayer');
        element.value = valueToSelect;

        selectDeckGroup.style.display = "none";
    } else {
        let element = document.getElementById('selectDeckGroup');
        selectDeckGroup.style.display = "none";
        selectPlayerGroup.style.display = "none";
    }
}

function sendCardButtonValueToPlugin(value, param) {
    if (websocket) {
    const json = {
            "action": "arkhamoverlay.cardbutton",
            "event": "sendToPlugin",
            "context": uuid, 
            "payload": {
                [param] : value
            }
        };
        websocket.send(JSON.stringify(json));
    }
}

function sendDeckListValueToPlugin(value, param) {
    if (websocket) {
    const json = {
            "action": "arkhamoverlay.showdecklist",
            "event": "sendToPlugin",
            "context": uuid, 
            "payload": {
                [param] : value
            }
        };
        websocket.send(JSON.stringify(json));
    }
}