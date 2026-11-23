// Genre limit (5), "Others" text boxes, and release-type "Others" box for the upload form.
document.addEventListener('DOMContentLoaded', function () {
    var MAX = 5;
    var boxes = Array.prototype.slice.call(document.querySelectorAll('.genre-check'));
    var count = document.getElementById('genre-count');
    var otherGenre = document.getElementById('other-genre-box');
    var releaseSelect = document.getElementById('ReleaseType');
    var releaseOther = document.getElementById('release-other-box');

    function syncGenres() {
        var n = boxes.filter(function (b) { return b.checked; }).length;
        boxes.forEach(function (b) { b.disabled = !b.checked && n >= MAX; });
        if (count) count.textContent = n + ' selected (up to ' + MAX + ')';
        if (otherGenre) {
            otherGenre.hidden = !boxes.some(function (b) { return b.checked && b.dataset.others === 'True'; });
        }
    }
    function syncRelease() {
        if (releaseSelect && releaseOther) {
            releaseOther.hidden = releaseSelect.value !== releaseSelect.dataset.other;
        }
    }

    boxes.forEach(function (b) { b.addEventListener('change', syncGenres); });
    if (releaseSelect) releaseSelect.addEventListener('change', syncRelease);
    syncGenres();
    syncRelease();
});
