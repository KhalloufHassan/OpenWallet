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
        window.PublicKeyCredential !== undefined &&
        typeof window.PublicKeyCredential === 'function',

    isPasskeySupported: async () => {
        if (!window.owAuth.isWebAuthnSupported()) return false;
        try {
            return await PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
        } catch { return false; }
    },

    hasPlatformKey: () => localStorage.getItem('ow_platform_key') === '1',
    setPlatformKey: (val) => {
        if (val) localStorage.setItem('ow_platform_key', '1');
        else localStorage.removeItem('ow_platform_key');
    },

    passkeyRegister: async (optionsJson) => {
        const options = JSON.parse(optionsJson);
        options.challenge = base64UrlDecode(options.challenge);
        options.user.id = base64UrlDecode(options.user.id);
        if (options.excludeCredentials) {
            options.excludeCredentials = options.excludeCredentials.map(c => ({
                ...c,
                id: base64UrlDecode(c.id)
            }));
        }

        const credential = await navigator.credentials.create({ publicKey: options });
        return JSON.stringify(encodeRegistrationCredential(credential));
    },

    passkeyAuthenticate: async (optionsJson) => {
        const options = JSON.parse(optionsJson);
        options.challenge = base64UrlDecode(options.challenge);
        if (options.allowCredentials) {
            options.allowCredentials = options.allowCredentials.map(c => ({
                ...c,
                id: base64UrlDecode(c.id)
            }));
        }

        const assertion = await navigator.credentials.get({ publicKey: options });
        return JSON.stringify(encodeAssertionCredential(assertion));
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

function base64UrlDecode(base64url) {
    const padding = '='.repeat((4 - base64url.length % 4) % 4);
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/') + padding;
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
    return bytes.buffer;
}

function base64UrlEncode(buffer) {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i]);
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

function encodeRegistrationCredential(credential) {
    return {
        id: credential.id,
        rawId: base64UrlEncode(credential.rawId),
        type: credential.type,
        response: {
            attestationObject: base64UrlEncode(credential.response.attestationObject),
            clientDataJSON: base64UrlEncode(credential.response.clientDataJSON),
            transports: credential.response.getTransports ? credential.response.getTransports() : []
        },
        extensions: credential.getClientExtensionResults()
    };
}

function encodeAssertionCredential(assertion) {
    return {
        id: assertion.id,
        rawId: base64UrlEncode(assertion.rawId),
        type: assertion.type,
        response: {
            authenticatorData: base64UrlEncode(assertion.response.authenticatorData),
            clientDataJson: base64UrlEncode(assertion.response.clientDataJSON),
            signature: base64UrlEncode(assertion.response.signature),
            userHandle: assertion.response.userHandle ? base64UrlEncode(assertion.response.userHandle) : null
        },
        extensions: assertion.getClientExtensionResults()
    };
}
