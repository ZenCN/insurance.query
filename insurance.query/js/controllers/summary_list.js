angular.module('app')
    .controller('summary_list_ctrl', [
        '$scope', '$http', function ($scope, $http) {
            $scope.date = {
                year: new Date().getFullYear(),
                month: {
                    data: ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'],
                    selected: (new Date().getMonth() + 1) + '月'
                }
            };

            $scope.search = function(month) {
                tools.get_last_day(month.replace('月',''))
            };
        }
    ]);