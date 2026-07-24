// Intégration du formateur IA (avatar).
//
// Fonctionnement :
// 1. Au clic sur "Démarrer", on appelle notre backend (AvatarController) qui contacte
//    le fournisseur configuré (HeyGen LiveAvatar ou Anam.ai) sans jamais exposer la clé
//    API au navigateur.
// 2. Selon le fournisseur renvoyé :
//    - HeyGen (LiveAvatar) : le backend renvoie directement une URL d'iframe prête à
//      l'emploi (POST /v2/embeddings) → on l'affiche dans <iframe id="avatar-frame">.
//      Aucun SDK JS n'est nécessaire côté client avec cette nouvelle API.
//    - Anam.ai : le backend renvoie un jeton de session de courte durée → on charge le
//      SDK officiel depuis esm.sh et on diffuse le flux dans <video id="avatar-video">.
//
// Documentation officielle à consulter en cas d'évolution des API :
//  - HeyGen (LiveAvatar) : https://docs.liveavatar.com/
//  - Anam.ai              : https://docs.anam.ai/

const config = window.formationAppAvatarConfig;
const startBtn = document.getElementById('avatar-start');
const stopBtn = document.getElementById('avatar-stop');
const statusEl = document.getElementById('avatar-status');
const videoEl = document.getElementById('avatar-video');
const frameEl = document.getElementById('avatar-frame');

let anamClient = null;
let currentProvider = null;

function setStatus(message) {
    if (statusEl) statusEl.textContent = message;
}

async function demarrerAvatar() {
    startBtn.disabled = true;
    setStatus('Connexion au formateur IA...');

    try {
        const response = await fetch(config.sessionUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin'
        });

        if (!response.ok) {
            throw new Error('Le serveur a refusé la demande de session (' + response.status + ').');
        }

        const session = await response.json();

        if (!session.configured) {
            setStatus(session.errorMessage || "Le formateur IA n'est pas configuré pour le moment.");
            startBtn.disabled = false;
            return;
        }

        currentProvider = session.provider;

        if (currentProvider === 'HeyGen') {
            demarrerHeyGen(session);
        } else if (currentProvider === 'Anam') {
            await demarrerAnam(session);
        } else {
            setStatus('Fournisseur d\'avatar IA non pris en charge : ' + currentProvider);
            startBtn.disabled = false;
            return;
        }

        startBtn.classList.add('d-none');
        stopBtn.classList.remove('d-none');
        setStatus('Formateur IA connecté — vous pouvez lui parler.');
    } catch (err) {
        console.error(err);
        setStatus("Impossible de démarrer le formateur IA. Réessayez dans quelques instants.");
        startBtn.disabled = false;
    }
}

function demarrerHeyGen(session) {
    if (!session.embedUrl) {
        throw new Error('Aucune URL d\'intégration reçue du serveur.');
    }
    videoEl.classList.add('d-none');
    frameEl.classList.remove('d-none');
    frameEl.src = session.embedUrl;
}

async function demarrerAnam(session) {
    frameEl.classList.add('d-none');
    videoEl.classList.remove('d-none');

    const { createClient } = await import('https://esm.sh/@anam-ai/js-sdk@latest');
    anamClient = createClient(session.token);
    await anamClient.streamToVideoElement('avatar-video');
}

function reinitialiserBoutons() {
    startBtn.disabled = false;
    startBtn.classList.remove('d-none');
    stopBtn.classList.add('d-none');
    setStatus('Formateur IA non démarré.');
}

async function arreterAvatar() {
    try {
        if (currentProvider === 'HeyGen') {
            frameEl.src = 'about:blank';
            frameEl.classList.add('d-none');
        } else if (currentProvider === 'Anam' && anamClient) {
            anamClient.stopStreaming?.();
            anamClient = null;
            videoEl.srcObject = null;
            videoEl.classList.add('d-none');
        }
    } catch (e) {
        console.warn('Erreur lors de l\'arrêt de la session avatar :', e);
    } finally {
        reinitialiserBoutons();
    }
}

startBtn?.addEventListener('click', demarrerAvatar);
stopBtn?.addEventListener('click', arreterAvatar);
