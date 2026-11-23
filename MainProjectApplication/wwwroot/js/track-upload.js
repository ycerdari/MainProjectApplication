// Reads the MP3's length in the browser and stores it in the hidden DurationSeconds field.
document.addEventListener('DOMContentLoaded', function () {
    var fileInput = document.getElementById('AudioFile');
    var durationInput = document.getElementById('DurationSeconds');
    if (!fileInput || !durationInput) return;

    fileInput.addEventListener('change', function () {
        var file = fileInput.files[0];
        if (!file) { return; }

        var url = URL.createObjectURL(file);
        var audio = new Audio();
        audio.preload = 'metadata';
        audio.addEventListener('loadedmetadata', function () {
            durationInput.value = Math.round(audio.duration);
            URL.revokeObjectURL(url);
        });
        audio.addEventListener('error', function () {
            durationInput.value = 0;
            URL.revokeObjectURL(url);
        });
        audio.src = url;
    });
});
