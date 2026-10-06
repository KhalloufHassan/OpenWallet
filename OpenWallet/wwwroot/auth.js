let _inactivityTimer = null;
let _inactivityDotnet = null;
let _inactivityMinutes = 5;

function resetInactivityTimer() {
    if (!_inactivityDotnet) return;
    clearTimeout(_inactivityTimer);
    _inactivityTimer = setTimeout(() => {
        _inactivityDotnet.invokeMethodAsync('OnInactivityTimeout');
    }, _inactivityMinutes * 60 * 1000);
}

const _activityEvents = ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart', 'click'];

window.owAuth = {
    isWebAuthnSupported: () =>
        typeof navigator.credentials !== 'undefined' &&
        typeof window.PublicKeyCredential !== 'undefined' &&
        typeof window.PublicKeyCredential.parseCreationOptionsFromJSON === 'function' &&
        typeof window.PublicKeyCredential.parseRequestOptionsFromJSON === 'function',

    hasPlatformKey: () => localStorage.getItem('ow_platform_key') === '1',
    setPlatformKey: (val) => {
        if (val) localStorage.setItem('ow_platform_key', '1');
        else localStorage.removeItem('ow_platform_key');
    },

    passkeyRegister: async (optionsJson) => {
        const options = PublicKeyCredential.parseCreationOptionsFromJSON(JSON.parse(optionsJson));
        const credential = await navigator.credentials.create({ publicKey: options });
        if (credential.authenticatorAttachment === 'platform') window.owAuth.setPlatformKey(true);
        return JSON.stringify(credential);
    },

    passkeyAuthenticate: async (optionsJson) => {
        const options = PublicKeyCredential.parseRequestOptionsFromJSON(JSON.parse(optionsJson));
        const credential = await navigator.credentials.get({ publicKey: options });
        return JSON.stringify(credential);
    },

    startInactivityTimer: (dotnetRef, minutes) => {
        _inactivityDotnet = dotnetRef;
        _inactivityMinutes = minutes;
        _activityEvents.forEach(e => document.addEventListener(e, resetInactivityTimer, { passive: true }));
        resetInactivityTimer();
    },

    stopInactivityTimer: () => {
        clearTimeout(_inactivityTimer);
        _activityEvents.forEach(e => document.removeEventListener(e, resetInactivityTimer));
        _inactivityDotnet = null;
    },

    renderQrCode: (elementId, text) => {
        const el = document.getElementById(elementId);
        if (!el || typeof QRCode === 'undefined') return;
        el.innerHTML = '';
        new QRCode(el, { text, width: 200, height: 200, colorDark: '#fff', colorLight: '#0d1117' });
    }
};
