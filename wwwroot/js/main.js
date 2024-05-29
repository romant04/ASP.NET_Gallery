const modal = document.getElementById("modal")

modal.addEventListener("click", e => {
    if (e.target.id == "modal") {
        var currentUrl = window.location.href;
        var newUrl = currentUrl.split("?")[0];
        window.location.href = newUrl;
    }
})