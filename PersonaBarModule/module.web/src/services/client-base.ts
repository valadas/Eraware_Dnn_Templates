import { IPersonaBarServicesFramework } from '../dnn/types/IDnnPersonaBarServicesFramework';

export class ClientBase {
  private sf: IPersonaBarServicesFramework;
  private _httpHandler: { fetch: (url: RequestInfo, init?: RequestInit) => Promise<Response> };
  private defaults: { silence: boolean; sync: boolean };
  private currentRequestOptions: PersonaBarRequestOptions = {};

  constructor(configuration: ConfigureRequest) {
    this.sf = configuration.sf;
    this.defaults = {
      silence: configuration.defaults?.silence ?? false,
      sync: configuration.defaults?.sync ?? false
    };

    // Create our custom HTTP handler
    this._httpHandler = {
      fetch: (url: RequestInfo, init?: RequestInit) => this.fetch(url, init || {})
    };

    // Override the http property using a getter that always returns our handler
    Object.defineProperty(this, 'http', {
      get: () => this._httpHandler,
      set: () => {
        // Ignore any attempts to set the http property
        console.log('Attempt to override http property ignored - using PersonaBar handler');
      },
      enumerable: true,
      configurable: false
    });
  }

  /**
   * Configure PersonaBar options for the next request only
   */
  withOptions(options: PersonaBarRequestOptions): this {
    this.currentRequestOptions = { ...options };
    return this;
  }

  protected getBaseUrl(_defaultUrl: string, baseUrl?: string): string {
    this.sf.moduleRoot = "TestCompany_MyModuleTest";
    baseUrl = this.sf
      .getServiceRoot()
      .replace(/\/$/, "");

    return baseUrl || "";
  }

  protected transformOptions(options: RequestInit): Promise<RequestInit> {
    // For PersonaBar, we don't need to transform headers since the sf.call method handles authentication
    // The PersonaBar services framework manages headers internally via setHeaders method
    return Promise.resolve(options);
  }

  /**
   * Converts fetch-style requests to PersonaBar services framework calls
   * This bridges the gap between the generated NSwag client and PersonaBar SF
   */
  protected fetch(url: RequestInfo, options: RequestInit): Promise<Response> {
    return new Promise<Response>((resolve, reject) => {
      const method = options.method?.toUpperCase() || 'GET';

      // Properly handle RequestInfo which can be string, URL, or Request object
      let urlString: string;
      if (typeof url === 'string') {
        urlString = url;
      } else if (url instanceof URL) {
        urlString = url.href;
      } else {
        // Request object
        urlString = url.url;
      }

      // Extract the controller action from the URL
      const baseUrl = this.getBaseUrl('');
      const relativePath = urlString.replace(baseUrl, '').replace(/^\//, '');
      const pathParts = relativePath.split('/');

      if (pathParts.length < 2) {
        reject(new Error('Invalid API path format. Expected: /Controller/Action'));
        return;
      }

      const controller = pathParts[0];
      const action = pathParts[1];

      // Set the controller for this request
      this.sf.controller = controller;

      let requestData: unknown = null;
      if (options.body != null) {
        try {
          requestData = JSON.parse(options.body as string);
        } catch {
          requestData = options.body;
        }
      }

      // Use current request options if set, otherwise use defaults
      const silence = this.currentRequestOptions.silence ?? this.defaults.silence;
      const sync = this.currentRequestOptions.sync ?? this.defaults.sync;
      const loadingCallback = this.currentRequestOptions.loading;
      const beforeSendCallback = this.currentRequestOptions.beforeSend;

      // Clear the current request options after using them (one-time use)
      this.currentRequestOptions = {};

      const successCallback = (data: unknown) => {
        // Create a Response-like object that matches fetch API
        const response = {
          ok: true,
          status: 200,
          statusText: 'OK',
          headers: new Headers(),
          text: () => Promise.resolve(JSON.stringify(data)),
          json: () => Promise.resolve(data),
        } as Response;

        resolve(response);
      };

      const failureCallback = (xhr: { status: number, statusText: string } | null, message: string) => {
        const status = xhr?.status || 500;
        const statusText = xhr?.statusText || 'Internal Server Error';

        // Create a Response-like object for errors
        const response = {
          ok: false,
          status,
          statusText,
          headers: new Headers(),
          text: () => Promise.resolve(message),
          json: () => Promise.resolve({ error: message }),
        } as Response;

        resolve(response); // Resolve with error response instead of rejecting
      };

      // Use the personabar call method with configurable options
      this.sf.call(
        method as "GET" | "POST",
        action,
        requestData as Parameters<typeof this.sf.call>[2],
        successCallback,
        failureCallback,
        loadingCallback, // loading callback - configurable via withOptions()
        beforeSendCallback, // beforeSend callback - configurable via withOptions()
        sync,      // sync - configurable via withOptions() or defaults
        silence    // silence - configurable via withOptions() or defaults
      );
    });
  }
}

/** Options for configuring PersonaBar behavior on individual requests */
interface PersonaBarRequestOptions {
  /** If true, request won't show the loading bar */
  silence?: boolean;
  /** If true, request will be synchronous */
  sync?: boolean;
  /** Custom loading callback - called when request starts/ends */
  loading?: (isLoading: boolean) => void;
  /** Custom beforeSend callback - called before request is sent */
  beforeSend?: (xhr: JQueryXHR) => void;
}

interface ConfigureRequest {
  sf: IPersonaBarServicesFramework;
  /** Default PersonaBar options for all requests */
  defaults?: {
    /** If true, requests won't show the loading bar (default: false) */
    silence?: boolean;
    /** If true, requests will be synchronous (default: false) */
    sync?: boolean;
  };
}
