angular.module('app')
    .controller('summary_list_ctrl', [
        '$scope', '$http', function ($scope, $http) {
            $scope.page.load_data = function () {
                var month = $scope.date.month.selected.replace('月', '');
                var url = 'index/get_summary_list?page_index=' + $scope.page.index + '&page_size=' + $scope.page.size +
                    '&start_time=' + $scope.date.year + '-' + month + '-1&end_time=' + tools.get_last_day(month);
                
                $http.get(url).then(function (response) {
                    if (angular.isObject(response.data)) {
                        $scope.search.result = response.data.source;

                        if (response.data.page_count > 0) {
                            $scope.page.all_items = [];
                            for (var i = 0; i < response.data.page_count; i++) {
                                $scope.page.all_items.push(i + 1);
                            }

                            $scope.page.record_count = response.data.record_count;
                            if (!$scope.page.inited) {
                                if ($scope.page.all_items.length >= $scope.page.per_num) {
                                    $scope.page.filtered = $scope.page.all_items.slice(0, $scope.page.per_num);
                                } else {
                                    $scope.page.filtered = $scope.page.all_items;
                                }
                                $scope.page.inited = true;
                            }
                        }
                    } else {
                        msg(response.data);
                        throw response.data;
                    }
                });
            };

            $scope.date = {
                year: new Date().getFullYear(),
                month: {
                    data: ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'],
                    selected: (new Date().getMonth() + 1) + '月'
                }
            };

            $scope.search = {
                result: {},
                from_server: function() {
                    $scope.page.inited = false;
                    $scope.page.index = 0;
                    $scope.page.load_data();
                }
            };

            $scope.search.from_server();
        }
    ]);