/**
 * Created by zizo on 2016-4-5.
 */
Array.prototype.exist = function (e) {   //e可为string、number类型或等于null、undefined
    var exist = false;
    if (typeof e == "string") {
        var s = String.fromCharCode(2);
        exist = new RegExp(s + e + s).test(s + this.join(s) + s);
    } else {
        for (var i = 0; i < this.length; i++) {
            if (this[i] == e) {
                exist = true;
                break;
            }
        }
    }

    return exist;
};

Array.prototype.last = function () {
    if (this.length > 0) {
        return this[this.length - 1];
    } else {
        return undefined;
    }
};

Array.prototype.remove_by = function (key, value, inverse) {
    var arr = [];
    if (inverse) {
        angular.forEach(this, function (obj) {
            if (obj[key] == value) {
                arr.push(obj);
            }
        });
    } else {
        angular.forEach(this, function (obj) {
            if (obj[key] != value) {
                arr.push(obj);
            }
        });
    }

    return arr;
};

Array.prototype._find = function (key, value, keyForReturn, remove) {
    var result = undefined;
    var arr = [];
    var realValue = undefined;
    var type = "string";
    var found = false;
    var _this = this;

    if (key.indexOf(".") > 0) {
        arr = key.split(".");
    }

    if (Number(value) >= 0) {
        type = "number";
        value = Number(value);
    }

    angular.forEach(this, function(obj, i) {

        if (arr.length > 0) {
            realValue = obj[arr[0]][arr[1]];
        } else {
            realValue = obj[key];
        }

        if (type == "number") {
            found = Number(realValue) == value;
        } else {
            if (value.indexOf("%") >= 0) {
                found = realValue.indexOf(value.replaceAll("%", "")) >= 0 ? true : false;
            } else {
                found = realValue == value;
            }
        }

        if (found) {
            if (keyForReturn == 'index') {
                result = i;
            } else if (keyForReturn) {
                result = obj[keyForReturn];
            } else {
                result = obj;
                if (remove) {
                    _this.splice(i, 1);
                }
            }

            return false;
        }
    });

    return result;
};

String.prototype.contains = function (str) {
    if (typeof (str) == "string" && this.indexOf(str) >= 0) {
        return true;
    } else {
        return false;
    }
};

String.prototype.replace_all = function (s1, s2) {
    return this.replace(new RegExp(s1, "gm"), s2); //g全局     
};