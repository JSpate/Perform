var socket;
var retryInterval = 1000;

var state = {
    show: null,
    song: null,
    active: false,
    availableShows: [],
    bpm: 120,
    stomps: {
        0: null,
        1: null,
        2: null,
        3: null
    },
}

function connectWebSocket() {
    socket = new WebSocket(`ws://${window.location.host}/ws`);

    socket.onopen = function (event) {
        console.log('WebSocket is open now.');
        hideErrorOverlay();
    };

    socket.onmessage = function (event) {
        const data = JSON.parse(event.data);

        if (data.type === "beat") {
            handleBeat(data.data);
            return;
        }
        state = data;
        stateChange();
        console.log('WebSocket message received:', event.data);
    };

    socket.onclose = function (event) {
        console.log('WebSocket is closed now. Reconnecting...');
        showErrorOverlay('Connection lost. Reconnecting...');
        setTimeout(connectWebSocket, retryInterval);
    };

    socket.onerror = function (error) {
        console.error('WebSocket error observed:', error);
        showErrorOverlay('Connection error. Reconnecting...');
        socket.close();
    };
}

function showErrorOverlay(message) {
    const errorOverlay = document.querySelector('#errorOverlay');
    errorOverlay.textContent = message;
    errorOverlay.style.display = 'flex';
    disableForm();
}

function hideErrorOverlay() {
    const errorOverlay = document.querySelector('#errorOverlay');
    errorOverlay.style.display = 'none';
    enableForm();
}

function disableForm() {
    document.querySelectorAll('button, select').forEach(element => {
        element.disabled = true;
    });
}

function enableForm() {
    document.querySelectorAll('button, select').forEach(element => {
        element.disabled = false;
    });
}

connectWebSocket();

function stateChange() {
    if (state.show) {
        updateShow();
        document.querySelector('#select').style.display = 'none';
        document.querySelector('#show').style.display = 'flex';
    } else {
        updateSelect();
        document.querySelector('#select').style.display = 'flex';
        document.querySelector('#show').style.display = 'none';
    }
    updateStompButtons();
}

function updateShow() {
    document.querySelector('#songName').textContent = (state.song + 1).toString() + ': ' + state.songs[state.song].name;
    
    if (state.active === false) {
        document.querySelector('#songDescription').style.display = 'block';
        document.querySelector('#songDescription').textContent = state.songs[state.song].description;
        document.querySelector('#songButtons').style.display = 'grid';
        document.querySelector('#activeButtons').style.display = 'none';
        document.querySelector('#songPrevious').disabled = state.song === 0;
        document.querySelector('#songNext').disabled = state.song === state.songs.length - 1;
        document.querySelector('#lyricsContainer').style.display = 'none';
    } else {
        document.querySelector('#songDescription').style.display = 'none';
        document.querySelector('#lyricsContainer').style.display = 'block';
        document.querySelector('#lyricsContainer').innerHTML = '';
        document.querySelector('#songButtons').style.display = 'none';
        document.querySelector('#activeButtons').style.display = 'grid';
    }
}

function updateSelect() {
    const selectElement = document.querySelector('#selectShow');
    selectElement.innerHTML = '<option value="" disabled selected>Please select a show</option>';
    state.availableShows.forEach(show => {
        const option = document.createElement('option');
        option.value = show;
        option.textContent = show;
        selectElement.appendChild(option);
    });
}

function updateStompButtons() {
    for (let i = 0; i < 4; i++) {
        const button = document.querySelector(`#stomp${i}`);
        const light = button.querySelector('.light');
        const stompState = state.stomps[i];

        if (stompState === null) {
            button.style.display = 'none';
        } else {
            button.style.display = 'block';
            light.style.backgroundColor = stompState ? '#0f0' : '#555';
        }
    }
}

function selectShow() {
    const selectElement = document.querySelector('#selectShow');
    const selectedOption = selectElement.querySelector('option[value=""]');
    if (selectedOption) {
        selectElement.removeChild(selectedOption);
    }
    socket.send(JSON.stringify({ action: "selectShow", value: selectElement.value }));
}

function nextSong() {
    socket.send(JSON.stringify({ action: "nextSong", value: null }));
}

function previousSong() {
    socket.send(JSON.stringify({ action: "previousSong", value: null }));
}

function startSong() {
    socket.send(JSON.stringify({ action: "startSong", value: null }));
}

function endSong() {
    socket.send(JSON.stringify({ action: "endSong", value: null }));
}

function endShow() {
    socket.send(JSON.stringify({ action: "selectShow", value: null }));
}

function handleBeat(beatData) {
    const light = document.querySelector('#light');
    if (beatData.isDownbeat) {
        light.style.backgroundColor = '#0f0';
        // Update lyrics display
        updateLyricsDisplay(beatData.bar);
    } else {
        light.style.backgroundColor = '#f00';
    }

    light.style.opacity = 1;
    setTimeout(() => {
        light.style.opacity = 0.1;
    }, beatData.interval / 2);
}

function updateLyricsDisplay(currentBar) {
    const lyricsContainer = document.querySelector('#lyricsContainer');
    if (!lyricsContainer || currentBar < 0) return;

    // Get the lyrics for the current song
    const lyrics = state.songs[state.song].lyrics || [];

    // Find the current lyric based on the current bar
    const currentIndex = lyrics.findIndex((lyric, index) => {
        const nextLyric = lyrics[index + 1];
        return currentBar >= lyric.bar && (!nextLyric || currentBar < nextLyric.bar);
    });

    // Get 5 lines: 2 before, the current, and 2 after
    const startIndex = Math.max(0, currentIndex - 2);
    const endIndex = Math.min(lyrics.length, currentIndex + 3);
    const visibleLyrics = lyrics.slice(startIndex, endIndex);

    // Clear the container
    lyricsContainer.innerHTML = '';

    // Render the lyrics
    visibleLyrics.forEach((lyric, index) => {
        const lineElement = document.createElement('div');
        lineElement.textContent = lyric.text;

        // Highlight the current lyric
        if (index === currentIndex - startIndex) {
            lineElement.style.fontSize = '1.5em';
            lineElement.style.fontWeight = 'bold';
        } else {
            lineElement.style.fontSize = '1.25em';
            lineElement.style.opacity = '0.7';
        }

        lyricsContainer.appendChild(lineElement);
    });
}