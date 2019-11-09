'use strict'

var state = {
    count: 0,
    connect: false,
    stringUrlServer: 'http://localhost:9696/server/',
    URLServer: new URL('http://localhost:9696/server/')
}


function log(m) {
    let code = `console.log("${m}")`;
    chrome.tabs.executeScript({
        code: code
    });
}

function postToServer(urlServer, data) {
    try {
        var xhttp = new XMLHttpRequest() || ActiveXObject();
        xhttp.onreadystatechange = function () {
            if (this.readyState == 4) {
                //In ra data nhan duoc
                log(`onreadystatechange: [${this.status}] [${xhttp.responseURL}] >> ${this.responseText}`)
            }
        }
        xhttp.open('POST', urlServer, true);
        xhttp.setRequestHeader('Content-Type', 'application/json');
        xhttp.send(data);
    } catch (e) {
        log(`error on postToServer ${e}`);
    }
}

//frameId: 0
//initiator: "https://www.digikey.com"
//method: "GET"
//parentFrameId: -1
//requestId: "15846"
//tabId: 193
//timeStamp: 1563899300663.702
//type: "image"
//url: "https://www.digikey.com/-/media/Images/Homepage/homepage-associations.png?la=en-US&ts=23fbbbc9-e9ca-
function logURL(requestDetails) {
    if (state.connect) {
        chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
            if (requestDetails.tabId == tabs[0].id && new URL(requestDetails.url).origin != state.URLServer.origin) {
                state.count++;
                log(" [" + state.count + "]: " + requestDetails.url);
                postToServer(state.stringUrlServer, JSON.stringify(requestDetails));
            }
        });
    }
}


function _reload() {
    chrome.tabs.executeScript({
        code: `window.location.reload();`
    });
}



chrome.webRequest.onBeforeRequest.addListener(logURL, { urls: ["<all_urls>"] });
chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
    if (changeInfo.status == 'loading') {
        state.count = 0;
    }
})

function showStateConnect() {
    document.getElementById('state_connect').innerText = state.connect ? ("CONNECT " + state.stringUrlServer) : "DISCONNECT";
}

function switchConnect() {
    state.connect = state.connect != true;
    state.stringUrlServer = document.getElementById('stringUrlServer').value;
    state.URLServer = new URL(state.stringUrlServer)
    showStateConnect();
}

document.getElementById('btnConnect').onclick = function () {
    switchConnect();
}

showStateConnect();

document.getElementById('btnReload').onclick = function () {
    _reload();
}

document.getElementById('btnConnectAndReload').onclick = function () {
    switchConnect();
    if (state.connect) {
        _reload();
    }
}