angular.module('app')
    .controller('illness_payment_ctrl', [
        '$scope', '$http', function ($scope, $http) {
            $scope.search = {
                condition: {
                    name: undefined,
                    id_card: undefined
                },
                result: [],
                from_server: function () {
                    $http.get('index/illness_payment_search?name=' + (this.condition.name || '') +
                            '&id_card=' + (this.condition.id_card || ''))
                        .then(function(response) {
                            if ($.isArray(response.data)) {
                                $scope.search.result = response.data;
                            } else {
                                msg(response.data);
                                throw response.data;
                            }
                        });
                }
            }
        }
    ]);