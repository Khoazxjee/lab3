(function () {
    const element = document.getElementById("myCarousel");
    if (!element) return;
    const carousel = new bootstrap.Carousel(element, { interval: 6000, pause: false });
    const button = document.getElementById("pauseBanner");
    let paused = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    function refresh() {
        if (paused) carousel.pause(); else carousel.cycle();
        button.textContent = paused ? "Tiếp tục trình chiếu" : "Dừng trình chiếu";
        button.setAttribute("aria-pressed", String(paused));
    }
    button.addEventListener("click", function () { paused = !paused; refresh(); });
    refresh();
})();
