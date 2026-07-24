// Ferme automatiquement les alertes après quelques secondes
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        document.querySelectorAll('.alert').forEach(function (alertEl) {
            var alert = bootstrap.Alert.getOrCreateInstance(alertEl);
            alert.close();
        });
    }, 6000);
});
