$(document).ready(function () {
    $('[data-toggle="tooltip"]').tooltip();

    $("#questionsTable .deleteQuestion").on('click', function () {
        var questionId = $(this).attr('data-questionId');

        if (window.confirm("Are you sure you want to delete the question?")) {
            $.get("/Administration/Question/RemoveQuestion?id=" + questionId)
                .done(function (data) {

                    $("#questionsTable tr[data-questionId='" + questionId + "'").remove();

                })
                .fail(function () {
                    window.alert("Could not delete the question, an internal error occured. Please try again later.");
                });
        }
    });

    $("#inputFilter").on('input', function () {
        filterTable($("#showAll")[0].checked);
    });

    $("#showAll").change(function () {
        filterTable(this.checked);
    });

});

filterTable = function (showAll = false) {

    $("#questionsTable tbody tr").each(function () {
        if (!$(this).find("td.name").text().toLowerCase().includes($("#inputFilter").val().toLowerCase())
            && !$(this).find("td.description").text().toLowerCase().includes($("#inputFilter").val().toLowerCase())) {

            // Check if we only have 1 td. If so, filter on that also
            if ($(this).find("td").length === 1 && $(this).find("td").text().toLowerCase().includes($("#inputFilter").val().toLowerCase())) {

                // Check if I only should see my questions
                $(this).show();
            }
            else {
                $(this).hide();
            }
        }
        else {
            if (!showAll) {
                if ($(this).attr("data-mine") && $(this).attr("data-mine") === "True") {
                    $(this).show();
                }
                else {
                    $(this).hide();
                }
            }
            else {
                $(this).show()
            }
        }
    });
}