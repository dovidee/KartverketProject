document.getElementById('obstacleForm').addEventListener('submit', function (e) {
    // sjekk at markere er satt i gruppen
    if (points.length == 0) {
        e.preventDefault();
        return;
    }

    // sett dato
    const currentDate = new Date();
    document.getElementById('ObstacleSubmittedDate').value = currentDate.toISOString();

    // stringify layers
    const geoJsonString = JSON.stringify(layers);
    document.getElementById('ObstacleJSON').value = geoJsonString;
});