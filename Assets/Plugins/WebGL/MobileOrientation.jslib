mergeInto(LibraryManager.library, {
    $OrientationGateSubscriptions: { nextId: 1, entries: {} },

    OrientationGate_IsMobile: function () {
        var agent = navigator.userAgent || "";
        var mobileHint = navigator.userAgentData && navigator.userAgentData.mobile;
        var desktopIPad = /Mac/.test(navigator.platform || "") && navigator.maxTouchPoints > 1;
        return mobileHint || /Android|iPhone|iPad|iPod|Mobile/i.test(agent) || desktopIPad ? 1 : 0;
    },

    OrientationGate_IsPortrait: function () {
        // Device orientation remains usable when itch.io embeds a fixed-size landscape canvas.
        var orientation = window.screen && window.screen.orientation;
        if (orientation && orientation.type) {
            return orientation.type.indexOf("portrait") === 0 ? 1 : 0;
        }
        if (typeof window.orientation === "number") {
            return Math.abs(window.orientation) % 180 === 0 ? 1 : 0;
        }
        return window.matchMedia("(orientation: portrait)").matches ? 1 : 0;
    },

    OrientationGate_Subscribe__deps: ["$OrientationGateSubscriptions"],
    OrientationGate_Subscribe: function (receiverPointer) {
        var receiver = UTF8ToString(receiverPointer);
        var callback = function () {
            SendMessage(receiver, "OnBrowserOrientationChanged", "");
        };
        var orientation = window.screen && window.screen.orientation;
        window.addEventListener("resize", callback);
        window.addEventListener("orientationchange", callback);
        if (orientation && orientation.addEventListener) {
            orientation.addEventListener("change", callback);
        }
        var id = OrientationGateSubscriptions.nextId++;
        OrientationGateSubscriptions.entries[id] = { callback: callback, orientation: orientation };
        return id;
    },

    OrientationGate_Unsubscribe__deps: ["$OrientationGateSubscriptions"],
    OrientationGate_Unsubscribe: function (id) {
        var entry = OrientationGateSubscriptions.entries[id];
        if (!entry) {
            return;
        }
        window.removeEventListener("resize", entry.callback);
        window.removeEventListener("orientationchange", entry.callback);
        if (entry.orientation && entry.orientation.removeEventListener) {
            entry.orientation.removeEventListener("change", entry.callback);
        }
        delete OrientationGateSubscriptions.entries[id];
    }
});
