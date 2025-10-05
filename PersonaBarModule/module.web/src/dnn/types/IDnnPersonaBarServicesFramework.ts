/** Allow easy access to DNN services framework for API calls. */
export type IPersonaBarServicesFramework = {
    /** The anti-forgery token to use for secure API calls. */
    antiForgeryToken: string;

    /**
    * Performs an API call to a PersonaBar controller method.
    * Based on DNN Platform sf.js implementation.
    */
    call(
        /** The HTTP method to use. */
        httpMethod: "GET" | "POST",
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data to append to the GET query or to send in a POST. */
        params?: Record<string, unknown> | string | FormData | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void,
        /** If true the request will be sent synchronously (defaults to asynchronous). */
        sync?: boolean,
        /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
        silence?: boolean,
        /** Set to true if the data (params) represents a file to be uploaded. */
        postFile?: boolean
    ): JQuery.jqXHR<unknown>;

    call<T>(
        /** The HTTP method to use. */
        httpMethod: "GET" | "POST",
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data to append to the GET query or to send in a POST. */
        params?: Record<string, unknown> | string | FormData | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: T
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void,
        /** If true the request will be sent synchronously (defaults to asynchronous). */
        sync?: boolean,
        /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
        silence?: boolean,
        /** Set to true if the data (params) represents a file to be uploaded. */
        postFile?: boolean
    ): JQuery.jqXHR<T>;

    /** The root path for the registered API routes of the Persona Bar. */
    moduleRoot: string;

    /** The API controller to use. */
    controller: string;

    /** Sets the information about the tab and the anti-forgery token. */
    setHeaders(xhr: JQueryXHR): void;

    /** Gets the root url for the service call. */
    getServiceRoot(): string;

    /** Gets the root path of the website. */
    getSiteRoot(): string;

    /**
    * Allows making a raw call to any URL.
    * Based on DNN Platform sf.js rawCall implementation.
    */
    rawCall(
        /** The HTTP method to use. */
        httpMethod: "GET" | "POST",
        /** The URL to call. */
        url: string,
        /** The data to append to the GET query or to send in a POST. */
        params?: Record<string, unknown> | string | FormData | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void,
        /** If true the request will be sent synchronously (defaults to asynchronous). */
        sync?: boolean,
        /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
        silence?: boolean,
        /** Set to true if the data (params) represents a file to be uploaded. */
        postFile?: boolean
    ): JQuery.jqXHR<unknown>;

    /** Allows making a raw call (to any url) */
    rawCall<T>(
        /** The HTTP method to use. */
        httpMethod: "GET" | "POST",
        /** The URL to call. */
        url: string,
        /** The data to append to the GET query or to send in a POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: T
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void,
        /** If true the request will be sent synchronously (defaults to asynchronous). */
        sync?: boolean,
        /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
        silence?: boolean,
        /** Set to true if the data (params) represents a file to be uploaded. */
        postFile?: boolean
    ): JQuery.jqXHR<T>;

    /** Performs a POST to a known API method. */
    post(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data to append to the GET query or to send in a POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void,
        /** If true the request will be sent synchronously (defaults to asynchronous). */
        sync?: boolean,
        /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
        silence?: boolean,
        /** Set to true if the data (params) represents a file to be uploaded. */
        postFile?: boolean
    ): JQuery.jqXHR<unknown>;

    /** Performs a POST to a known API method. */
    post<T>(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data to append to the GET query or to send in a POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: T
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void,
        /** If true the request will be sent synchronously (defaults to asynchronous). */
        sync?: boolean,
        /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
        silence?: boolean,
        /** Set to true if the data (params) represents a file to be uploaded. */
        postFile?: boolean
    ): JQuery.jqXHR<T>;

    /** Performs a POST to a known API method to upload a file. */
    postfile(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data of the fiel to POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void,
        /** If true the request will be sent synchronously (defaults to asynchronous). */
        sync?: boolean,
        /** If true, will prevent showing the Persona Bar loading bar while we wait on the response. */
        silence?: boolean,
        /** Set to true if the data (params) represents a file to be uploaded. */
        postFile?: boolean
    ): JQuery.jqXHR<unknown>;

    /** Performs a POST API call without displaying the loading bar. */
    postsilence(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data of the fiel to POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void
    ): JQuery.jqXHR<unknown>;

    /** Performs a POST API call without displaying the loading bar. */
    postsilence<T>(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data of the fiel to POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: T
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void
    ): JQuery.jqXHR<T>;

    /** Performs a GET API call. */
    get(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data of the fiel to POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void
    ): JQuery.jqXHR<unknown>;

    /** Performs a GET API call. */
    get<T>(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data of the fiel to POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: T
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void
    ): JQuery.jqXHR<T>;

    /** Performs a GET API call without displaying the loading bar. */
    getsilence(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data of the fiel to POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void
    ): JQuery.jqXHR<unknown>;

    /** Performs a GET API call without displaying the loading bar. */
    getsilence<T>(
        /** The controller action to call (method of the controller class). */
        method: string,
        /** The data of the fiel to POST. */
        params?: JQuery.PlainObject | string | object | unknown[],
        /** Fires when the call succeeded. */
        success?: (
            /** The data returned by the API call. */
            data: unknown
        ) => void,
        /** Fires when the API call fails. */
        failure?: (
            /** The XHR object that contains the error details. */
            xhr: JQueryXHR | null,
            /** The error message. */
            message: string
        ) => void,
        /** Fires with true when the API call starts and false when complete (no matter if it succeeded or failed). */
        loading?: (loading: boolean) => void,
        /** A callback that can be used to customize the request before it is sent.*/
        beforeSend?: (xhr: JQueryXHR) => void
    ): JQuery.jqXHR<T>;
};
