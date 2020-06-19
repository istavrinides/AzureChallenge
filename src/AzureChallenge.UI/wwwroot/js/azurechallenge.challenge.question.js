const connection = new signalR.HubConnectionBuilder()
    .withUrl("/challengeHub")
    .build();

$(document).ready(function () {

    $("#btnCheck").click(function () {

        var questionId = $("#QuestionId").val();
        var challengeId = $("#ChallengeId").val();
        var nextQuestionId = $("#NextQuestionId").val();
        var difficulty = $("#Difficulty").val();
        var questionIndex = $("#QuestionIndex").val();

        // Clear the contents
        $("#checkModalContentDiv").empty();
        $("#checkModalWaiting").show();
        $("#checkModalContent").addClass('d-none');
        $("#justification").addClass('d-none');
        $("#checkModal").modal('show');

        $.get("/Challenge/ValidateQuestion?questionId=" + questionId + "&challengeId=" + challengeId + "&nextQuestionId=" + nextQuestionId + "&points=" + difficulty + "00&questionIndex=" + questionIndex)
            .done(function (data) {
                $("#checkModalWaiting").hide();

                $("#checkModalContent").removeClass('d-none');

                if (data.filter(e => e.Value === false).length > 0) {
                    data.forEach(function (item) {
                        if (!item.Value) {
                            if (item.Key.startsWith('Error:')) {
                                $("#checkModalContentDiv").append("<div class='col-md-12 text-danger text-center h2 pt-5 pb-5'>Could not validate the question. Please check your deployment</div>");
                            }
                            else {
                                $("#checkModalContentDiv").append("<div class='col-md-12 text-danger text-center h2 pt-5 pb-5'>" + item.Key + "</div>");
                            }
                        }
                    });
                }
                else {
                    $("#checkModalContentDiv").append("<div class='col-md-12 text-success text-center h2 pt-5 pb-5'>You successfully completed the question!</div>");
                    $("#justification").removeClass('d-none');
                    $("#btnNext").removeAttr('disabled');
                    $("#btnNextModal").removeClass('d-none');
                    connection.invoke("SendQuestionCompletionToGroup", $("#userId").val(), challengeId, questionIndex).catch(err => console.error(err));
                }
            })
            .fail(function () {
                window.alert("Could not validate the question, an internal error occured. Please try again later.");
                $("#checkModal").modal('hide');
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
            });
    });

});



(async () => {
    try {
        await connection.start();
    }
    catch (e) {
        console.error(e.toString());
    }
})();