import { DnnServicesFramework } from '@dnncommunity/dnn-elements';
export class ClientBase {

  private sf: DnnServicesFramework;
  private moduleId: number;

  constructor(configuration: ConfigureRequest) {
    this.moduleId = configuration.moduleId;
    this.sf = new DnnServicesFramework(this.moduleId);
  }

  protected getBaseUrl(_defaultUrl: string, baseUrl?: string): string {
    baseUrl = this.sf.getServiceRoot("$ext_packagename$");

    // Strips the last / if present for future concatenations
    baseUrl = baseUrl.replace(/\/$/, "");

    return baseUrl || "";
  }

  protected async transformOptions(options: RequestInit): Promise<RequestInit> {
    const dnnHeaders = this.sf.getModuleHeaders();

    let headers: Headers;
    if (!options.headers) {
      headers = new Headers();
    } else if (options.headers instanceof Headers) {
      headers = options.headers;
    } else if (Array.isArray(options.headers)) {
      headers = new Headers(options.headers);
    } else {
      headers = new Headers(Object.entries(options.headers));
    }

    dnnHeaders.forEach((value, key) => {
      headers.append(key, value);
    });

    options.headers = headers;

    return options;
  }
}

export interface ConfigureRequest {
  moduleId: number;
} 
