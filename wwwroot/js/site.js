document.addEventListener("DOMContentLoaded", () => {

    const content = document.getElementById("content");
    const pagination = document.getElementById("pagination");

    const seedInput = document.getElementById("seed");
    const likesInput = document.getElementById("likes");
    const language = document.getElementById("language");

    const tableBtn = document.getElementById("tableBtn");
    const galleryBtn = document.getElementById("galleryBtn");

    const player = document.getElementById("player");

    let currentProgress = null;
    let currentSongId = null;
    let currentBtn = null;

    let page = 1;
    let mode = "table";
    let loading = false;

    let isSeeking = false;


    document.addEventListener("click", (e) => {

        const row = e.target.closest(".song-row");
        if (row) {
            const id = row.dataset.id;
            const el = document.getElementById(`d-${id}`);
            if (el) {
                el.style.display = el.style.display === "none" ? "table-row" : "none";
            }
        }

        const btn = e.target.closest(".play-btn, .play-inline");
        if (btn) {
            const url = btn.dataset.url;
            if (url) playSong(url, btn);
        }
    });

    async function load(reset = false) {
        if (loading) return;
        loading = true;

        if (reset) {
            if (page === 1) content.innerHTML = "";
            pagination.innerHTML = "";
            window.scrollTo(0, 0);
        }

        const res = await fetch(`/api/music?seed=${seedInput.value}&likesAvg=${likesInput.value}&region=${language.value}&page=${page}`);
        const data = await res.json();

        if (mode === "table") {
            renderTable(data);
            renderPagination();
        } else {
            renderGallery(data);
        }

        loading = false;
    }

    function renderTable(data) {
        let html = `
        <table class="table table-hover align-middle">
            <thead class="table-light">
                <tr>
                    <th>#</th>
                    <th>Song</th>
                    <th>Artist</th>
                    <th>Album</th>
                    <th>Genre</th>
                </tr>
            </thead>
            <tbody>
        `;

        data.forEach(s => {
            html += `
            <tr class="song-row" data-id="${s.id}" style="cursor:pointer">
                <td>${s.id}</td>
                <td><strong>${s.title}</strong></td>
                <td>${s.artist}</td>
                <td>${s.album}</td>
                <td><span class="badge bg-secondary">${s.genre}</span></td>
            </tr>

            <tr id="d-${s.id}" style="display:none">
                <td colspan="5">
                    ${renderDetails(s)}
                </td>
            </tr>
            `;
        });

        html += "</tbody></table>";
        content.innerHTML = html;
    }

    function renderDetails(s) {
        return `
        <div class="card shadow-sm p-3">
            <div class="row g-3">

                <div class="col-md-3">
                    <div class="cover-lg">
    <img src="${s.coverUrl}">

    <div class="cover-overlay">
        <div class="cover-title">${s.title}</div>
        <div class="cover-artist">${s.artist}</div>
    </div>
</div>
                </div>

                <div class="col-md-9">

                    <div class="d-flex gap-2 mb-2">

                        <button class="btn btn-success btn-sm rounded-circle play-inline"
                                data-url="${s.audioUrl}">
                            <i class="bi bi-play-fill"></i>
                        </button>

                        <div class="player-bar mt-2">

                            <span id="current-${s.id}">0:00</span>

                            <input type="range"
                                   min="0"
                                   max="100"
                                   value="0"
                                   id="progress-${s.id}"
                                   onmousedown="startSeek()"
                                   onmouseup="endSeek(this.value)"
                            >

                            <span id="duration-${s.id}">0:00</span>

                        </div>

                        <span class="badge bg-primary">👍 ${s.likes}</span>

                    </div>

                    <div class="lyrics">
                        ${s.lyrics.map(l => `<p>${l}</p>`).join("")}
                    </div>

                </div>
            </div>
        </div>`;
    }

    window.playSong = function (url, btn) {

        const card = btn.closest(".music-card, .card");
        if (!card) return;

        const range = card.querySelector("input[type=range]");
        const id = range.id.split("-")[1];

        const currentTimeEl = card.querySelector(`#current-${id}`);
        const durationEl = card.querySelector(`#duration-${id}`);

        currentProgress = range;

        document.querySelectorAll(".music-card").forEach(c => c.classList.remove("active"));
        card.classList.add("active");

        player.src = url;

        if (player.paused) {
            player.play();

            btn.innerHTML = `<i class="bi bi-pause-fill"></i>`;
            currentBtn = btn;
        } else {
            player.pause();
            btn.innerHTML = `<i class="bi bi-play-fill"></i>`;
        }

        player.onloadedmetadata = () => {
            durationEl.innerText = formatTime(player.duration);
        };

        player.ontimeupdate = () => {
            if (!currentProgress || isSeeking || !player.duration) return;

            const percent = (player.currentTime / player.duration) * 100;

            currentProgress.value = percent;
            currentProgress.style.background =
                `linear-gradient(to right, #22c55e ${percent}%, #ddd ${percent}%)`;

            currentTimeEl.innerText = formatTime(player.currentTime);
        };
    };

    window.startSeek = () => isSeeking = true;

    window.endSeek = (value) => {
        if (!player.duration) return;

        const time = (value / 100) * player.duration;
        player.currentTime = time;

        isSeeking = false;
    };

    function formatTime(sec) {
        if (!sec) return "0:00";
        const m = Math.floor(sec / 60);
        const s = Math.floor(sec % 60).toString().padStart(2, '0');
        return `${m}:${s}`;
    }

    function renderPagination() {
        pagination.innerHTML = `
        <button class="btn btn-outline-primary btn-sm"
                onclick="changePage(-1)"
                ${page === 1 ? "disabled" : ""}>
            Prev
        </button>

        <span class="mx-2 fw-bold">Page ${page}</span>

        <button class="btn btn-outline-primary btn-sm"
                onclick="changePage(1)">
            Next
        </button>
    `;
    }

    window.changePage = function (dir) {

        const newPage = page + dir;

        if (newPage < 1) return;

        page = newPage;
        load(true);
    };

    function renderGallery(data) {

        content.className = "row g-4";
        if (page === 1) content.innerHTML = "";
        pagination.innerHTML = "";

        data.forEach(s => {
            const card = document.createElement("div");
            card.className = "col-md-3";

            card.innerHTML = `
            <div class="music-card">

                <div class="cover">
                    <img src="${s.coverUrl}" />

                    <div class="overlay">

                        <div class="likes">👍 ${s.likes}</div>

                        <button class="play-btn" data-url="${s.audioUrl}">
                            <i class="bi bi-play-fill"></i>
                        </button>

                        <div class="bottom">
                            <div class="title">${s.title}</div>
                            <div class="artist">${s.artist}</div>
                        </div>

                    </div>
                </div>

                <div class="player-bar">
                    <span id="current-${s.id}">0:00</span>
                    <input type="range"
                           min="0"
                           max="100"
                           value="0"
                           id="progress-${s.id}"
                           onmousedown="startSeek()"
                           onmouseup="endSeek(this.value)">
                    <span id="duration-${s.id}">0:00</span>
                </div>

            </div>`;
            content.appendChild(card);
        });
    }

    tableBtn.onclick = () => { mode = "table"; load(true); };
    galleryBtn.onclick = () => { mode = "gallery"; load(true); };

    document.querySelectorAll("input, select").forEach(el =>
        el.addEventListener("change", () => load(true))
    );

    document.getElementById("randomSeed").onclick = () => {
        seedInput.value = Math.floor(Math.random() * 1000000);
        load(true);
    };

    window.addEventListener("scroll", () => {
        if (mode !== "gallery") return;
        if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 200) {
            page++;
            load();
        }
    });

    load(true);
});