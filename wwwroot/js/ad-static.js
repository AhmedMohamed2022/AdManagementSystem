//(function () {
//    const params = new URLSearchParams(window.location.search);
//    const key = params.get('key');
//    const container = document.getElementById('ad-container');

//    if (!key || !container) return;
//    console.log(params)
//    console.log(key)
//    console.log(container)
//    // Fetch ad JSON
//    fetch(`https://localhost:7223/ad/get?key=${key}`)
//        .then(r => r.json())
//        .then(ad => {
//            if (!ad || !ad.imageUrl) {
//                container.innerHTML = "<p>No ad available right now.</p>";
//                return;
//            }

//            // Create ad HTML
//            const img = document.createElement("img");
//            img.src = ad.imageUrl;
//            img.style.maxWidth = "100%";
//            img.style.cursor = "pointer";

//            img.onclick = () => {
//                window.open(`/ad/click?adId=${ad.id}&key=${key}`, "_blank");
//            };

//            container.innerHTML = "";
//            container.appendChild(img);
//        })
//        .catch(() => {
//            container.innerHTML = "<p>Error loading ad.</p>";
//        });
//})();

//(function () {
//    // ✅ Find the current <script> tag that loaded this file
//    const currentScript = document.currentScript || document.querySelector('script[src*="ad.js"]');

//    // ✅ Extract key from script src (e.g. ad.js?key=abc123)
//    const scriptUrl = new URL(currentScript.src);
//    const key = scriptUrl.searchParams.get("key");

//    const container = document.getElementById('ad-container');

//    if (!key || !container) {
//        console.error("Missing key or container");
//        return;
//    }

//    console.log("✅ Extracted key:", key);

//    // Fetch ad JSON
//    fetch(`https://localhost:7223/ad/get?key=${key}`)
//        .then(r => {
//            if (!r.ok) throw new Error("Network response was not ok");
//            return r.json();
//        })
//        .then(ad => {
//            if (!ad || !ad.imageUrl) {
//                container.innerHTML = "<p>No ad available right now.</p>";
//                return;
//            }

//            const img = document.createElement("img");
//            img.src = ad.imageUrl;
//            img.style.maxWidth = "100%";
//            img.style.cursor = "pointer";

//            img.onclick = () => {
//                window.open(`https://localhost:7223/ad/click?adId=${ad.id}&key=${key}`, "_blank");
//            };

//            container.innerHTML = "";
//            container.appendChild(img);
//        })
//        .catch(err => {
//            console.error("Error loading ad:", err);
//            container.innerHTML = "<p>Error loading ad.</p>";
//        });
//})();
(function () {
    // Find the current <script> tag that loaded this file
    const currentScript = document.currentScript || document.querySelector('script[src*="ad.js"]');
    if (!currentScript) {
        console.error("ad.js: cannot find script tag");
        return;
    }

    // Extract key from script src (e.g. ad.js?key=abc123)
    let scriptUrl;
    try {
        scriptUrl = new URL(currentScript.src);
    } catch (e) {
        console.error("ad.js: invalid script src", e);
        return;
    }

    const key = scriptUrl.searchParams.get("key");
    const container = document.getElementById('ad-container');

    if (!key || !container) {
        console.error("ad.js: Missing key or container", { key, container });
        return;
    }

    console.log("ad.js: Extracted key:", key);

    const API_BASE = 'https://localhost:7223';

    // Fetch ad JSON
    fetch(`${API_BASE}/ad/get?key=${encodeURIComponent(key)}`)
        .then(r => {
            if (!r.ok) {
                throw new Error(`ad/get returned ${r.status}`);
            }
            return r.json();
        })
        .then(ad => {
            if (!ad || !ad.imageUrl) {
                container.innerHTML = "<p>No ad available right now.</p>";
                return;
            }

            const img = document.createElement("img");
            img.src = ad.imageUrl;
            img.alt = ad.title || "Ad";
            img.style.maxWidth = "100%";
            img.style.cursor = "pointer";

            // record impression when image successfully loads
            img.onload = function () {
                // fire-and-forget; don't block rendering
                fetch(`${API_BASE}/ad/impression?adId=${ad.id}&key=${encodeURIComponent(key)}`)
                    .catch(err => console.warn("ad.js: impression record failed", err));
            };

            // fallback: if image errors, show message
            img.onerror = function () {
                container.innerHTML = "<p>Ad image failed to load.</p>";
            };

            img.onclick = function () {
                // open the advertiser target via the ad click endpoint (absolute)
                window.open(`${API_BASE}/ad/click?adId=${ad.id}&key=${encodeURIComponent(key)}`, "_blank");
            };

            container.innerHTML = "";
            container.appendChild(img);
        })
        .catch(err => {
            console.error("ad.js: Error loading ad:", err);
            container.innerHTML = "<p>Error loading ad.</p>";
        });
})();
