"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var ContentType = /** @class */ (function () {
    function ContentType() {
    }
    ContentType.FORM = "application/x-www-form-urlencoded";
    ContentType.JSON = "application/json";
    return ContentType;
}());
exports.ContentType = ContentType;
var HttpRequest = /** @class */ (function () {
    function HttpRequest(url, data) {
        this.baseUrl = "";
        this.onfinish = null;
        this._url = url;
        this._data = data;
        this._req = new DuktapeJS.HttpRequest();
    }
    Object.defineProperty(HttpRequest.prototype, "isCancelled", {
        get: function () {
            return this._cancel;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(HttpRequest.prototype, "isDone", {
        get: function () {
            return this._done;
        },
        enumerable: true,
        configurable: true
    });
    /**
     * 将json对象转为 key=val 形式的 query 参数
     */
    HttpRequest.QUERY = function (payload) {
        if (payload) {
            var str = '';
            for (var key in payload) {
                if (str.length > 0) {
                    str += '&';
                }
                str += key + "=" + encodeURIComponent(payload[key]);
            }
            return str;
        }
        return null;
    };
    /**
     * 拼接 url 与 query 参数
     */
    HttpRequest.URL = function (url, payload) {
        var query = this.QUERY(payload);
        if (query && query.length > 0) {
            return url + "?" + query;
        }
        return url;
    };
    // 以 GET 方式发送请求 （参数直接在url中携带）
    HttpRequest.GET = function (url, query, finish) {
        var req = new HttpRequest(this.URL(url, query), undefined);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.get();
        return req;
    };
    // 以 POST 方式发送 JSON 对象
    HttpRequest.POST = function (url, payload, finish) {
        var data = payload && JSON.stringify(payload);
        var req = new HttpRequest(url, data);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.contentType(ContentType.JSON);
        req.post();
        return req;
    };
    // 以 POST 方式发送 表单 数据 (data 内容形式 "a=val1&b=val2&c=123")
    HttpRequest.FORM = function (url, data, finish) {
        var req = new HttpRequest(url, data);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.contentType(ContentType.FORM);
        req.post();
        return req;
    };
    /**
     * 取消请求
     */
    HttpRequest.prototype.cancel = function () {
        if (!this._done) {
            this._cancel = true;
            if (this.onfinish) {
                this.onfinish(false, "request cancelled");
            }
        }
        return this;
    };
    /**
     * 设定超时
     */
    HttpRequest.prototype.timeout = function (seconds) {
        var _this = this;
        if (!this._done) {
            setTimeout(function () {
                if (!_this._done) {
                    _this._cancel = true;
                    if (_this.onfinish) {
                        _this.onfinish(false, "request timeout");
                    }
                }
            }, seconds * 1000);
        }
        return this;
    };
    /**
     * 伪造 User-Agent
     */
    HttpRequest.prototype.userAgent = function (agent) {
        // this._headers.push("User-Agent", agent || "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
        this._req.SetRequestHeader("User-Agent", agent || "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
    };
    HttpRequest.prototype.contentType = function (type) {
        // this._headers.push("Content-Type", type);
        this._req.SetRequestHeader("Content-Type", type);
    };
    HttpRequest.prototype.onComplete = function (succ, res) {
        this._done = true;
        // console.log("httprequest", this._cancel, res);
        if (this.onfinish && !this._cancel) {
            this.onfinish(succ, res);
        }
    };
    HttpRequest.prototype.get = function () {
        // console.log("GET", this._url);
        this._req.SendGetRequest(this.baseUrl + this._url, this.onComplete.bind(this));
    };
    HttpRequest.prototype.post = function () {
        // console.log("POST", this._url, this._data);
        this._req.SendPostRequest(this.baseUrl + this._url, this._data, this.onComplete.bind(this));
    };
    HttpRequest.sharedBaseUrl = "";
    return HttpRequest;
}());
exports.HttpRequest = HttpRequest;
//# sourceMappingURL=http.js.map