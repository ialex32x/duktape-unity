
export class ContentType {
    static readonly FORM = "application/x-www-form-urlencoded";
    static readonly JSON = "application/json";
}

export class HttpRequest {
    static sharedBaseUrl = "";

    public baseUrl = "";
    private _url: string;
    private _data: any;
    private _req: DuktapeJS.HttpRequest;
    private _cancel: boolean;
    private _done: boolean;

    onfinish: (status: boolean, res: any) => void = null;

    get isCancelled() {
        return this._cancel;
    }

    get isDone() {
        return this._done;
    }

    /**
     * 将json对象转为 key=val 形式的 query 参数
     */
    static QUERY(payload?: any) {
        if (payload) {
            let str = '';
            for (let key in payload) {
                if (str.length > 0) {
                    str += '&';
                }
                str += `${key}=${encodeURIComponent(payload[key])}`;
            }
            return str;
        }
        return null;
    }

    /**
     * 拼接 url 与 query 参数
     */
    static URL(url: string, payload?: any) {
        let query = this.QUERY(payload);
        if (query && query.length > 0) {
            return `${url}?${query}`;
        }
        return url;
    }

    // 以 GET 方式发送请求 （参数直接在url中携带）
    static GET(url: string, query?: any, finish?: (status: boolean, res: any) => void) {
        let req = new HttpRequest(this.URL(url, query), undefined);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.get();
        return req;
    }

    // 以 POST 方式发送 JSON 对象
    static POST(url: string, payload: any, finish?: (status: boolean, res: any) => void) {
        let data = payload && JSON.stringify(payload);
        let req = new HttpRequest(url, data);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.contentType(ContentType.JSON);
        req.post();
        return req;
    }

    // 以 POST 方式发送 表单 数据 (data 内容形式 "a=val1&b=val2&c=123")
    static FORM(url: string, data: string, finish?: (status: boolean, res: any) => void) {
        let req = new HttpRequest(url, data);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.contentType(ContentType.FORM);
        req.post();
        return req;
    }

    constructor(url: string, data?: any) {
        this._url = url;
        this._data = data;
        this._req = new DuktapeJS.HttpRequest();
    }

    /**
     * 取消请求
     */
    cancel() {
        if (!this._done) {
            this._cancel = true;
            if (this.onfinish) {
                this.onfinish(false, "request cancelled");
            }
        }
        return this;
    }

    /**
     * 设定超时
     */
    timeout(seconds: number) {
        if (!this._done) {
            setTimeout(() => {
                if (!this._done) {
                    this._cancel = true;
                    if (this.onfinish) {
                        this.onfinish(false, "request timeout");
                    }
                }
            }, seconds * 1000);
        }
        return this;
    }

    /**
     * 伪造 User-Agent
     */
    userAgent(agent?: string) {
        // this._headers.push("User-Agent", agent || "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
        this._req.SetRequestHeader("User-Agent", agent || "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
    }

    contentType(type: string) {
        // this._headers.push("Content-Type", type);
        this._req.SetRequestHeader("Content-Type", type);
    }

    private onComplete(succ: boolean, res: any) {
        this._done = true;
        // console.log("httprequest", this._cancel, res);
        if (this.onfinish && !this._cancel) {
            this.onfinish(succ, res);
        }
    }

    get() {
        // console.log("GET", this._url);
        this._req.SendGetRequest(this.baseUrl + this._url, this.onComplete.bind(this));
    }

    post() {
        // console.log("POST", this._url, this._data);
        this._req.SendPostRequest(this.baseUrl + this._url, this._data, this.onComplete.bind(this));
    }
}
