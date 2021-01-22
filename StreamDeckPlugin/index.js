var websocket = null,
    uuid = null,
    actionInfo = {},
    coordinates;

function isPlayerAction() {
    if (actionInfo.action === "arkhamoverlay.showdecklist") {
        return true;
    }

    if (actionInfo.action === "arkhamoverlay.trackhealth") {
        return true;
    }

    if (actionInfo.action === "arkhamoverlay.tracksanity") {
        return true;
    }

    if (actionInfo.action === "arkhamoverlay.trackresources") {
        return true;
    }

    if (actionInfo.action === "arkhamoverlay.trackclues") {
        return true;
    }

    return false;
}


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
    } else if (isPlayerAction()) {
        //if we have a saved value, initialize the deck drop down
        var valueToSelect = actionInfo.payload.settings['deck'];
        if (!valueToSelect) {
            valueToSelect = 'player1';
        }

        let element = document.getElementById('selectPlayer');
        element.value = valueToSelect;

        selectDeckGroup.style.display = "none";
    } else {
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
            "action": actionInfo.action,
            "event": "sendToPlugin",
            "context": uuid, 
            "payload": {
                [param] : value
            }
        };
        websocket.send(JSON.stringify(json));
    }
}