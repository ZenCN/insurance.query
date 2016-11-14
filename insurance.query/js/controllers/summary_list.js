angular.module('app')
    .controller('summary_list_ctrl', [
        '$scope', '$http', function ($scope, $http) {
            $scope.page.load_data = function () {
                var month = $scope.date.month.selected.replace('月', '');
                var url = 'index/get_summary_list?page_index=' + $scope.page.index + '&page_size=' + $scope.page.size +
                    '&start_time=' + $scope.date.year + '-' + month + '-1&end_time=' + tools.get_last_day(month) +
                    '&hospital_id=' + $scope.search.condition.hospital_id + '&state=' + $scope.search.condition.state
                    + '&area_code=' + $scope.search.condition.area_code + '&source_type=' + $scope.search.condition.source_type;


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

            $scope.export = function () {
                if ($scope.search.result.list.length > 0) {
                    var month = $scope.date.month.selected.replace('月', '');
                    window.open('index/export_to_excel?hospital_id=' + $scope.search.condition.hospital_id +
                        '&hospital_name=' + $scope.search.condition.hospital_name +
                        '&area_code=' + $scope.search.condition.area_code + '&state=' + $scope.search.condition.state +
                        '&start_time=' + $scope.date.year + '-' + month + '-1&end_time=' + tools.get_last_day(month) +
                        '&source_type=' + $scope.search.condition.source_type);
                } else {
                    msg('没有清单数据');
                }
            };
            $scope.search = {
                result: {
                    summary: undefined,
                    list: []
                },
                condition: {
                    state: '',
                    hospital_id: undefined,
                    hospital_name: undefined,
                    area_code: '',
                    source_type: '1'  //数据来源 1：医院报销 ACK197=1 要明细及汇总 2：中心报销 ACK197=0 只要明细
                },
                from_server: function() {
                    if (typeof this.condition.hospital_id != "string" || this.condition.hospital_id.trim().length == 0) {
                        msg('请输入要查询的医院');
                    } else {
                        $scope.page.inited = false;
                        $scope.page.index = 0;
                        $scope.page.load_data();
                    }
                }
            };

            $scope.get_hospital_names = function(val) {
                return $http.get('index/search_hospital?name=' + val + '&area_code=' + $scope.search.condition.area_code)
                    .then(function(response) {
                        return response.data;
                    });
            };
            $scope.hospital_selecting = function ($item) {
                $scope.search.condition.hospital_id = $item.id;
            };

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