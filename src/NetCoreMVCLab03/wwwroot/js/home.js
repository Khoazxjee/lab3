$(function () {
    const button = $("#loadAjax");
    const region = $("#data-from-ajax");
    const status = $("#ajaxStatus");
    button.on("click", function () {
        button.prop("disabled", true);
        region.attr("aria-busy", "true");
        status.removeClass("text-danger").text("Đang tải sách...");
        region.load(button.data("url"), function (response, result) {
            button.prop("disabled", false);
            region.attr("aria-busy", "false");
            if (result === "error") status.addClass("text-danger").text("Không tải được danh sách. Vui lòng thử lại.");
            else status.text("Đã tải " + region.find(".book-card").length + " cuốn sách.");
        });
    });
});
