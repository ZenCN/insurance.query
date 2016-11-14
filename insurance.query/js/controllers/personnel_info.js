angular.module('app')
    .controller('personnel_info_ctrl', [
        '$scope', '$http', function ($scope, $http) {
            $scope.page.load_data = function() {
                var url = 'index/search_personnel_info?page_index=' + $scope.page.index + '&page_size=' + $scope.page.size;

                if ($scope.search.condition.name) {
                    url += '&name=' + $scope.search.condition.name;
                }

                if ($scope.search.condition.id_card) {
                    url += '&id_card=' + $scope.search.condition.id_card;
                }

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

            $scope.search = {
                condition: {
                    name: undefined,
                    id_card: undefined
                },
                result: [],
                from_server: function () {
                    $scope.page.inited = false;
                    $scope.page.index = 0;
                    $scope.page.load_data();
                }
            }

            $scope.query_person_type = function (val) {
                switch (parseInt(val)) {
                    case 1:
                        return '一般人员';
                    case 2:
                        return '低保家庭';
                    case 3:
                        return '重度残疾';
                    case 4:
                        return '三无人员';
                    case 5:
                        return '困难人员';
                    default:
                        return "";
                }
            };
        }
    ]);