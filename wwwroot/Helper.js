function GetLocation() {
    var myPromise = new Promise((resolve, reject) => {

        navigator.geolocation.getCurrentPosition(
            position => {
                let longLat = `${position.coords.latitude.toString()}, ${position.coords.longitude.toString()}`;

                resolve(longLat);
            },
            error => { reject(error) }
        )
    }).catch(error => error);

    return myPromise;
}

function isDevice() {
    return /android|webos|iphone|ipad|ipod|blackberry|iemobile|opera mini|mobile/i.test(navigator.userAgent);
}