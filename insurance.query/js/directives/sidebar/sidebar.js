angular.module('app')
    .directive('sidebar', [
        function () {
            return {
                templateUrl: 'js/directives/sidebar/sidebar.html',
                restrict: 'E',
                replace: true,
                controller: [
                    '$rootScope', '$scope', '$http', function ($r_scope, $scope, $http) {
                        $scope.selectedMenu = 'dashboard';
                        $scope.collapseVar = 0;
                        $scope.multiCollapseVar = 0;

                        $scope.check = function (x) {

                            if (x == $scope.collapseVar)
                                $scope.collapseVar = 0;
                            else
                                $scope.collapseVar = x;
                        };


                        $scope.multiCheck = function (y) {
                            if (y == $scope.multiCollapseVar)
                                $scope.multiCollapseVar = 0;
                            else
                                $scope.multiCollapseVar = y;
                        };

                        $r_scope.$watch('user', function (to, from) {
                            if (!to) {
                                $r_scope.user = {
                                    name: $.cookie('username'),
                                    authority_level: $.cookie('authority_level')
                                }
                            }
                        });
                    }
                ]
            }
        }
    ]);