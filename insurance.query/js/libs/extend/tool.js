window.tools = window.tools || {};

tools = {
    get_today: function (format, date) {
        date = date ? date : new Date();
        var year = date.getFullYear();
        var month = (date.getMonth() + 1);
        var day = date.getDate();
        var time = undefined;
        var result = "";
        month = month < 10 ? "0" + month : month;
        day = day < 10 ? "0" + day : day;

        result = year + "-" + month + "-" + day;

        switch (format) {
            case 'HH:mm:ss':
                result += " " + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds();
                break;
        }

        return result;
    },
    get_one_day: function (days, date) {
        var d, m, r, t = "en";

        if (typeof date == "string") {
            if (date.indexOf("年") > 0 && date.indexOf("月") > 0 && date.indexOf("日") > 0) {
                t = "cn";
                date = date.replace("年", "-").replace("月", "-").replace("日", "");
            }
            d = new Date(date);
        } else {
            d = new Date();
        }

        d = d.valueOf();
        d = d + days * 24 * 60 * 60 * 1000;
        d = new Date(d);
        m = d.getMonth() + 1;
        m = m < 10 ? "0" + m : m;
        r = d.getDate();
        r = r < 10 ? "0" + r : r;

        if (t == "cn") {
            t = d.getFullYear() + "年" + m + "月" + r + "日";
        } else {
            t = d.getFullYear() + "-" + m + "-" + r;
        }

        return t;
    },
    get_last_day: function(month) {
        var d = new Date();
        var y = d.getFullYear();
        var m = month || d.getMonth() + 1;
        var out = undefined;

        if (m < 10) {
            m = "0" + m;
        }

        d = new Date(y, m, 0);
        out = y + "-" + m + "-" + d.getDate();

        return out;
    },
    calculator: {
        addition: function(arg1, arg2) {
            var r1,
                r2,
                m,
                n,
                result;
            arg1 = arg1 == undefined ? 0 : arg1;
            arg2 = arg2 == undefined ? 0 : arg2;
            try {
                r1 = arg1.toString().split(".")[1].length;
            } catch (e) {
                r1 = 0;
            }
            try {
                r2 = arg2.toString().split(".")[1].length;
            } catch (e) {
                r2 = 0;
            }
            m = Math.pow(10, Math.max(r1, r2));
            n = (r1 >= r2) ? r1 : r2;
            result = ((arg1 * m + arg2 * m) / m).toFixed(n);
            return Number(result) <= 0 ? undefined : result;
        }
    }
};