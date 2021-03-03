(function () {

    var getElementByTagName = function (name) {
        return document.getElementsByTagName(name)[0];
    }

    var srvrApp = getElementByTagName("srvr-app");
    var wasmApp = getElementByTagName("wasm-app");

    var addServerEvent = function (type, listener, options) {
        srvrApp.addEventListener(type, listener, options);
    }

    var addWasmEvent = function (type, listener, options) {
        wasmApp.addEventListener(type, listener, options);
    }

    var loadScript = function (name, callback) {
        var script = document.createElement('script');
        script.onload = callback;
        script.src = '_framework/blazor.' + name + '.js';
        if (name === "webassembly") {
            script.defer = true;
            script.async = true;
        }
        document.head.appendChild(script);
    }

    var blazorInfo = function (message) {
        console.info('[' + new Date().toISOString() + '] Information: ' + message);
    }

    var loadWasmFunction = function () {

        if (getElementByTagName('srvr-app').innerHTML.indexOf('<!--Blazor:{') !== -1) {
            setTimeout(loadWasmFunction, 100);
            return;
        }

        window.addEventListener = addWasmEvent;
        document.addEventListener = addWasmEvent;

        loadScript('webassembly');
    };

    var init = function () {
        var wasmReadyToSwitch = false;

        window.addEventListener = addServerEvent;
        document.addEventListener = addServerEvent;

        loadScript('server', function () {
            window.BlazorServer = window.Blazor;
        });

        setTimeout(loadWasmFunction, 100);

        window.wasmReady = function () {
            blazorInfo('Wasm ready');
            wasmReadyToSwitch = true;

            if (window.hybridType === 'HybridOnReady') {
                window.switchToWasm(window.location.href);
            }
        }

        window.switchToWasm = function (location, manual) {
            if (window.hybridType === 'HybridManual' && !manual) {
                return true;
            }

            if (manual) {
                location = window.location.href;
            }

            if (!wasmReadyToSwitch) return false;
            wasmReadyToSwitch = false;

            blazorInfo('Switch to wasm');

            setTimeout(function () {
                window.BlazorServer.defaultReconnectionHandler.onConnectionDown = () => { };
                window.BlazorServer._internal.forceCloseConnection();

                wasmApp.style.display = "block";
                srvrApp.style.display = "none";

                window.Blazor.navigateTo(location, false, false)
            }, 0);

            return true;
        }
    }

    init();

})();