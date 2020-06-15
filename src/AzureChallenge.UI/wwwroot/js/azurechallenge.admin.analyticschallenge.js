$(document).ready(function () {

    var started = parseInt($("#Started").val());
    var finished = parseInt($("#Finished").val());

    if (started + finished > 0) {

        var data = {
            series: [started, finished]
        };

        new Chartist.Pie('.ct-chart', {
            labels: ["Started", "Finished"],
            series: [started, finished]
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

});