/*
class ContentType {
    static readonly FORM = "application/x-www-form-urlencoded"
    static readonly JSON = "application/json"
}

export class HttpRequest {
    static sharedBaseUrl = ""
    public baseUrl = ""
    private url: string
    private data: any
    private req: DuktapeJS.HttpRequest
    private type: string
    private _headers: Array<any> = []
    private _cancel: boolean
    private _done: boolean

    onfinish: (status: boolean, res: any) => void

    isCancelled() {
        return this._cancel
    }

    isDone() {
        return this._done
    }

    // 将json对象转为 key=val 形式的 query 参数
    static QUERY(payload?: any) {
        if (!!payload) {
            let str = ''
            for (let key in payload) {
                if (str.length > 0) {
                    str += '&'
                }
                str += `${key}=${encodeURIComponent(payload[key])}`
            }
            return str
        }
    }

    // 拼接 url 与 query 参数
    static URL(url: string, payload?: any) {
        let query = this.QUERY(payload)
        if (!!query) {
            return `${url}?${query}`
        }
        return url
    }

    // 以 GET 方式发送请求 （参数直接在url中携带）
    static GET(url: string, query?: any, finish?: (status: boolean, res: any) => void, type = "json") {
        let req = new HttpRequest(this.URL(url, query), undefined, type)
        req.baseUrl = this.sharedBaseUrl
        req.onfinish = finish
        req.get()
        return req
    }

    // 以 POST 方式发送 JSON 对象
    static POST(url: string, payload: any, finish: (status: boolean, res: any) => void, type = "json") {
        let data = payload && JSON.stringify(payload)
        let req = new HttpRequest(url, data, type)
        req.baseUrl = this.sharedBaseUrl
        req.onfinish = finish
        req.contentType(ContentType.JSON)
        req.post()
        return req
    }

    // 以 POST 方式发送 表单 数据 (data 内容形式 "a=val1&b=val2&c=123")
    static FORM(url: string, data: string, finish: (status: boolean, res: any) => void, type = "json") {
        let req = new HttpRequest(url, data, type)
        req.baseUrl = this.sharedBaseUrl
        req.onfinish = finish
        req.contentType(ContentType.FORM)
        req.post()
        return req
    }

    constructor(url: string, data?: any, type = "json") {
        this.url = url
        this.data = data
        this.type = type
        this.req = new DuktapeJS.HttpRequest()
        this.req.once(DuktapeJS.COMPLETE, this, this.onComplete)
        this.req.once(DuktapeJS.ERROR, this, this.onError)
    }

    // 取消请求
    cancel() {
        if (!this._done) {
            this._cancel = true
            if (!!this.onfinish) {
                this.onfinish(false, "request cancelled")
            }
        }
        return this
    }

    // 设定超时
    timeout(seconds: number) {
        if (!this._done) {
            setTimeout(() => {
                if (!this._done) {
                    this._cancel = true
                    if (!!this.onfinish) {
                        this.onfinish(false, "request timeout")
                    }
                }
            }, seconds * 1000)
        }
        return this
    }

    // 伪造 User-Agent
    userAgent() {
        this._headers.push("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36")
    }

    contentType(type: string) {
        this._headers.push("Content-Type", type)
    }

    private onComplete(res: any) {
        this._done = true
        console.log("httprequest", this._cancel, res)
        if (!!this.onfinish && !this._cancel) {
            this.onfinish(true, res)
        }
    }

    private onError(res: any) {
        this._done = true
        console.log("httprequest", res)
        if (!!this.onfinish && !this._cancel) {
            this.onfinish(false, res)
        }
    }

    get() {
        console.log("GET", this.type, this.url)
        this.req.send(this.baseUrl + this.url, this.data, "get", this.type, this._headers)
    }

    post() {
        console.log("POST", this.type, this.url, this.data)
        this.req.send(this.baseUrl + this.url, this.data, "post", this.type, this._headers)
    }
}
*/ 
//# sourceMappingURL=http.js.map