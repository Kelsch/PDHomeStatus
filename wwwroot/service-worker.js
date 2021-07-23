// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
//self.addEventListener('fetch', () => { });
/*self.addEventListener('fetch', event => event.respondWith(onFetch(event)));*/

self.addEventListener('fetch', function (event) {
    let cachedResponse = null;

    if (event.request.method === 'PUT') {
        if (event.request.headers.get('Background-Sync') === 'True') {
            sendToBackgroundSync(event.request);
            event.preventDefault();
        }
        else {
            return fetch(event.request);
        }
    }

    //if (event.request.method === 'GET') {
    //    return cachedResponse || fetch(event.request);
    //}
});

function sendToBackgroundSync(request) {
    const id = Date.now();
    syncStore[id] = request;

    self.registration.sync.register(id
        // Not working, but SyncManager not supported everywhere so not super worth it to change
        //, {
        ////maxDelay: 500000, // 5 minutes
        //}
    );
}

const syncStore = {}
self.addEventListener('sync', function (event) {
    var request = syncStore[event.tag];
    if (request) {
        event.waitUntil(fetch(request.clone()));
    }
});

//function sendToServer(url, authKey, job) {
//    const myHeaders = new Headers();

//    myHeaders.append('Content-Type', 'application/json');
//    myHeaders.append('Authorization', 'Bearer ' + authKey.replace(/['"]+/g, ''));

//    return fetch(url, {
//        method: 'PUT',
//        body: JSON.stringify(job),
//        headers: myHeaders,
//    })
//        .then((response) => {
//            if (!response.ok) {
//                console.log(response)
//            }

//            Promise.resolve();
//        })
//        .catch((err) => { console.error(err); Promise.reject(); });
//}

//self.addEventListener('message', function (event) {
//    if (event.data.type === 'syncMessage') {
//        const id = event.data.job.jobId;
//        syncStore[id] = event.data;

//        self.registration.sync.register(id);
//    }
//});

//async function onFetch(event) {
//    let cachedResponse = null;
//    if (event.request.method === 'PUT') {
//        const id = Date.now();
//        syncStore[id] = event.request;

//        self.registration.sync.register(id);
//    }

//    return cachedResponse || fetch(event.request);
//}