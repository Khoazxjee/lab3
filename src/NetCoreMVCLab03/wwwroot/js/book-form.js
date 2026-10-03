(function ($) {
    $.extend($.validator.messages, { required: "Vui lòng nhập thông tin bắt buộc.", number: "Vui lòng nhập một số hợp lệ." });
    $.validator.addMethod("bookimage", function (value, element) {
        const file = element.files[0];
        if (!file) return element.dataset.existing === "true";
        return file.size > 0 && file.size <= 5 * 1024 * 1024 && /\.(png|jpe?g|webp)$/i.test(file.name);
    });
    $.validator.unobtrusive.adapters.addBool("bookimage");
    const input = document.getElementById("Image");
    const preview = document.getElementById("imagePreview");
    const original = preview.getAttribute("src");
    let objectUrl;
    input.addEventListener("change", function () {
        if (objectUrl) URL.revokeObjectURL(objectUrl);
        const file = input.files[0];
        if (file && file.size > 0 && file.size <= 5 * 1024 * 1024 && /\.(png|jpe?g|webp)$/i.test(file.name)) {
            objectUrl = URL.createObjectURL(file);
            preview.src = objectUrl;
            preview.hidden = false;
        } else {
            preview.hidden = !original;
            if (original) preview.src = original; else preview.removeAttribute("src");
        }
        $(input).valid();
    });
})(window.jQuery);
