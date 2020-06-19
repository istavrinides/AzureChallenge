const connection = new signalR.HubConnectionBuilder()
    .withUrl("/challengeHub")
    .build();

$(document).ready(function () {

    var started = parseInt($("#Started").val());
    var finished = parseInt($("#Finished").val());

    configureChart(started, finished);

    if (rawData) {
        rawData.forEach(function (item) {
            var parsed = item.split(':');

            var itemDate = new Date(parseInt(parsed[2]), parseInt(parsed[3]) - 1, parseInt(parsed[4]));

            var fiveDaysAgo = new Date();
            fiveDaysAgo.setDate(fiveDaysAgo.getDate() - 6);

            if (itemDate >= fiveDaysAgo) {

                var index = parseInt(parsed[1]);

                if (questionData[index]) {
                    questionData[index].push(parsed[0]);
                }
                else {
                    questionData[index] = [parsed[0]];
                }
            }

        });

        chartSeries = Array.from(Array(questionData.length), (e, i) => questionData[i] ? questionData[i].length : 0);

        new Chartist.Bar('#questionCompletionChart', {
            labels: Array.from(Array(chartSeries.length), (e, i) => i + 1),
            series: [chartSeries]
        }, {
            seriesBarDistance: 10,
            horizontalBars: true,
            reverseData: true,
            axisY: {
                offset: 20,
                showLabel: true
            },
            axisX: {
                onlyInteger: true,
                showLabel: true
            },
            plugins: [
                Chartist.plugins.ctAxisTitle({
                    axisX: {
                        axisTitle: "Number of users completed",
                        axisClass: "ct-axis-title",
                        offset: {
                            x: 0,
                            y: 30
                        },
                        textAnchor: "middle"
                    },
                    axisY: {
                        axisTitle: "Question Index",
                        axisClass: "ct-axis-title",
                        offset: {
                            x: 0,
                            y: -1
                        },
                        flipTitle: false
                    }
                })
            ]
        });
    }

});

var configureChart = function (started, finished) {
    if (started + finished > 0) {

        var data = {
            series: [started, finished]
        };

        new Chartist.Pie('#numOfUsersChart', {
            labels: ["Started", "Finished"],
            series: data.series
        }, {
            donut: true,
            showLabel: true,
            plugins: [
                Chartist.plugins.legend()
            ],
            labelInterpolationFnc: function (value, index) {
                return data.series[index] === 0 ? "" : data.series[index];
            }
        });
    }
}

connection.on('QuestionComplete', function (message) {
    console.log(message);
    // Parse the message
    var parts = message.split(':');

    var today = new Date();

    if (parseInt(parts[2]) === today.getFullYear() && parseInt(parts[3]) === (today.getMonth() + 1) && parseInt(parts[4]) === today.getDate()) {

        // Get the users in the current question index
        var userId = parts[0];
        var questionIndex = parseInt(parts[1]);
        var users = questionData[questionIndex];

        var tempQuestionData = [];
        // First, we need to sanitize the question data by removing the user in question
        questionData.forEach(function (item, index) {
            if (item.includes(userId)) {
                item.forEach(function (uId) {
                    if (uId !== userId)
                        tempQuestionData[index] = uId;
                });
            }
            else {
                tempQuestionData[index] = item;
            }
        });

        questionData = tempQuestionData;

        // if there are some users
        if (questionData[questionIndex] && questionData[questionIndex].length > 0)
            questionData[questionIndex].push(userId);
        // If not
        else
            questionData[questionIndex] = [userId];


        chartSeries = Array.from(Array(questionData.length), (e, i) => questionData[i] ? questionData[i].length : 0);

        new Chartist.Bar('#questionCompletionChart', {
            labels: Array.from(Array(questionData.length), (e, i) => i + 1),
            series: [chartSeries]
        }, {
            seriesBarDistance: 10,
            horizontalBars: true,
            reverseData: true,
            axisY: {
                offset: 20,
                showLabel: true
            },
            axisX: {
                onlyInteger: true,
                showLabel: true
            },
            plugins: [
                Chartist.plugins.ctAxisTitle({
                    axisX: {
                        axisTitle: "Number of users completed",
                        axisClass: "ct-axis-title",
                        offset: {
                            x: 0,
                            y: 30
                        },
                        textAnchor: "middle"
                    },
                    axisY: {
                        axisTitle: "Question Index",
                        axisClass: "ct-axis-title",
                        offset: {
                            x: 0,
                            y: -1
                        },
                        flipTitle: false
                    }
                })
            ]
        });
    }
});

(async () => {
    try {
        await connection.start();
        await connection.invoke("AddToGroup", $("#ChallengeId").val());
    }
    catch (e) {
        console.error(e.toString());
    }
})();