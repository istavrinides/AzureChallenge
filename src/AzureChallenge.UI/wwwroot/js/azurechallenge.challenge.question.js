const connection = new signalR.HubConnectionBuilder()
    .withUrl("/challengeHub")
    .build();

$(document).ready(function () {

    $("#btnCheck").click(function () {

        var questionId = $("#QuestionId").val();
        var challengeId = $("#ChallengeId").val();
        var nextQuestionId = $("#NextQuestionId").val();
        var difficulty = parseInt($("#Difficulty").val()) * 100;
        var questionIndex = $("#QuestionIndex").val();
        var questionType = $("#QuestionType").val();

        var selectedRBChoice = "";
        var choices = [];

        if (questionType === "MultiChoice") {
            var questionSubType = $("#MCType").val();

            // Check if it's a radio button or check box multiplce choice question
            if (questionSubType === "CB") {

                if ($(".mccb:checked").length === 0) {
                    alert("Please select at least one option");
                    return;
                }

                $(".mccb:checked").each(function () {
                    choices.push($(this).data('key'));
                })
            }
            else {

                if ($(".mcrb:checked").length === 0) {
                    alert("Please select at least one option");
                    return;
                }

                selectedRBChoice = $(".mcrb:checked").first().data('key');
            }
        }

        // Clear the contents
        $("#checkModalContentDiv").empty();
        $("#checkModalWaiting").show();
        $("#checkModalContent").addClass('d-none');
        $("#justification").addClass('d-none');
        if (questionType === "API")
            $("#checkModal").modal('show');

        $.post("/Challenge/ValidateQuestion",
            {
                inputModel:
                {
                    QuestionId: questionId,
                    challengeId: challengeId,
                    NextQuestionId: nextQuestionId,
                    Difficulty: difficulty,
                    QuestionIndex: questionIndex,
                    SelectedRBChoice: selectedRBChoice,
                    Choices: choices
                }
            }
        ).done(function (data) {
            $("#checkModalWaiting").hide();

            $("#checkModalContent").removeClass('d-none');
            $(".mc-message-div").addClass('d-none');
            $(".mc-justification").addClass('d-none');
            $(".mc-more-to-check").addClass('d-none');
            $(".mc-correct").addClass('d-none');

            if (questionType === "API") {
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
            }
            else if (questionType === "MultiChoice") {
                $("#checkModal").modal('hide');

                data.filter(e => e.Key.includes("#*#*#")).forEach(function (item) {
                    var splitted = item.Key.split("#*#*#");

                    var lbl = $(".mc-message[data-key='" + splitted[0] + "'");
                    lbl.text(splitted[1]);
                    lbl.addClass(item.Value === false ? "text-danger" : "text-success");
                    $(".mc-message-div[data-key='" + splitted[0] + "'").removeClass('d-none');
                });

                if (data.filter(e => e.Value === false).length === 0) {
                    $(".mc-justification").removeClass('d-none');
                    $("#btnNext").removeAttr('disabled');
                    $(".mc-correct").removeClass('d-none');
                    connection.invoke("SendQuestionCompletionToGroup", $("#userId").val(), challengeId, questionIndex).catch(err => console.error(err));
                }
                else if (data.filter(e => e.Value === false && e.Key != "WrongChoiceCombo").length === 0) {
                    $(".mc-more-to-check").removeClass('d-none');
                }
            }
        }).fail(function () {
            window.alert("Could not validate the question, an internal error occured. Please try again later.");
            $("#checkModal").modal('hide');
            $('body').removeClass('modal-open');
            $('.modal-backdrop').remove();
        });
    });

    if ($("#TimeLeftInSeconds").val() > 0) {
        var countDown = function () {


            var timeleft = $("#TimeLeftInSeconds").val() - 1;
            $("#TimeLeftInSeconds").val(timeleft);

            var hours = Math.floor(((timeleft * 1000) % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            var minutes = Math.floor(((timeleft * 1000) % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor(((timeleft * 1000) % (1000 * 60)) / 1000);

            var timeString = ('0' + hours).slice(-2) + ':' + ('0' + minutes).slice(-2) + ':' + ('0' + seconds).slice(-2);

            $("#timeLeft").text(timeString);

            // If we have 30 minutes remaining
            if (timeleft < 1800)
                $("#timeLeft").addClass('text-warning');
            // If we have 10 minutes remaining
            if (timeleft < 600)
                $("#timeLeft").addClass('text-danger');

        };

        countDown();
        setInterval(countDown, 1000);
    }

});



(async () => {
    try {
        await connection.start();
    }
    catch (e) {
        console.error(e.toString());
    }
})();